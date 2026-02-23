using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.Parties.Errors;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Customer.Application.Parties.GetPartyByPublicId;

// Flow: GET /api/v1/parties/{publicId} → query Customer.parties
internal sealed class GetPartyByPublicIdQueryHandler(
    IPartyRepository partyRepository)
    : IQueryHandler<GetPartyByPublicIdQuery, PartyResponse>
{
    public async Task<Result<PartyResponse>> Handle(
        GetPartyByPublicIdQuery request,
        CancellationToken cancellationToken)
    {
        var party = await partyRepository.GetByPublicIdAsync(
            request.PublicId,
            cancellationToken);

        if (party is null)
        {
            return Result.Failure<PartyResponse>(
                PartyErrors.NotFoundByPublicId(request.PublicId));
        }

        return PartyResponseMapper.MapToResponse(party);
    }
}
