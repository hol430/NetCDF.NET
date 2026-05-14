using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class DimensionTests
{
    [Fact]
    public void DefineAndInquireDimension_RoundTrip()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "time", (nuint)4, out int dimid), "nc_def_dim");

            InteropTestCommon.AssertSuccess(Native.nc_inq_dimid(ncid, "time", out int lookedUpDimId), "nc_inq_dimid");
            Assert.Equal(dimid, lookedUpDimId);

            InteropTestCommon.AssertSuccess(Native.nc_inq_dimlen(ncid, dimid, out nuint len), "nc_inq_dimlen");
            Assert.Equal((nuint)4, len);

            InteropTestCommon.AssertSuccess(Native.nc_inq_dimlen(ncid, dimid, out nuint inqLen), "nc_inq_dimlen(second)");
            Assert.Equal((nuint)4, inqLen);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }

    [Fact]
    public void InqUnlimdims_UsesTwoCallPattern()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "time", 0, out int unlimDimId), "nc_def_dim(unlimited)");

            InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(ncid, out int count, null), "nc_inq_unlimdims(count)");
            Assert.Equal(1, count);

            int[] ids = new int[count];
            InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(ncid, out int countAgain, ids), "nc_inq_unlimdims(values)");
            Assert.Equal(count, countAgain);
            Assert.Equal(unlimDimId, ids[0]);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }
}
