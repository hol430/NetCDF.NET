using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class CreateOpenCloseTests
{
    [Fact]
    public void NcInqLibvers_ReturnsNonEmptyString()
    {
        string version = Native.nc_inq_libvers();
        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.Contains('.', version);
    }

    [Fact]
    public void NcStrerror_ForNegativeCode_ReturnsMessage()
    {
        string message = Native.nc_strerror(-1);
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void NcOpen_MissingFile_ReturnsError()
    {
        using var temp = new TempFile();
        int status = Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcCreateSyncClose_Reopen_Succeeds()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_sync(ncid), "nc_sync");
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close(create)");
            ncid = -1;

            InteropTestCommon.AssertSuccess(Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out ncid), "nc_open");
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }

        Assert.True(File.Exists(temp.FilePath));
    }
}
