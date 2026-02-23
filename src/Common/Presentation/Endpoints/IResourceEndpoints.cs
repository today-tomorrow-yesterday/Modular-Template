using Microsoft.AspNetCore.Routing;

namespace Rtl.Core.Presentation.Endpoints;

/// <summary>
/// Interface for defining endpoints for a specific resource within a module.
/// Each resource (like a controller) implements this interface and composes individual IEndpoint implementations.
/// </summary>
public interface IResourceEndpoints
{
    void MapEndpoints(RouteGroupBuilder group);
}

/// <summary>
/// Base class for resource endpoints that provides common endpoint mapping functionality.
/// </summary>
public abstract class ResourceEndpoints : IResourceEndpoints
{
    protected abstract IEndpoint[] Endpoints { get; }

    public void MapEndpoints(RouteGroupBuilder group)
    {
        foreach (var endpoint in Endpoints)
        {
            endpoint.MapEndpoint(group);
        }
    }
}
