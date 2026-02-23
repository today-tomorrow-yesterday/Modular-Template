using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.LandInventory.RemoveLandParcelFromCdc;

public sealed record RemoveLandParcelFromCdcCommand(int LandParcelId) : ICommand;
