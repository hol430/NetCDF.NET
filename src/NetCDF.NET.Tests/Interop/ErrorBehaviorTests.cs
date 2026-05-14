using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class ErrorBehaviorTests
{
    [Fact]
    public void NcClose_InvalidId_ReturnsError()
    {
        int status = Native.nc_close(-1);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcInqDimid_InvalidName_ReturnsError()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            int status = Native.nc_inq_dimid(ncid, "does_not_exist", out _);
            Assert.NotEqual(InteropTestCommon.NcNoErr, status);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }

    [Fact]
    public void NcInqVarid_InvalidName_ReturnsError()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            int status = Native.nc_inq_varid(ncid, "does_not_exist", out _);
            Assert.NotEqual(InteropTestCommon.NcNoErr, status);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }
}
