using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon.Runtime;

namespace Rtl.Core.Infrastructure.Tests.Secrets;

/// <summary>
/// Hand-rolled fake for IAmazonSecretsManager. Returns configured secret or throws configured exception.
/// </summary>
internal sealed class FakeSecretsManager : IAmazonSecretsManager
{
    public string SecretToReturn { get; set; } = string.Empty;
    public Exception? ExceptionToThrow { get; set; }
    public int CallCount { get; private set; }

    public Task<GetSecretValueResponse> GetSecretValueAsync(
        GetSecretValueRequest request, CancellationToken cancellationToken = default)
    {
        CallCount++;

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        return Task.FromResult(new GetSecretValueResponse
        {
            SecretString = SecretToReturn
        });
    }

    // --- Not used by AwsSecretProvider — throw NotImplementedException for everything else ---

    public ISecretsManagerPaginatorFactory Paginators => throw new NotImplementedException();
    public IClientConfig Config => throw new NotImplementedException();

    public Task<BatchGetSecretValueResponse> BatchGetSecretValueAsync(BatchGetSecretValueRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CancelRotateSecretResponse> CancelRotateSecretAsync(CancelRotateSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<CreateSecretResponse> CreateSecretAsync(CreateSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<DeleteResourcePolicyResponse> DeleteResourcePolicyAsync(DeleteResourcePolicyRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<DeleteSecretResponse> DeleteSecretAsync(DeleteSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<DescribeSecretResponse> DescribeSecretAsync(DescribeSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<GetRandomPasswordResponse> GetRandomPasswordAsync(GetRandomPasswordRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<GetResourcePolicyResponse> GetResourcePolicyAsync(GetResourcePolicyRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<ListSecretVersionIdsResponse> ListSecretVersionIdsAsync(ListSecretVersionIdsRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<ListSecretsResponse> ListSecretsAsync(ListSecretsRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<PutResourcePolicyResponse> PutResourcePolicyAsync(PutResourcePolicyRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<PutSecretValueResponse> PutSecretValueAsync(PutSecretValueRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<RemoveRegionsFromReplicationResponse> RemoveRegionsFromReplicationAsync(RemoveRegionsFromReplicationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<ReplicateSecretToRegionsResponse> ReplicateSecretToRegionsAsync(ReplicateSecretToRegionsRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<RestoreSecretResponse> RestoreSecretAsync(RestoreSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<RotateSecretResponse> RotateSecretAsync(RotateSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<StopReplicationToReplicaResponse> StopReplicationToReplicaAsync(StopReplicationToReplicaRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<TagResourceResponse> TagResourceAsync(TagResourceRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<UntagResourceResponse> UntagResourceAsync(UntagResourceRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<UpdateSecretResponse> UpdateSecretAsync(UpdateSecretRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<UpdateSecretVersionStageResponse> UpdateSecretVersionStageAsync(UpdateSecretVersionStageRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<ValidateResourcePolicyResponse> ValidateResourcePolicyAsync(ValidateResourcePolicyRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Amazon.Runtime.Endpoints.Endpoint DetermineServiceOperationEndpoint(AmazonWebServiceRequest request) => throw new NotImplementedException();
    public void Dispose() { }
}
