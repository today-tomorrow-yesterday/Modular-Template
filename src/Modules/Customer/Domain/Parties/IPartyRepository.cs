using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Domain;

namespace Modules.Customer.Domain.Parties;

public interface IPartyRepository : IRepository<Party, int>
{
    Task<Party?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Party?> GetByIdentifierAsync(IdentifierType type, string value, CancellationToken cancellationToken = default);

    Task<Party?> GetForUpdateByIdentifierAsync(IdentifierType type, string value, CancellationToken cancellationToken = default);

    Task<Party?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
