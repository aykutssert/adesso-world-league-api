using AdessoWorldLeague.Domain.Abstractions;
using AdessoWorldLeague.Domain.Common;
using AdessoWorldLeague.Domain.Exceptions;
using AdessoWorldLeague.Domain.Teams;

namespace AdessoWorldLeague.Domain.Draws.Engine;

/// <summary>
/// Draws the league in the round-robin order required by the challenge: one team is drawn into group A,
/// then one into group B, and so on; once every group has received a team the pointer returns to group A
/// for the next round. This repeats until every team has a group. A group may never hold two teams from
/// the same country.
/// </summary>
/// <remarks>
/// <para>
/// <b>The trap.</b> Picking a random legal team at every step satisfies the country rule locally but can
/// strand the draw: the last groups end up facing a pool that consists only of countries they already
/// contain, and there is no legal pick left. It does not happen on every run, which is what makes it
/// dangerous - the naive engine passes a casual manual test and fails in production. The fuzz tests in
/// this repository reproduce it within a few hundred seeds.
/// </para>
/// <para>
/// <b>The fix.</b> Before a pick is accepted, this engine asks whether the <i>rest</i> of the draw is still
/// completable with that team removed from the pool (see <see cref="DrawFeasibility"/>, a small max-flow
/// check). Candidates are tried in random order and the first one that keeps the draw solvable is taken.
/// </para>
/// <para>
/// <b>Why that always terminates.</b> The starting position is solvable, and if a position is solvable then
/// some complete solution exists; the country that solution assigns to the group currently being filled is
/// one of the candidates, and choosing it leaves the remainder of that very solution intact. So at every
/// step at least one candidate passes the check, and no backtracking is ever required.
/// </para>
/// <para>
/// <b>Fairness.</b> Because the candidates are shuffled before they are tried, the engine picks uniformly
/// among the countries that keep the draw solvable, and the team taken from the chosen country is drawn
/// from a shuffled stack. Nothing about the order of the seed data leaks into the result.
/// </para>
/// </remarks>
public sealed class RoundRobinDrawEngine : IGroupDrawEngine
{
    /// <inheritdoc />
    public DrawPlan Execute(GroupCount groupCount, IReadOnlyCollection<Team> teamPool, IRandomSource randomSource)
    {
        ArgumentNullException.ThrowIfNull(teamPool);
        ArgumentNullException.ThrowIfNull(randomSource);

        EnsurePoolIsDrawable(teamPool);

        var state = DrawState.Create(groupCount, teamPool, randomSource);

        // Outer loop = the round-robin round, inner loop = the groups in order A, B, C, ...
        // Keeping this shape makes the draw order required by the challenge visible in the code itself.
        for (var round = 0; round < groupCount.TeamsPerGroup; round++)
        {
            for (var groupIndex = 0; groupIndex < groupCount.Value; groupIndex++)
            {
                state.DrawNextTeamInto(groupIndex, round, randomSource);
            }
        }

        var plan = state.ToPlan();

        // Belt and braces: the engine never returns a plan it cannot prove correct.
        DrawPlanGuard.EnsureValid(plan, groupCount, teamPool);

        return plan;
    }

    private static void EnsurePoolIsDrawable(IReadOnlyCollection<Team> teamPool)
    {
        if (teamPool.Count != LeagueRules.TotalTeams)
        {
            throw new InvalidTeamPoolException(
                $"The league requires exactly {LeagueRules.TotalTeams} teams but the pool contains {teamPool.Count}.");
        }

        if (teamPool.Select(team => team.Id).Distinct().Count() != teamPool.Count)
        {
            throw new InvalidTeamPoolException("The team pool contains duplicate teams.");
        }

        var countries = teamPool.GroupBy(team => team.CountryId).ToList();

        if (countries.Count != LeagueRules.CountryCount)
        {
            throw new InvalidTeamPoolException(
                $"The league requires exactly {LeagueRules.CountryCount} countries but the pool contains {countries.Count}.");
        }

        var unbalanced = countries.FirstOrDefault(country => country.Count() != LeagueRules.TeamsPerCountry);

        if (unbalanced is not null)
        {
            throw new InvalidTeamPoolException(
                $"Country '{unbalanced.Key}' fields {unbalanced.Count()} teams but exactly {LeagueRules.TeamsPerCountry} are required.");
        }
    }

    /// <summary>Mutable bookkeeping for a single run of the engine.</summary>
    private sealed class DrawState
    {
        private readonly GroupCount _groupCount;
        private readonly Guid[] _countryIds;
        private readonly Stack<Guid>[] _teamsByCountry;
        private readonly int[] _remainingTeamsByCountry;
        private readonly int[] _freeSlotsByGroup;
        private readonly bool[,] _isCountryAllowedInGroup;
        private readonly List<DrawPlanSlot>[] _slotsByGroup;
        private readonly int[] _candidateBuffer;

