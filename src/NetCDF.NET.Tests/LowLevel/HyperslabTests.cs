using NetCDF.LowLevel;

namespace NetCDF.Tests.LowLevel;

public class HyperslabTests
{
    [Fact]
    public void Hyperslab_EmptyStart_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Hyperslab([], [1]));
    }

    [Theory]
    [InlineData(new long[] { 0, 1, 2 })]
    public void Hyperslab_SetsProperties(long[] start)
    {
        long[] count = start.Select(x => x + 1).ToArray();
        long[] stride = start.Select(x => x + 2).ToArray();
        Hyperslab hyperslab = new(start, count, stride);
        Assert.Equal(start, hyperslab.Start);
        Assert.Equal(count, hyperslab.Count);
        Assert.Equal(stride, hyperslab.Stride);
    }

    [Fact]
    public void Hyperslab_NonStrided_IsStridedReturnsFalse()
    {
        Hyperslab hyperslab = new([0], [1]);
        Assert.False(hyperslab.IsStrided);
    }

    [Fact]
    public void Hyperslab_NonStrided_ToNativeStride_ThrowsInvalidOperationException()
    {
        Hyperslab hyperslab = new([0], [1]);
        Assert.Throws<InvalidOperationException>(() => hyperslab.ToNativeStride());
    }

    [Fact]
    public void Hyperslab_NegativeStart_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Hyperslab([-1], [1]));
    }

    [Fact]
    public void Hyperslab_NonPositiveCount_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Hyperslab([0], [0]));
    }
}
