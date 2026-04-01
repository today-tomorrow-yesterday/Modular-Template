namespace ModularTemplate.Presentation.Endpoints;

/// <summary>
/// Attach to a minimal API endpoint via .WithMetadata(...) to provide a Swagger request body example.
/// </summary>
public sealed record RequestBodyExample(string Json);
