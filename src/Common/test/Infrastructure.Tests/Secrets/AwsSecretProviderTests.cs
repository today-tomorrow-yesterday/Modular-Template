using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Rtl.Core.Infrastructure.Secrets;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.Secrets;

public class AwsSecretProviderTests
{
    private readonly FakeSecretsManager _secretsManager = new();
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
        _secretsManager.SecretToReturn = "my-signing-key";
        var sut = CreateSut();

        var result = await sut.GetSecretAsync<string>("test-secret");

        Assert.Equal("my-signing-key", result);
    }

    [Fact]
    public async Task GetSecretAsync_Typed_Deserializes_Json()
    {
        _secretsManager.SecretToReturn = """{"Username":"admin","Password":"s3cret","Host":"db.example.com"}""";
        var sut = CreateSut();

        var result = await sut.GetSecretAsync<TestDbSecret>("rds-secret");

        Assert.Equal("admin", result.Username);
        Assert.Equal("s3cret", result.Password);
        Assert.Equal("db.example.com", result.Host);
    }

    [Fact]
    public async Task GetSecretAsync_Caches_On_Second_Call()
    {
        _secretsManager.SecretToReturn = "cached-value";
        var sut = CreateSut();

        var first = await sut.GetSecretAsync<string>("test-secret");
        var second = await sut.GetSecretAsync<string>("test-secret");

        Assert.Equal(first, second);
        Assert.Equal(1, _secretsManager.CallCount);
    }

    [Fact]
    public async Task GetSecretAsync_ResourceNotFound_Throws()
    {
        _secretsManager.ExceptionToThrow = new ResourceNotFoundException("not found");
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetSecretAsync<string>("missing-secret"));
        Assert.IsType<ResourceNotFoundException>(ex.InnerException);
    }

    [Fact]
    public async Task GetSecretAsync_DecryptionFailure_Throws()
    {
        _secretsManager.ExceptionToThrow = new DecryptionFailureException("kms error");
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetSecretAsync<string>("encrypted-secret"));
        Assert.IsType<DecryptionFailureException>(ex.InnerException);
    }

    [Fact]
    public async Task GetSecretAsync_InvalidRequest_Throws()
    {
        _secretsManager.ExceptionToThrow = new InvalidRequestException("pending deletion");
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetSecretAsync<string>("deleted-secret"));
        Assert.IsType<InvalidRequestException>(ex.InnerException);
    }

    [Fact]
    public async Task GetSecretAsync_InvalidParameter_Throws()
    {
        _secretsManager.ExceptionToThrow = new InvalidParameterException("bad param");
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetSecretAsync<string>("bad-name"));
        Assert.IsType<InvalidParameterException>(ex.InnerException);
    }

    [Fact]
    public async Task GetSecretAsync_InternalServiceError_Throws()
    {
        _secretsManager.ExceptionToThrow = new InternalServiceErrorException("aws down");
        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.GetSecretAsync<string>("any-secret"));
        Assert.IsType<InternalServiceErrorException>(ex.InnerException);
    }

    [Fact]
    public async Task GetSecretAsync_Null_SecretName_Throws()
    {
        var sut = CreateSut();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.GetSecretAsync<string>(null!));
    }

    [Fact]
    public async Task GetSecretAsync_Empty_SecretName_Throws()
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
    }
}
