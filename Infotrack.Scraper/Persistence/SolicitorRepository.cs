using System.Security.Cryptography;
using System.Text;
using CSharpFunctionalExtensions;
using Dapper;
using Infotrack.Scraper.Conveyancing;
using Npgsql;

namespace Infotrack.Scraper.Persistence;

internal sealed class SolicitorRepository(NpgsqlDataSource dataSource) : ISolicitorRepository
{
    public async Task<Maybe<IReadOnlyList<SolicitorRecord>>> GetByLocationAsync(
        string location, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);

        var locationId = await conn.QuerySingleOrDefaultAsync<long?>(
            "SELECT id FROM locations WHERE name = @Location",
            new { Location = location });

        if (locationId is null)
            return Maybe.None;

        var solicitors = (await conn.QueryAsync<SolicitorRecord>(
            """
            SELECT solicitor_key AS SolicitorKey,
                   location_id   AS LocationId,
                   name          AS Name,
                   address       AS Address,
                   phone         AS Phone,
                   description   AS Description,
                   website       AS Website
            FROM solicitors
            WHERE location_id = @LocationId
            """,
            new { LocationId = locationId })).AsList();

        return Maybe.From<IReadOnlyList<SolicitorRecord>>(solicitors);
    }

    public async Task UpsertAsync(
        string location, IEnumerable<Solicitor> solicitors, CancellationToken ct = default)
    {
        await using var conn = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        var locationId = await conn.QuerySingleAsync<long>(
            """
            INSERT INTO locations (name, last_updated)
            VALUES (@Name, NOW())
            ON CONFLICT (name) DO UPDATE SET last_updated = NOW()
            RETURNING id
            """,
            new { Name = location }, tx);

        foreach (var s in solicitors)
        {
            await conn.ExecuteAsync(
                """
                INSERT INTO solicitors (solicitor_key, location_id, name, address, phone, description, website)
                VALUES (@SolicitorKey, @LocationId, @Name, @Address, @Phone, @Description, @Website)
                ON CONFLICT (solicitor_key) DO UPDATE SET
                    address     = EXCLUDED.address,
                    phone       = EXCLUDED.phone,
                    description = EXCLUDED.description,
                    website     = EXCLUDED.website
                """,
                new
                {
                    SolicitorKey = ComputeKey(locationId, s.Name),
                    LocationId = locationId,
                    s.Name,
                    s.Address,
                    s.Phone,
                    s.Description,
                    s.Website
                }, tx);
        }

        await tx.CommitAsync(ct);
    }

    private static string ComputeKey(long locationId, string name) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{locationId}:{name}")));
}