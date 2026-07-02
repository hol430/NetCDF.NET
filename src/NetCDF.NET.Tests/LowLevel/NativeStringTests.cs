using System.Text;
using NetCDF.LowLevel;

namespace NetCDF.Tests.LowLevel;

public class NativeStringTests
{
    [Fact]
    public void DecodeNullTerminatedUtf8_EmptyBuffer_ReturnsEmptyString()
    {
        byte[] buffer = Array.Empty<byte>();
        string result = NativeString.DecodeNullTerminatedUtf8(buffer);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void DecodeNullTerminatedUtf8_BufferWithNullTerminator_ReturnsDecodedString()
    {
        byte[] buffer = Encoding.UTF8.GetBytes("Hello, World!\0ExtraData");
        string result = NativeString.DecodeNullTerminatedUtf8(buffer);
        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void DecodeNullTerminatedUtf8_BufferWithoutNullTerminator_ReturnsDecodedString()
    {
        byte[] buffer = Encoding.UTF8.GetBytes("Hello, World!");
        string result = NativeString.DecodeNullTerminatedUtf8(buffer);
        Assert.Equal("Hello, World!", result);
    }
}