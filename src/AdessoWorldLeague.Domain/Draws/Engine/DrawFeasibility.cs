namespace AdessoWorldLeague.Domain.Draws.Engine;

/// <summary>
/// Answers one question for the draw engine: "if the draw continues from here, can it still be finished
/// legally?" It is what turns the draw from a hopeful greedy walk into an algorithm that provably never
/// gets stuck.
/// </summary>
/// <remarks>
/// <para>
/// The remaining part of a draw is a bipartite flow problem. Every country still holds a number of teams
/// that must be placed; every group still has a number of free slots; and a country may occupy at most one
/// slot of a given group (never two, and none at all if that group already holds one of its teams).
/// </para>
/// <para>
/// Modelling it as a flow network - source → country (capacity: teams left), country → group (capacity 1
/// when allowed), group → sink (capacity: free slots) - the draw can be completed exactly when the maximum
/// flow saturates every country, i.e. equals the number of teams still waiting. This is the constructive
/// side of Hall's theorem for degree-constrained bipartite graphs.
/// </para>
/// <para>
/// The network is tiny (at most eight countries and eight groups), so the check costs microseconds and is
/// run before every single pick.
/// </para>
/// </remarks>
internal static class DrawFeasibility
{
    private const int MaxCountries = 8;
    private const int MaxGroups = 8;
    private const int MaxNodes = MaxCountries + MaxGroups + 2;

    /// <summary>
    /// Returns whether the remaining teams can still be distributed over the remaining group slots
    /// without ever putting two teams of the same country into one group.
    /// </summary>
    /// <param name="remainingTeamsByCountry">Teams still waiting for a group, indexed by country.</param>
    /// <param name="freeSlotsByGroup">Free slots per group, indexed by group.</param>
    /// <param name="isCountryAllowedInGroup">
    /// <c>true</c> when the country (first index) may still be placed into the group (second index).
    /// </param>
    public static bool CanComplete(
        ReadOnlySpan<int> remainingTeamsByCountry,
        ReadOnlySpan<int> freeSlotsByGroup,
        bool[,] isCountryAllowedInGroup)
    {
        var countryCount = remainingTeamsByCountry.Length;
        var groupCount = freeSlotsByGroup.Length;
        var nodeCount = countryCount + groupCount + 2;

        // The network is built on the stack, so its size is fixed at compile time. If the league ever
        // grows beyond that, fail with a sentence that says so rather than an index-out-of-range.
        if (nodeCount > MaxNodes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(remainingTeamsByCountry),
                $"The feasibility check supports at most {MaxCountries} countries and {MaxGroups} groups; " +
                $"got {countryCount} and {groupCount}.");
        }

        var source = 0;
        var sink = nodeCount - 1;

        var teamsToPlace = 0;

        Span<int> capacity = stackalloc int[MaxNodes * MaxNodes];
        capacity.Clear();

        for (var country = 0; country < countryCount; country++)
        {
            var remaining = remainingTeamsByCountry[country];
            teamsToPlace += remaining;
            capacity[Index(source, 1 + country, nodeCount)] = remaining;

            for (var group = 0; group < groupCount; group++)
            {
                if (isCountryAllowedInGroup[country, group])
                {
                    capacity[Index(1 + country, 1 + countryCount + group, nodeCount)] = 1;
                }
            }
        }

        for (var group = 0; group < groupCount; group++)
        {
            capacity[Index(1 + countryCount + group, sink, nodeCount)] = freeSlotsByGroup[group];
        }

        return teamsToPlace == 0 || MaxFlow(capacity, nodeCount, source, sink) == teamsToPlace;
    }

    /// <summary>Edmonds-Karp maximum flow. The network never exceeds eighteen nodes.</summary>
    private static int MaxFlow(Span<int> capacity, int nodeCount, int source, int sink)
    {
        Span<int> parents = stackalloc int[MaxNodes];
        Span<int> queue = stackalloc int[MaxNodes];

        var flow = 0;

        while (true)
        {
            parents.Fill(-1);
            parents[source] = source;

            var head = 0;
            var tail = 0;
            queue[tail++] = source;

            while (head < tail && parents[sink] < 0)
            {
                var current = queue[head++];

                for (var next = 0; next < nodeCount; next++)
                {
                    if (parents[next] < 0 && capacity[Index(current, next, nodeCount)] > 0)
                    {
                        parents[next] = current;
                        queue[tail++] = next;
                    }
                }
            }

            if (parents[sink] < 0)
            {
                return flow;
            }

            // Find the bottleneck of the augmenting path, then push that much flow along it.
            var bottleneck = int.MaxValue;

            for (var node = sink; node != source; node = parents[node])
            {
                bottleneck = Math.Min(bottleneck, capacity[Index(parents[node], node, nodeCount)]);
            }

            for (var node = sink; node != source; node = parents[node])
            {
                capacity[Index(parents[node], node, nodeCount)] -= bottleneck;
                capacity[Index(node, parents[node], nodeCount)] += bottleneck;
            }

            flow += bottleneck;
        }
    }

    private static int Index(int from, int to, int nodeCount) => (from * nodeCount) + to;
}
