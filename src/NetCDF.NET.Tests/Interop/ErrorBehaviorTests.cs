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
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_inq_dimid(hnd.Id, "does_not_exist", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcInqVarid_InvalidName_ReturnsError()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_inq_varid(hnd.Id, "does_not_exist", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcInqVartype_InvalidVarId_ReturnsError()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_inq_vartype(hnd.Id, -1, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcInqDimlen_InvalidDimId_ReturnsError()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_inq_dimlen(hnd.Id, -1, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }
}
