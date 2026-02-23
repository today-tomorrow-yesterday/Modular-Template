using Modules.Sales.Domain.FundingCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.FundingCache.UpsertFundingRequestCache;

public sealed record UpsertFundingRequestCacheCommand(FundingRequestCache Cache) : ICommand;
