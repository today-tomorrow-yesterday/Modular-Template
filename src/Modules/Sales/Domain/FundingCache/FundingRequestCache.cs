using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Domain.Caching;
using System.Text.Json;

namespace Modules.Sales.Domain.FundingCache;

public enum FundingRequestStatus
{
    Pending,
    Approved,
    Funded,
    Denied,
    Canceled
}

// ECST Cache Entity — cache.funding. Populated by Funding events (FundingRequestSubmitted/StatusChanged).
// Uses JSONB FundingKeys for multi-lender abstraction — AppId/LoanId/AppRefCode stored as key-value pairs.
// CRITICAL: AppId or any lender-specific key MUST NEVER appear as a typed property.
// Lender identifiers exist only inside FundingKeys JSONB. The iSeries adapter is the ONLY place
// that extracts specific keys. All other Sales logic works with FundingRequestId and generic FundingKeys.
public sealed class FundingRequestCache : ICacheProjection
{
    public int Id { get; set; }

    public int RefFundingRequestId { get; set; }

    public int SaleId { get; set; }

    public int PackageId { get; set; }

    public JsonDocument? FundingKeys { get; set; } // [{"Key":"AppId","Value":"999999"},{"Key":"LoanId","Value":"VMF-2026-12345"}]

    public int LenderId { get; set; } // 0 = unassigned

    public string? LenderName { get; set; }

    public FundingRequestStatus Status { get; set; }

    public decimal RequestAmount { get; set; } // Funding request total

    public DateTimeOffset? ApprovalDate { get; set; }

    public DateTimeOffset? ApprovalExpirationDate { get; set; }

    public DateTime LastSyncedAtUtc { get; set; }

    // Navigation properties
    public Sale Sale { get; set; } = null!;

    public Package Package { get; set; } = null!;

    public void ApplyChangesFrom(FundingRequestCache incoming)
    {
        SaleId = incoming.SaleId;
        PackageId = incoming.PackageId;
        FundingKeys = incoming.FundingKeys;
        RequestAmount = incoming.RequestAmount;
        LenderId = incoming.LenderId;
        LenderName = incoming.LenderName;
        Status = incoming.Status;
        ApprovalDate = incoming.ApprovalDate;
        ApprovalExpirationDate = incoming.ApprovalExpirationDate;
        LastSyncedAtUtc = incoming.LastSyncedAtUtc;
    }
}
