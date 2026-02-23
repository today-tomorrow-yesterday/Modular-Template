using Bogus;
using Modules.Sales.Domain.FundingCache;
using System.Text.Json;

namespace Modules.Sales.Infrastructure.Seeding.Fakers;

internal sealed class FundingRequestCacheFaker : Faker<FundingRequestCache>
{
    private int _id;
    private int _refFundingRequestId;

    public FundingRequestCacheFaker()
    {
        _id = 0;
        _refFundingRequestId = 0;

        RuleFor(f => f.Id, _ => ++_id);
        RuleFor(f => f.RefFundingRequestId, _ => ++_refFundingRequestId);
        // SaleId and PackageId are assigned after generation
        RuleFor(f => f.LenderId, f => f.PickRandom(1, 2, 3));
        RuleFor(f => f.LenderName, f => f.PickRandom("VMF", "21st Mortgage", "Triad Financial"));
        RuleFor(f => f.Status, f => f.PickRandom<FundingRequestStatus>());
        RuleFor(f => f.ApprovalDate, (f, fr) =>
            fr.Status >= FundingRequestStatus.Approved
                ? f.Date.RecentOffset(30).ToUniversalTime()
                : null);
        RuleFor(f => f.ApprovalExpirationDate, (f, fr) =>
            fr.ApprovalDate.HasValue
                ? fr.ApprovalDate.Value.AddDays(90)
                : null);
        RuleFor(f => f.FundingKeys, f => CreateFundingKeys(f));
        RuleFor(f => f.LastSyncedAtUtc, f => f.Date.Recent(30).ToUniversalTime());
    }

    private static JsonDocument CreateFundingKeys(Faker f)
    {
        var keys = new[]
        {
            new { Key = "AppId", Value = f.Random.Int(100000, 999999).ToString() },
            new { Key = "LoanId", Value = $"VMF-2026-{f.Random.Int(10000, 99999)}" }
        };
        return JsonDocument.Parse(JsonSerializer.Serialize(keys));
    }
}
