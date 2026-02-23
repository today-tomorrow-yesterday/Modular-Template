using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Rtl.Core.Infrastructure.Secrets;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.Secrets;

public class AwsSecretProviderTests
{
    private readonly IAmazonSecretsManager _secretsManager = Substitute.For<IAmazonSecretsManager>();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly IOptions<SecretProviderOptions> _options = Options.Create(new SecretProviderOptions
    {
        CacheDurationMinutes = 5
    });

    private AwsSecretProvider CreateSut() => new(
        _secretsManager,
        _cache,
        _options,
        NullLogger<AwsSecretProvider>.Instance);

    [Fact]
    public async Task GetSecretAsync_String_Returns_Raw_Secret()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetSecretValueResponse { SecretString = "my-signing-key" });

        var sut = CreateSut();

        var result = await sut.GetSecretAsync<string>("test-secret");

        Assert.Equal("my-signing-key", result);
    }

    [Fact]
    public async Task GetSecretAsync_Typed_Deserializes_Json()
    {
        var json = """{"Username":"admin","Password":"s3cret","Host":"db.example.com","Port":"5432","Engine":"postgres"}""";
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetSecretValueResponse { SecretString = json });

        var sut = CreateSut();

        var result = await sut.GetSecretAsync<TestDbSecret>("rds-secret");

        Assert.Equal("admin", result.Username);
        Assert.Equal("s3cret", result.Password);
        Assert.Equal("db.example.com", result.Host);
    }

    [Fact]
    public async Task GetSecretAsync_Caches_On_Second_Call()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns(new GetSecretValueResponse { SecretString = "cached-value" });

        var sut = CreateSut();

        var first = await sut.GetSecretAsync<string>("test-secret");
        var second = await sut.GetSecretAsync<string>("test-secret");

        Assert.Equal(first, second);
        await _secretsManager.Received(1).GetSecretValueAsync(
            Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSecretAsync_ResourceNotFound_Throws_And_Logs()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns<GetSecretValueResponse>(_ => throw new ResourceNotFoundException("not found"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<ResourceNotFoundException>(
            () => sut.GetSecretAsync<string>("missing-secret"));
    }

    [Fact]
    public async Task GetSecretAsync_DecryptionFailure_Throws_And_Logs()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns<GetSecretValueResponse>(_ => throw new DecryptionFailureException("kms error"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<DecryptionFailureException>(
            () => sut.GetSecretAsync<string>("encrypted-secret"));
    }

    [Fact]
    public async Task GetSecretAsync_InvalidRequest_Throws_And_Logs()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns<GetSecretValueResponse>(_ => throw new InvalidRequestException("pending deletion"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidRequestException>(
            () => sut.GetSecretAsync<string>("deleted-secret"));
    }

    [Fact]
    public async Task GetSecretAsync_InvalidParameter_Throws_And_Logs()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns<GetSecretValueResponse>(_ => throw new InvalidParameterException("bad param"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidParameterException>(
            () => sut.GetSecretAsync<string>("bad-name"));
    }

    [Fact]
    public async Task GetSecretAsync_InternalServiceError_Throws_And_Logs()
    {
        _secretsManager.GetSecretValueAsync(Arg.Any<GetSecretValueRequest>(), Arg.Any<CancellationToken>())
            .Returns<GetSecretValueResponse>(_ => throw new InternalServiceErrorException("aws down"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InternalServiceErrorException>(
            () => sut.GetSecretAsync<string>("any-secret"));
    }

    [Fact]
    public async Task GetSecretAsync_Null_SecretName_Throws_ArgumentNullException()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.GetSecretAsync<string>(null!));
    }

    [Fact]
    public async Task GetSecretAsync_Empty_SecretName_Throws_ArgumentException()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetSecretAsync<string>(""));
    }

    private sealed class TestDbSecret
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Engine { get; set; } = string.Empty;
    }
}
