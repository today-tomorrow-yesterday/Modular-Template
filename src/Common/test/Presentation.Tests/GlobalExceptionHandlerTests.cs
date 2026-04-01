using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ModularTemplate.Presentation.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Returns500StatusCode()
    {
        var logger = new NullLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        await handler.TryHandleAsync(httpContext, new Exception("Test"), CancellationToken.None);

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_ReturnsTrue()
    {
        var logger = new NullLogger<GlobalExceptionHandler>();
        var handler = new GlobalExceptionHandler(logger);
        var httpContext = new DefaultHttpContext { Response = { Body = new MemoryStream() } };

        var result = await handler.TryHandleAsync(httpContext, new Exception("Test"), CancellationToken.None);

        Assert.True(result);
    }

    private sealed class NullLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