        private int _pickNumber;

        private DrawState(
            GroupCount groupCount,
            Guid[] countryIds,
            Stack<Guid>[] teamsByCountry)
        {
            _groupCount = groupCount;
            _countryIds = countryIds;
            _teamsByCountry = teamsByCountry;
            _remainingTeamsByCountry = [.. teamsByCountry.Select(teams => teams.Count)];
            _freeSlotsByGroup = [.. Enumerable.Repeat(groupCount.TeamsPerGroup, groupCount.Value)];
            _isCountryAllowedInGroup = new bool[countryIds.Length, groupCount.Value];
            _slotsByGroup = [.. Enumerable.Range(0, groupCount.Value).Select(_ => new List<DrawPlanSlot>())];
            _candidateBuffer = new int[countryIds.Length];

            for (var country = 0; country < countryIds.Length; country++)
            {
                for (var group = 0; group < groupCount.Value; group++)
                {
                    _isCountryAllowedInGroup[country, group] = true;
                }
            }
        }

        public static DrawState Create(
            GroupCount groupCount,
            IReadOnlyCollection<Team> teamPool,
            IRandomSource randomSource)
        {
            var byCountry = teamPool
                .GroupBy(team => team.CountryId)
                .Select(country => (
                    CountryId: country.Key,
                    Teams: new Stack<Guid>(Shuffle([.. country.Select(team => team.Id)], randomSource))))
                .ToList();

            return new DrawState(
                groupCount,
                [.. byCountry.Select(country => country.CountryId)],
                [.. byCountry.Select(country => country.Teams)]);
        }

        /// <summary>Draws one team into the given group, keeping the rest of the draw solvable.</summary>
        public void DrawNextTeamInto(int groupIndex, int round, IRandomSource randomSource)
        {
            var candidateCount = CollectCandidates(groupIndex);

            if (candidateCount == 0)
            {
                throw new DrawInfeasibleException(
                    $"No remaining team can be placed into group '{LeagueRules.GroupLabels[groupIndex]}' " +
                    "without repeating a country.");
            }

            Shuffle(_candidateBuffer.AsSpan(0, candidateCount), randomSource);

            for (var i = 0; i < candidateCount; i++)
            {
                var countryIndex = _candidateBuffer[i];

                Assign(countryIndex, groupIndex);

                if (DrawFeasibility.CanComplete(_remainingTeamsByCountry, _freeSlotsByGroup, _isCountryAllowedInGroup))
                {
                    var teamId = _teamsByCountry[countryIndex].Pop();

                    _slotsByGroup[groupIndex].Add(
                        new DrawPlanSlot(teamId, _countryIds[countryIndex], round, ++_pickNumber));

                    return;
                }

                Revert(countryIndex, groupIndex);
            }

            // Unreachable for a valid pool: the argument in the class remarks guarantees a solvable candidate.
            throw new DrawInfeasibleException(
                $"Every candidate for group '{LeagueRules.GroupLabels[groupIndex]}' would strand the draw. " +
                "This indicates a bug in the draw engine.");
        }

        public DrawPlan ToPlan() =>
            new(_groupCount,
                [.. Enumerable
                    .Range(0, _groupCount.Value)
                    .Select(groupIndex => new DrawPlanGroup(
                        LeagueRules.GroupLabels[groupIndex],
                        groupIndex,
                        _slotsByGroup[groupIndex]))]);

        private int CollectCandidates(int groupIndex)
        {
            var count = 0;

            for (var countryIndex = 0; countryIndex < _countryIds.Length; countryIndex++)
            {
                if (_remainingTeamsByCountry[countryIndex] > 0 && _isCountryAllowedInGroup[countryIndex, groupIndex])
                {
                    _candidateBuffer[count++] = countryIndex;
                }
            }

            return count;
        }

        private void Assign(int countryIndex, int groupIndex)
        {
            _remainingTeamsByCountry[countryIndex]--;
            _freeSlotsByGroup[groupIndex]--;
            _isCountryAllowedInGroup[countryIndex, groupIndex] = false;
        }

        private void Revert(int countryIndex, int groupIndex)
        {
            _remainingTeamsByCountry[countryIndex]++;
            _freeSlotsByGroup[groupIndex]++;
            _isCountryAllowedInGroup[countryIndex, groupIndex] = true;
        }

        private static Guid[] Shuffle(Guid[] values, IRandomSource randomSource)
        {
            for (var i = values.Length - 1; i > 0; i--)
            {
                var j = randomSource.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }

            return values;
        }

        private static void Shuffle(Span<int> values, IRandomSource randomSource)
        {
            for (var i = values.Length - 1; i > 0; i--)
            {
                var j = randomSource.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }
}
