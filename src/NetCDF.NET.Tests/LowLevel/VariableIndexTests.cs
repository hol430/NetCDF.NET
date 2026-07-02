using NetCDF.LowLevel;

namespace NetCDF.Tests.LowLevel;

public class VariableIndexTests
{
    [Fact]
    public void VariableIndex_EmptyIndices_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new VariableIndex([]));
    }

    [Fact]
    public void VariableIndex_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VariableIndex(-1, 1));
    }

    [Theory]
    [InlineData(new long[] { 0, 1, 2, 128 })]
    public void VariableIndex_SetsProperties(long[] indices)
    {
        VariableIndex index = new(indices);
        Assert.Equal(indices, index.Coordinates);
    }
}
