using NetCDF.Interop;
using NetCDF.Tests.Helpers;

using static NetCDF.Tests.Interop.InteropTestCommon;

namespace NetCDF.Tests.Interop;

public sealed class DataRoundTripTests
{
    [Theory]
    [MemberData(nameof(NetcdfTestFormats.AllFormats), MemberType = typeof(NetcdfTestFormats))]
    public void IntData_RoundTripAcrossCloseReopen_AllAvailableFileFormats(NetcdfTestFormat format)
    {
        int[] expected = [3, 1, 4, 1, 5];
        using NcTempFile hnd = new(format);

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, expected), "nc_put_var_int");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "ints", out int readVarId), "nc_inq_varid");

        int[] actual = new int[expected.Length];
        AssertSuccess(Native.nc_get_var_int(read.Id, readVarId, actual), "nc_get_var_int");
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(NetcdfTestFormats.AllFormats), MemberType = typeof(NetcdfTestFormats))]
    public void PutVaraInt_WritesSubset_AllAvailableFileFormats(NetcdfTestFormat format)
    {
        using NcTempFile hnd = new(format);
        int[] all = [0, 0, 0, 0, 0, 0];

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)all.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, all), "nc_put_var_int(init)");

        nuint[] start = [(nuint)2];
        nuint[] count = [(nuint)3];
        int[] subset = [9, 8, 7];
        AssertSuccess(Native.nc_put_vara_int(hnd.Id, varId, start, count, subset), "nc_put_vara_int");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "ints", out varId), "nc_inq_varid");
        int[] actual = new int[6];
        AssertSuccess(Native.nc_get_var_int(read.Id, varId, actual), "nc_get_var_int");
        Assert.Equal(new[] { 0, 0, 9, 8, 7, 0 }, actual);
    }

    [Theory]
    [InlineData(3, 1, 4, 1, 5)]
    public void IntData_RoundTripAcrossCloseReopen(params int[] expected)
    {
        using NcTempFile hnd = new();

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, expected), "nc_put_var_int");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "ints", out int readVarId), "nc_inq_varid");

        int[] actual = new int[expected.Length];
        AssertSuccess(Native.nc_get_var_int(read.Id, readVarId, actual), "nc_get_var_int");
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0.5d, -1.25d, 2.0d, 9.75d)]
    public void DoubleData_RoundTripAcrossCloseReopen(params double[] expected)
    {
        using NcTempFile hnd = new();

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "doubles", NCType.NC_DOUBLE, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        AssertSuccess(Native.nc_put_var_double(hnd.Id, varId, expected), "nc_put_var_double");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "doubles", out int readVarId), "nc_inq_varid");

        double[] actual = new double[expected.Length];
        AssertSuccess(Native.nc_get_var_double(read.Id, readVarId, actual), "nc_get_var_double");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PutVaraInt_WritesSubset_AndGetVarIntReadsExpectedFullArray()
    {
        using NcTempFile hnd = new();
        int[] all = [0, 0, 0, 0, 0, 0];

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)all.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, all), "nc_put_var_int(init)");

        nuint[] start = [(nuint)2];
        nuint[] count = [(nuint)3];
        int[] subset = [9, 8, 7];
        AssertSuccess(Native.nc_put_vara_int(hnd.Id, varId, start, count, subset), "nc_put_vara_int");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "ints", out varId), "nc_inq_varid");
        int[] actual = new int[6];
        AssertSuccess(Native.nc_get_var_int(read.Id, varId, actual), "nc_get_var_int");
        Assert.Equal(new[] { 0, 0, 9, 8, 7, 0 }, actual);
    }

    [Fact]
    public void GetVaraInt_ReadsSubset()
    {
        using NcTempFile hnd = new();
        int[] expected = [10, 20, 30, 40, 50, 60];

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "ints", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, expected), "nc_put_var_int");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "ints", out varId), "nc_inq_varid");

        nuint[] start = [(nuint)1];
        nuint[] count = [(nuint)3];
        int[] actual = new int[3];
        AssertSuccess(Native.nc_get_vara_int(read.Id, varId, start, count, actual), "nc_get_vara_int");
        Assert.Equal(new[] { 20, 30, 40 }, actual);
    }

    [Theory]
    [InlineData(1.25f, -3.5f, 0.0f, 9.75f)]
    public void FloatData_RoundTripAcrossCloseReopen(params float[] expected)
    {
        using NcTempFile hnd = new();

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "floats", NCType.NC_FLOAT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        AssertSuccess(Native.nc_put_var_float(hnd.Id, varId, expected), "nc_put_var_float");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "floats", out int readVarId), "nc_inq_varid");

        float[] actual = new float[expected.Length];
        AssertSuccess(Native.nc_get_var_float(read.Id, readVarId, actual), "nc_get_var_float");
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData((short)-2, (short)0, (short)7, (short)1024)]
    public void ShortData_RoundTripAcrossCloseReopen(params short[] expected)
    {
        using NcTempFile hnd = new();

        AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        AssertSuccess(Native.nc_def_var(hnd.Id, "shorts", NCType.NC_SHORT, 1, [dimId], out int varId), "nc_def_var");
        AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        AssertSuccess(Native.nc_put_var_short(hnd.Id, varId, expected), "nc_put_var_short");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        AssertSuccess(Native.nc_inq_varid(read.Id, "shorts", out int readVarId), "nc_inq_varid");

        short[] actual = new short[expected.Length];
        AssertSuccess(Native.nc_get_var_short(read.Id, readVarId, actual), "nc_get_var_short");
        Assert.Equal(expected, actual);
    }
}
