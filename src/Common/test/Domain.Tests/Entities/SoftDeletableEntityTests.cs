using Rtl.Core.Domain.Entities;
using Xunit;

namespace Rtl.Core.Domain.Tests.Entities;

public class SoftDeletableEntityTests
{
    [Fact]
    public void Restore_ResetsAllDeleteFields()
    {
        var entity = new TestSoftDeletableEntity();
        entity.SetDeleted(true, DateTime.UtcNow, Guid.NewGuid());

        entity.Restore();

        Assert.False(entity.IsDeleted);
        Assert.Null(entity.DeletedAtUtc);
        Assert.Null(entity.DeletedByUserId);
    }

    private sealed class TestSoftDeletableEntity : SoftDeletableEntity
    {
        public void SetDeleted(bool isDeleted, DateTime? deletedAtUtc, Guid? deletedByUserId)
        {
            IsDeleted = isDeleted;
            DeletedAtUtc = deletedAtUtc;
            DeletedByUserId = deletedByUserId;
        }
    }
}
