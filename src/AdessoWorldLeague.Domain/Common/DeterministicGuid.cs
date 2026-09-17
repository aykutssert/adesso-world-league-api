using System.Security.Cryptography;
using System.Text;

namespace AdessoWorldLeague.Domain.Common;

/// <summary>
/// Produces RFC 4122 version 5 (name based, SHA-1) identifiers.
/// </summary>
/// <remarks>
/// Seed data needs identifiers that are identical on every machine and across every migration run;
/// deriving them from a namespace and a stable name gives exactly that, without hard-coding
/// forty opaque GUID literals into the codebase.
/// </remarks>
public static class DeterministicGuid
{
    /// <summary>Namespace used for every identifier of this application's seed data.</summary>
    public static readonly Guid SeedNamespace = new("6f9b2f4c-1f6c-4a3c-9a0d-0f1a2b3c4d5e");

    /// <summary>Creates a deterministic identifier from a namespace and a name.</summary>
    /// <param name="namespaceId">Namespace the name lives in.</param>
    /// <param name="name">Stable name, for example "country:TR".</param>
    public static Guid Create(Guid namespaceId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var namespaceBytes = namespaceId.ToByteArray(bigEndian: true);
        var nameBytes = Encoding.UTF8.GetBytes(name);

        Span<byte> buffer = stackalloc byte[namespaceBytes.Length + nameBytes.Length];
        namespaceBytes.CopyTo(buffer);
        nameBytes.CopyTo(buffer[namespaceBytes.Length..]);

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(buffer, hash);

        Span<byte> guidBytes = hash[..16];
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50); // version 5
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // RFC 4122 variant

        return new Guid(guidBytes, bigEndian: true);
    }

    /// <summary>Creates a deterministic identifier inside the seed-data namespace.</summary>
    public static Guid ForSeed(string name) => Create(SeedNamespace, name);
}
