using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class DataRoundTripTests
{
    [Fact]
    public void IntData_RoundTripAcrossCloseReopen()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)5, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(ncid), "nc_enddef");

            int[] expected = [3, 1, 4, 1, 5];
            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(ncid, varId, expected), "nc_put_var_int");
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close(write)");
            ncid = -1;

            InteropTestCommon.AssertSuccess(Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out ncid), "nc_open");
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(ncid, "ints", out int readVarId), "nc_inq_varid");

            int[] actual = new int[expected.Length];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_int(ncid, readVarId, actual), "nc_get_var_int");
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }

    [Fact]
    public void DoubleData_RoundTripAcrossCloseReopen()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)4, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "doubles", NCType.NC_DOUBLE, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(ncid), "nc_enddef");

            double[] expected = [0.5d, -1.25d, 2.0d, 9.75d];
            InteropTestCommon.AssertSuccess(Native.nc_put_var_double(ncid, varId, expected), "nc_put_var_double");
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close(write)");
            ncid = -1;

            InteropTestCommon.AssertSuccess(Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out ncid), "nc_open");
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(ncid, "doubles", out int readVarId), "nc_inq_varid");

            double[] actual = new double[expected.Length];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_double(ncid, readVarId, actual), "nc_get_var_double");
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }
}
