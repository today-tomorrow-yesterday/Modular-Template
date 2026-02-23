using FluentValidation;
using Modules.Customer.Domain.Parties.Enums;

namespace Modules.Customer.Application.Parties.SyncPartyFromCrm;

internal sealed class SyncPartyFromCrmCommandValidator : AbstractValidator<SyncPartyFromCrmCommand>
{
    public SyncPartyFromCrmCommandValidator()
    {
        RuleFor(x => x.PartyId)
            .GreaterThan(0)
            .WithMessage("PartyId is required");

        RuleFor(x => x.HomeCenterNumber)
            .GreaterThan(0)
            .WithMessage("HomeCenterNumber is required");

        RuleFor(x => x.PartyType)
            .IsInEnum()
            .WithMessage("PartyType must be Person or Organization");

        RuleFor(x => x.LifecycleStage)
            .IsInEnum()
            .WithMessage("LifecycleStage must be Lead, Opportunity, or Customer");

        RuleFor(x => x.PersonData)
            .NotNull()
            .When(x => x.PartyType == PartyType.Person)
            .WithMessage("PersonData is required for Person parties");

        RuleFor(x => x.OrganizationData)
            .NotNull()
            .When(x => x.PartyType == PartyType.Organization)
            .WithMessage("OrganizationData is required for Organization parties");
    }
}
