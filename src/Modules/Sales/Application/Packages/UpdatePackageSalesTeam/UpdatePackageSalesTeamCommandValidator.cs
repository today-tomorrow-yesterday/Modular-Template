using FluentValidation;
using Modules.Sales.Domain.Packages.SalesTeam;

namespace Modules.Sales.Application.Packages.UpdatePackageSalesTeam;

internal sealed class UpdatePackageSalesTeamCommandValidator
    : AbstractValidator<UpdatePackageSalesTeamCommand>
{
    public UpdatePackageSalesTeamCommandValidator()
    {
        RuleFor(x => x.PackagePublicId)
            .NotEmpty();

        RuleFor(x => x.Members)
            .NotEmpty()
            .Must(m => m.Length <= 2)
            .WithMessage("Sales team cannot have more than 2 members.");

        RuleFor(x => x.Members)
            .Must(HaveExactlyOnePrimary)
            .WithMessage("Sales team must have exactly one Primary member.")
            .When(x => x.Members.Length > 0);

        RuleFor(x => x.Members)
            .Must(HaveNoDuplicateRoles)
            .WithMessage("Sales team members must have unique roles.")
            .When(x => x.Members.Length > 0);

        RuleFor(x => x.Members)
            .Must(HaveConsistentSplitPercentages)
            .WithMessage("If any member has a commission split percentage, all members must have one.")
            .When(x => x.Members.Length > 0);

        RuleFor(x => x.Members)
            .Must(HaveSplitPercentagesSumTo100)
            .WithMessage("Commission split percentages must sum to 100.")
            .When(x => x.Members.Length > 0 && x.Members.All(m => m.CommissionSplitPercentage.HasValue));

        RuleForEach(x => x.Members).ChildRules(member =>
        {
            member.RuleFor(m => m.Role)
                .IsInEnum();

            member.RuleFor(m => m.CommissionSplitPercentage)
                .InclusiveBetween(0m, 100m)
                .When(m => m.CommissionSplitPercentage.HasValue);
        });
    }

    private static bool HaveExactlyOnePrimary(UpdatePackageSalesTeamMemberRequest[] members) =>
        members.Count(m => m.Role == SalesTeamRole.Primary) == 1;

    private static bool HaveNoDuplicateRoles(UpdatePackageSalesTeamMemberRequest[] members) =>
        members.Select(m => m.Role).Distinct().Count() == members.Length;

    private static bool HaveConsistentSplitPercentages(UpdatePackageSalesTeamMemberRequest[] members)
    {
        var hasSplit = members.Where(m => m.CommissionSplitPercentage.HasValue).ToList();
        return hasSplit.Count == 0 || hasSplit.Count == members.Length;
    }

    private static bool HaveSplitPercentagesSumTo100(UpdatePackageSalesTeamMemberRequest[] members) =>
        members.Sum(m => m.CommissionSplitPercentage!.Value) == 100m;
}
