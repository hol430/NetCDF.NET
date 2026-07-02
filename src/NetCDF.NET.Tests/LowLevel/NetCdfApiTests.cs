using NetCDF.Interop;
using NetCDF.LowLevel;
using NetCDF.Tests.Helpers;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.Tests.LowLevel;

public sealed class NetCdfApiTests
{
    [Fact]
    public void Create_ReturnsOwnedHandle_AndCloseClosesNativeFile()
    {
        NetCdfApi api = new();

        using TempFile file = new();
        using (NetCdfHandle handle = api.Create(file.FilePath))
            Assert.True(handle.Id >= 0);

        using NetCdfHandle reopened = api.Open(file.FilePath);
        Assert.True(reopened.Id >= 0);
    }

    [Fact]
    public void GetLibraryVersion_ReturnsNonEmptyString()
    {
        NetCdfApi api = new();

        string version = api.GetLibraryVersion();

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void Open_ExistingFile_ReturnsOwnedHandle()
    {
        NetCdfApi api = new();

        using TempFile file = new();
        using (NetCdfHandle created = api.Create(file.FilePath))
        {
        }

        using NetCdfHandle opened = api.Open(file.FilePath, OpenMode.NC_NOWRITE);

        Assert.True(opened.Id >= 0);
    }

    [Fact]
    public void Open_MissingFile_ThrowsNetCdfException()
    {
        NetCdfApi api = new();

        using TempFile file = new();

        NetCdfException ex = Assert.Throws<NetCdfException>(
            () => api.Open(file.FilePath, OpenMode.NC_NOWRITE));

        Assert.Equal(nameof(Native.nc_open), ex.FunctionName);
        Assert.NotEqual(NcNoErr, ex.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(ex.NativeMessage));
    }

    [Fact]
    public void Dispose_CalledTwice_ClosesOnlyOnce()
    {
        NetCdfApi api = new();

        using TempFile file = new();
        NetCdfHandle handle = api.Create(file.FilePath);

        handle.Dispose();
        handle.Dispose();
    }
}
