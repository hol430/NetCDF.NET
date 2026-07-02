using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class AttributeTests
{
    private static int DefineIntVariable(int ncid)
    {
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)4, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        return varId;
    }

    private static void AssertAttributeMetadata(int ncid, int varid, string name, NCType expectedType, nuint expectedLen)
    {
        InteropTestCommon.AssertSuccess(Native.nc_inq_att(ncid, varid, name, out NCType typeFromInqAtt, out nuint lenFromInqAtt), "nc_inq_att");
        Assert.Equal(expectedType, typeFromInqAtt);
        Assert.Equal(expectedLen, lenFromInqAtt);

        InteropTestCommon.AssertSuccess(Native.nc_inq_atttype(ncid, varid, name, out NCType typeFromInqType), "nc_inq_atttype");
        Assert.Equal(expectedType, typeFromInqType);

        InteropTestCommon.AssertSuccess(Native.nc_inq_attlen(ncid, varid, name, out nuint lenFromInqLen), "nc_inq_attlen");
        Assert.Equal(expectedLen, lenFromInqLen);

        InteropTestCommon.AssertSuccess(Native.nc_inq_attid(ncid, varid, name, out int attId), "nc_inq_attid");
        Assert.True(attId >= 0);
    }

    [Fact]
    public void GlobalIntAttribute_RoundTrip()
    {
        using NcTempFile hnd = new();

        int[] expected = [42];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1, expected),
            "nc_put_att_int(global)");

        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1);

        int[] actual = new int[1];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", actual), "nc_get_att_int(global)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void VariableIntAttribute_RoundTrip()
    {
        using NcTempFile hnd = new();

        int varId = DefineIntVariable(hnd.Id);

        int[] expected = [7, 11];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "pair", NCType.NC_INT, 2, expected), "nc_put_att_int(var)");
        AssertAttributeMetadata(hnd.Id, varId, "pair", NCType.NC_INT, 2);

        int[] actual = new int[2];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_int(hnd.Id, varId, "pair", actual), "nc_get_att_int(var)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalDoubleAttribute_RoundTrip()
    {
        using NcTempFile hnd = new();

        double[] expected = [1.5d, -2.25d];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_double(hnd.Id, InteropTestCommon.NcGlobal, "dvals", NCType.NC_DOUBLE, 2, expected),
            "nc_put_att_double(global)");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "dvals", NCType.NC_DOUBLE, 2);

        double[] actual = new double[2];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_double(hnd.Id, InteropTestCommon.NcGlobal, "dvals", actual), "nc_get_att_double(global)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void VariableFloatAttribute_RoundTrip()
    {
        using NcTempFile hnd = new();

        int varId = DefineIntVariable(hnd.Id);

        float[] expected = [0.5f, -3.25f, 9.0f];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_float(hnd.Id, varId, "scales", NCType.NC_FLOAT, 3, expected),
            "nc_put_att_float(var)");
        AssertAttributeMetadata(hnd.Id, varId, "scales", NCType.NC_FLOAT, 3);

        float[] actual = new float[3];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_float(hnd.Id, varId, "scales", actual), "nc_get_att_float(var)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalTextAttribute_InquiryMatches()
    {
        using NcTempFile hnd = new();

        const string text = "netcdf";
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_text(hnd.Id, InteropTestCommon.NcGlobal, "title", (nuint)text.Length, text),
            "nc_put_att_text(global)");

        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "title", NCType.NC_CHAR, (nuint)text.Length);
    }

    [Fact]
    public void GlobalScharAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new();
        sbyte[] expected = [-4, 9];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_schar(hnd.Id, InteropTestCommon.NcGlobal, "schar_values", NCType.NC_BYTE, 2, expected), "nc_put_att_schar");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "schar_values", NCType.NC_BYTE, 2);
        sbyte[] actual = new sbyte[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_schar(hnd.Id, InteropTestCommon.NcGlobal, "schar_values", actual), "nc_get_att_schar");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalUcharAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        byte[] expected = [0, 255];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_uchar(hnd.Id, InteropTestCommon.NcGlobal, "uchar_values", NCType.NC_UBYTE, 2, expected), "nc_put_att_uchar");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "uchar_values", NCType.NC_UBYTE, 2);
        byte[] actual = new byte[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_uchar(hnd.Id, InteropTestCommon.NcGlobal, "uchar_values", actual), "nc_get_att_uchar");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalShortAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new();
        short[] expected = [-123, 456];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_short(hnd.Id, InteropTestCommon.NcGlobal, "short_values", NCType.NC_SHORT, 2, expected), "nc_put_att_short");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "short_values", NCType.NC_SHORT, 2);
        short[] actual = new short[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_short(hnd.Id, InteropTestCommon.NcGlobal, "short_values", actual), "nc_get_att_short");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalUbyteAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        byte[] expected = [1, 200];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_ubyte(hnd.Id, InteropTestCommon.NcGlobal, "ubyte_values", NCType.NC_UBYTE, 2, expected), "nc_put_att_ubyte");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "ubyte_values", NCType.NC_UBYTE, 2);
        byte[] actual = new byte[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_ubyte(hnd.Id, InteropTestCommon.NcGlobal, "ubyte_values", actual), "nc_get_att_ubyte");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalUshortAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        ushort[] expected = [12, 65000];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_ushort(hnd.Id, InteropTestCommon.NcGlobal, "ushort_values", NCType.NC_USHORT, 2, expected), "nc_put_att_ushort");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "ushort_values", NCType.NC_USHORT, 2);
        ushort[] actual = new ushort[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_ushort(hnd.Id, InteropTestCommon.NcGlobal, "ushort_values", actual), "nc_get_att_ushort");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalUintAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        uint[] expected = [1234u, 3_000_000_000u];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_uint(hnd.Id, InteropTestCommon.NcGlobal, "uint_values", NCType.NC_UINT, 2, expected), "nc_put_att_uint");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "uint_values", NCType.NC_UINT, 2);
        uint[] actual = new uint[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_uint(hnd.Id, InteropTestCommon.NcGlobal, "uint_values", actual), "nc_get_att_uint");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalLongAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        long[] expected = [-1234567890L, 9876543210L];
        PutAttLong(hnd.Id, InteropTestCommon.NcGlobal, "long_values", NCType.NC_INT64, expected);
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "long_values", NCType.NC_INT64, 2);
        long[] actual = new long[expected.Length];
        GetAttLong(hnd.Id, InteropTestCommon.NcGlobal, "long_values", actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalLonglongAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        long[] expected = [-9_000_000_000L, 9_000_000_000L];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_longlong(hnd.Id, InteropTestCommon.NcGlobal, "longlong_values", NCType.NC_INT64, 2, expected), "nc_put_att_longlong");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "longlong_values", NCType.NC_INT64, 2);
        long[] actual = new long[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_longlong(hnd.Id, InteropTestCommon.NcGlobal, "longlong_values", actual), "nc_get_att_longlong");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GlobalUlonglongAttribute_RoundTripAndInquire()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);
        ulong[] expected = [0UL, 18_000_000_000UL];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_ulonglong(hnd.Id, InteropTestCommon.NcGlobal, "ulonglong_values", NCType.NC_UINT64, 2, expected), "nc_put_att_ulonglong");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "ulonglong_values", NCType.NC_UINT64, 2);
        ulong[] actual = new ulong[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_ulonglong(hnd.Id, InteropTestCommon.NcGlobal, "ulonglong_values", actual), "nc_get_att_ulonglong");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CopyRenameDeleteAttribute_GlobalAndVariable_WorkAsExpected()
    {
        using NcTempFile hnd = new();
        int varId = DefineIntVariable(hnd.Id);

        int[] gSource = [41];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "source_global", NCType.NC_INT, 1, gSource), "nc_put_att_int(source_global)");

        InteropTestCommon.AssertSuccess(Native.nc_copy_att(hnd.Id, InteropTestCommon.NcGlobal, "source_global", hnd.Id, varId), "nc_copy_att(global->var)");
        AssertAttributeMetadata(hnd.Id, varId, "source_global", NCType.NC_INT, 1);
        int[] copiedToVar = new int[1];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_int(hnd.Id, varId, "source_global", copiedToVar), "nc_get_att_int(copied_var)");
        Assert.Equal(gSource, copiedToVar);

        InteropTestCommon.AssertSuccess(Native.nc_rename_att(hnd.Id, varId, "source_global", "renamed_var"), "nc_rename_att(var)");
        int renamedLookupStatus = Native.nc_inq_attid(hnd.Id, varId, "renamed_var", out _);
        InteropTestCommon.AssertSuccess(renamedLookupStatus, "nc_inq_attid(renamed_var)");
        int oldNameLookupStatus = Native.nc_inq_attid(hnd.Id, varId, "source_global", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, oldNameLookupStatus);

        InteropTestCommon.AssertSuccess(Native.nc_del_att(hnd.Id, varId, "renamed_var"), "nc_del_att(var)");
        int deletedLookupStatus = Native.nc_inq_attid(hnd.Id, varId, "renamed_var", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, deletedLookupStatus);

        int[] varSource = [7, 8];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "var_source", NCType.NC_INT, 2, varSource), "nc_put_att_int(var_source)");
        InteropTestCommon.AssertSuccess(Native.nc_copy_att(hnd.Id, varId, "var_source", hnd.Id, InteropTestCommon.NcGlobal), "nc_copy_att(var->global)");
        AssertAttributeMetadata(hnd.Id, InteropTestCommon.NcGlobal, "var_source", NCType.NC_INT, 2);
        int[] copiedToGlobal = new int[2];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_int(hnd.Id, InteropTestCommon.NcGlobal, "var_source", copiedToGlobal), "nc_get_att_int(copied_global)");
        Assert.Equal(varSource, copiedToGlobal);
    }

    [Fact]
    public void InqNattAndVarNatts_ReturnExpectedCounts()
    {
        using NcTempFile hnd = new();

        int varId = DefineIntVariable(hnd.Id);

        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "g1", NCType.NC_INT, 1, [1]), "nc_put_att_int(global)");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_float(hnd.Id, InteropTestCommon.NcGlobal, "g2", NCType.NC_FLOAT, 2, [1.0f, 2.0f]), "nc_put_att_float(global)");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "a1", NCType.NC_INT, 1, [1]), "nc_put_att_int(var)");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_double(hnd.Id, varId, "a2", NCType.NC_DOUBLE, 1, [2.0d]), "nc_put_att_double(var)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_natts(hnd.Id, out int ngatts), "nc_inq_natts");
        Assert.Equal(2, ngatts);

        InteropTestCommon.AssertSuccess(Native.nc_inq_varnatts(hnd.Id, varId, out int varNatts), "nc_inq_varnatts");
        Assert.Equal(2, varNatts);
    }

    [Fact]
    public void MissingAttribute_InquiryAndRead_ReturnErrors()
    {
        using NcTempFile hnd = new();
        int varId = DefineIntVariable(hnd.Id);

        Assert.NotEqual(InteropTestCommon.NcNoErr, Native.nc_inq_att(hnd.Id, varId, "missing", out _, out _));
        Assert.NotEqual(InteropTestCommon.NcNoErr, Native.nc_inq_attid(hnd.Id, varId, "missing", out _));
        Assert.NotEqual(InteropTestCommon.NcNoErr, Native.nc_inq_attlen(hnd.Id, varId, "missing", out _));
        Assert.NotEqual(InteropTestCommon.NcNoErr, Native.nc_inq_atttype(hnd.Id, varId, "missing", out _));

        int[] values = new int[1];
        Assert.NotEqual(InteropTestCommon.NcNoErr, Native.nc_get_att_int(hnd.Id, varId, "missing", values));
    }

    private static void PutAttLong(int ncid, int varid, string name, NCType type, long[] values)
    {
        if (OperatingSystem.IsWindows())
        {
            int[] i32 = Array.ConvertAll(values, checked(v => (int)v));
            InteropTestCommon.AssertSuccess(Native.Windows.nc_put_att_long(ncid, varid, name, type, (nuint)i32.Length, i32), "nc_put_att_long");
            return;
        }

        InteropTestCommon.AssertSuccess(Native.Unix.nc_put_att_long(ncid, varid, name, type, (nuint)values.Length, values), "nc_put_att_long");
    }

    private static void GetAttLong(int ncid, int varid, string name, long[] destination)
    {
        if (OperatingSystem.IsWindows())
        {
            int[] i32 = new int[destination.Length];
            InteropTestCommon.AssertSuccess(Native.Windows.nc_get_att_long(ncid, varid, name, i32), "nc_get_att_long");
            for (int i = 0; i < i32.Length; i++)
            {
                destination[i] = i32[i];
            }

            return;
        }

        InteropTestCommon.AssertSuccess(Native.Unix.nc_get_att_long(ncid, varid, name, destination), "nc_get_att_long");
    }
}
