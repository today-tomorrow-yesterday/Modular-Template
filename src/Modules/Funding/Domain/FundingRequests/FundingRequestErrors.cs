using Rtl.Core.Domain.Results;

namespace Modules.Funding.Domain.FundingRequests;

public static class FundingRequestErrors
{
    public static Error NotFound(int id) => Error.NotFound(
        "FundingRequest.NotFound",
        $"The funding request with Id = '{id}' was not found");
}
