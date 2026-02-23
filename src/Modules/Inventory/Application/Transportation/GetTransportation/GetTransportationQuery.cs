using Rtl.Core.Application.Messaging;

namespace Modules.Inventory.Application.Transportation.GetTransportation;

public sealed record GetTransportationQuery(decimal Length, decimal Width) : IQuery<TransportationResponse>;

public sealed record TransportationResponse(
    int NumberOfAxles,
    int NumberOfWheels);
