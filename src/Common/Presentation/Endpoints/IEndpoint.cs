using Microsoft.AspNetCore.Routing;

namespace Rtl.Core.Presentation.Endpoints;

/// <summary>
/// Interface for individual endpoint handlers.
/// Each endpoint class handles a single HTTP operation (GET, POST, PUT, DELETE, etc.).
/// </summary>
/// <remarks>
/// This follows the same pattern as CQRS handlers where each command/query has its own class.
/// Example folder structure:
/// <code>
/// SampleItems/
/// ├── GetAllSampleItems/
/// │   └── GetAllSampleItemsEndpoint.cs
/// ├── GetSampleItemById/
/// │   └── GetSampleItemByIdEndpoint.cs
/// ├── CreateSampleItem/
/// │   └── CreateSampleItemEndpoint.cs
/// │   └── CreateSampleItemRequest.cs
/// └── SampleItemsEndpoints.cs  (composes all endpoints)
/// </code>
/// </remarks>
public interface IEndpoint
{
    /// <summary>
    /// Maps this endpoint to the route group.
    /// The route group already has the resource prefix applied (e.g., "/sample-items").
    /// </summary>
    /// <param name="group">The route group builder with resource prefix applied.</param>
    void MapEndpoint(RouteGroupBuilder group);
}
