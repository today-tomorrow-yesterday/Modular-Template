using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.RetailLocationCache.UpsertRetailLocationCache;

public sealed record UpsertRetailLocationCacheCommand(
    int HomeCenterNumber,
    string Name,
    string StateCode,
    string Zip,
    bool IsActive) : ICommand;
