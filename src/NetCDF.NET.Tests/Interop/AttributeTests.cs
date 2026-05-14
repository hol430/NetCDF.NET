using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class AttributeTests
{
    [Fact]
    public void GlobalIntAttribute_RoundTrip()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");

            int[] expected = [42];
            InteropTestCommon.AssertSuccess(
                Native.nc_put_att_int(ncid, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1, expected),
                "nc_put_att_int(global)");

            InteropTestCommon.AssertSuccess(Native.nc_inq_attlen(ncid, InteropTestCommon.NcGlobal, "answer", out nuint len), "nc_inq_attlen");
            Assert.Equal((nuint)1, len);

            InteropTestCommon.AssertSuccess(Native.nc_inq_atttype(ncid, InteropTestCommon.NcGlobal, "answer", out NCType type), "nc_inq_atttype");
            Assert.Equal(NCType.NC_INT, type);

            int[] actual = new int[1];
            InteropTestCommon.AssertSuccess(Native.nc_get_att_int(ncid, InteropTestCommon.NcGlobal, "answer", actual), "nc_get_att_int(global)");
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }

    [Fact]
    public void VariableIntAttribute_RoundTrip()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)2, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

            int[] expected = [7, 11];
            InteropTestCommon.AssertSuccess(Native.nc_put_att_int(ncid, varId, "pair", NCType.NC_INT, 2, expected), "nc_put_att_int(var)");

            int[] actual = new int[2];
            InteropTestCommon.AssertSuccess(Native.nc_get_att_int(ncid, varId, "pair", actual), "nc_get_att_int(var)");
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }
}
