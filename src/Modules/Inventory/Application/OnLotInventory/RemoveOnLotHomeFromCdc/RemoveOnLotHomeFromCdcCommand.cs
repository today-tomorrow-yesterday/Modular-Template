using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.OnLotInventory.RemoveOnLotHomeFromCdc;

public sealed record RemoveOnLotHomeFromCdcCommand(int OnLotHomeId) : ICommand;
