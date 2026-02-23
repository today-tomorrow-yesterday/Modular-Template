using Rtl.Core.Domain;

namespace Modules.Customer.Domain.SalesPersons;

public interface ISalesPersonRepository : IReadRepository<SalesPerson, string>
{
    void Add(SalesPerson salesPerson);

    void Update(SalesPerson salesPerson);
}
