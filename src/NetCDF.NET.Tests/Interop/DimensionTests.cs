using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class DimensionTests
{
    [Fact]
    public void DefineAndInquireDimension_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", (nuint)4, out int dimid), "nc_def_dim");

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimid(hnd.Id, "time", out int lookedUpDimId), "nc_inq_dimid");
        Assert.Equal(dimid, lookedUpDimId);

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimlen(hnd.Id, dimid, out nuint len), "nc_inq_dimlen");
        Assert.Equal((nuint)4, len);
    }

    [Fact]
    public void InqUnlimdims_UsesTwoCallPattern()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", 0, out int unlimDimId), "nc_def_dim(unlimited)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(hnd.Id, out int count, null), "nc_inq_unlimdims(count)");
        Assert.Equal(1, count);

        int[] ids = new int[count];
        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(hnd.Id, out int countAgain, ids), "nc_inq_unlimdims(values)");
        Assert.Equal(count, countAgain);
        Assert.Equal(unlimDimId, ids[0]);
    }

    [Fact]
    public void InqUnlimdim_ReturnsSingleUnlimitedId()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", 0, out int unlimDimId), "nc_def_dim(unlimited)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdim(hnd.Id, out int lookedUp), "nc_inq_unlimdim");

        Assert.Equal(unlimDimId, lookedUp);
    }
}
