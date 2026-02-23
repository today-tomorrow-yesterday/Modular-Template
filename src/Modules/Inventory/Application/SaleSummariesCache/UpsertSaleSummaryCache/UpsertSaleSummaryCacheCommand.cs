using Modules.Inventory.Domain.SaleSummariesCache;
using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.SaleSummariesCache.UpsertSaleSummaryCache;

public sealed record UpsertSaleSummaryCacheCommand(SaleSummaryCache Cache) : ICommand;
