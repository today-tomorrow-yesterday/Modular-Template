using Microsoft.Extensions.Logging.Abstractions;
using ModularTemplate.Infrastructure.Secrets;
using Xunit;

namespace ModularTemplate.Infrastructure.Tests.Secrets;

public class DevelopmentSecretProviderTests
{
    private readonly DevelopmentSecretProvider _sut = new(
        NullLogger<DevelopmentSecretProvider>.Instance);

    [Fact]
    public async Task GetSecretAsync_String_Returns_Empty()
    {
        var result = await _sut.GetSecretAsync<string>("any-secret");

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetSecretAsync_Typed_Throws_InvalidOperation()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetSecretAsync<TestSecret>("any-secret"));
    }

    private sealed class TestSecret
    {
        public string Value { get; set; } = string.Empty;
    }
}
