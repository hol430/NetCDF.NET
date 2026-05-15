using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class DataRoundTripTests
{
    [Theory]
    [InlineData(3, 1, 4, 1, 5)]
    public void IntData_RoundTripAcrossCloseReopen(params int[] expected)
    {
        using var temp = new TempFile();

        using (NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)5, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, expected), "nc_put_var_int");
        }

        using (NcFileHandle hnd = NcFileHandle.Open(temp.FilePath, OpenMode.NC_NOWRITE))
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "ints", out int readVarId), "nc_inq_varid");

            int[] actual = new int[expected.Length];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_int(hnd.Id, readVarId, actual), "nc_get_var_int");
            Assert.Equal(expected, actual);
        }
    }

    [Theory]
    [InlineData(0.5d, -1.25d, 2.0d, 9.75d)]
    public void DoubleData_RoundTripAcrossCloseReopen(params double[] expected)
    {
        using var temp = new TempFile();

        using (NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)4, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "doubles", NCType.NC_DOUBLE, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

            InteropTestCommon.AssertSuccess(Native.nc_put_var_double(hnd.Id, varId, expected), "nc_put_var_double");
        }

        using (NcFileHandle hnd = NcFileHandle.Open(temp.FilePath, OpenMode.NC_NOWRITE))
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "doubles", out int readVarId), "nc_inq_varid");

            double[] actual = new double[expected.Length];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_double(hnd.Id, readVarId, actual), "nc_get_var_double");
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void PutVaraInt_WritesSubset_AndGetVarIntReadsExpectedFullArray()
    {
        using var temp = new TempFile();

        using (NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)6, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

            int[] all = [0, 0, 0, 0, 0, 0];
            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, all), "nc_put_var_int(init)");

            IntPtr[] start = [(IntPtr)2];
            IntPtr[] count = [(IntPtr)3];
            int[] subset = [9, 8, 7];
            InteropTestCommon.AssertSuccess(Native.nc_put_vara_int(hnd.Id, varId, start, count, subset), "nc_put_vara_int");
        }

        using (NcFileHandle hnd = NcFileHandle.Open(temp.FilePath, OpenMode.NC_NOWRITE))
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "ints", out int varId), "nc_inq_varid");
            int[] actual = new int[6];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_int(hnd.Id, varId, actual), "nc_get_var_int");
            Assert.Equal(new[] { 0, 0, 9, 8, 7, 0 }, actual);
        }
    }

    [Fact]
    public void GetVaraInt_ReadsSubset()
    {
        using var temp = new TempFile();

        using (NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)6, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

            int[] expected = [10, 20, 30, 40, 50, 60];
            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, expected), "nc_put_var_int");
        }

        using (NcFileHandle hnd = NcFileHandle.Open(temp.FilePath, OpenMode.NC_NOWRITE))
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "ints", out int varId), "nc_inq_varid");

            IntPtr[] start = [(IntPtr)1];
            IntPtr[] count = [(IntPtr)3];
            int[] actual = new int[3];
            InteropTestCommon.AssertSuccess(Native.nc_get_vara_int(hnd.Id, varId, start, count, actual), "nc_get_vara_int");
            Assert.Equal(new[] { 20, 30, 40 }, actual);
        }
    }
}
