using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.RetailLocations.UpsertRetailLocation;

public sealed record UpsertRetailLocationCommand(
    int HomeCenterNumber,
    string Name,
    string StateCode,
    string Zip,
    bool IsActive) : ICommand;
