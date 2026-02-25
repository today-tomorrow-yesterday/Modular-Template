using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Details;

public enum CreditType
{
    DownPayment,
    Concessions
}

public sealed class CreditDetails : IVersionedDetails
{
    private CreditDetails() { }

    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public CreditType CreditType { get; private set; }

    public static CreditDetails Create(CreditType creditType) => new() { CreditType = creditType };
}
