using Rtl.Core.Domain.Entities;
using System.Text.Json;

namespace Modules.Sales.Domain.Packages.Details;

public enum SalesTeamRole
{
    Primary,
    Secondary
}

public sealed class SalesTeamDetails : IVersionedDetails
{
    public int SchemaVersion => 1;
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public List<SalesTeamMember> SalesTeamMembers { get; private set; } = [];

    private SalesTeamDetails() { }

    public static SalesTeamDetails Create(List<SalesTeamMember> members) => new() { SalesTeamMembers = members.ToList() };
}

public sealed class SalesTeamMember
{
    public int? AuthorizedUserId { get; private set; }
    public SalesTeamRole Role { get; private set; }
    public decimal? CommissionSplitPercentage { get; private set; }
    public decimal CommissionAmount { get; private set; } // Populated by commission calculation journey
    public string? EmployeeName { get; private set; } // DisplayName from AuthorizedUserCache
    public int? EmployeeNumber { get; private set; } // iSeries employee ID

    public static SalesTeamMember Create(
        int? authorizedUserId,
        SalesTeamRole role,
        decimal? commissionSplitPercentage,
        string? employeeName = null,
        int? employeeNumber = null)
    {
        return new SalesTeamMember
        {
            AuthorizedUserId = authorizedUserId,
            Role = role,
            CommissionSplitPercentage = commissionSplitPercentage,
            CommissionAmount = 0m,
            EmployeeName = employeeName,
            EmployeeNumber = employeeNumber
        };
    }
}
