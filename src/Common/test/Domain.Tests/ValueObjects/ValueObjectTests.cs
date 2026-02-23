using Rtl.Core.Domain.ValueObjects;
using Xunit;

namespace Rtl.Core.Domain.Tests.ValueObjects;

public class ValueObjectTests
{
    [Fact]
    public void Equals_ReturnsTrueForEqualValues()
    {
        var vo1 = new TestValueObject("value", 42);
        var vo2 = new TestValueObject("value", 42);

        Assert.True(vo1.Equals(vo2));
        Assert.True(vo1 == vo2);
    }

    [Fact]
    public void Equals_ReturnsFalseForDifferentValues()
    {
        var vo1 = new TestValueObject("value1", 42);
        var vo2 = new TestValueObject("value2", 42);

        Assert.False(vo1.Equals(vo2));
        Assert.True(vo1 != vo2);
    }

    [Fact]
    public void GetHashCode_ReturnsSameHashForEqualObjects()
    {
        var vo1 = new TestValueObject("value", 42);
        var vo2 = new TestValueObject("value", 42);

        Assert.Equal(vo1.GetHashCode(), vo2.GetHashCode());
    }

    private sealed class TestValueObject(string stringValue, int intValue) : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return stringValue;
            yield return intValue;
        }
    }
}
