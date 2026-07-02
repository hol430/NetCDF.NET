using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class MetadataFunctionTests
{
    [Fact]
    public void NcInqVar_AndNcInqVarname_ReturnExpectedMetadata()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)5, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "temperature", NCType.NC_FLOAT, 1, [dimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "scale", NCType.NC_INT, 1, [1]), "nc_put_att_int");

        byte[] nameBuffer = new byte[256];
        int[] dimIds = new int[1];
        InteropTestCommon.AssertSuccess(
            Native.nc_inq_var(hnd.Id, varId, nameBuffer, out NCType type, out int ndims, dimIds, out int natts),
            "nc_inq_var");

        Assert.Equal(NCType.NC_FLOAT, type);
        Assert.Equal(1, ndims);
        Assert.Equal(1, natts);
        Assert.Equal(dimId, dimIds[0]);
        Assert.Equal("temperature", DecodeCString(nameBuffer));

        byte[] varNameOnly = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_varname(hnd.Id, varId, varNameOnly), "nc_inq_varname");
        Assert.Equal("temperature", DecodeCString(varNameOnly));
    }

    [Fact]
    public void NcInqAttname_ReturnsAttributeNamesByIndex()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1, [42]), "nc_put_att_int(answer)");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_text(hnd.Id, InteropTestCommon.NcGlobal, "title", (nuint)5, "ocean"), "nc_put_att_text(title)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_natts(hnd.Id, out int ngatts), "nc_inq_natts");
        Assert.Equal(2, ngatts);

        byte[] first = new byte[256];
        byte[] second = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_attname(hnd.Id, InteropTestCommon.NcGlobal, 0, first), "nc_inq_attname(0)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_attname(hnd.Id, InteropTestCommon.NcGlobal, 1, second), "nc_inq_attname(1)");

        string[] names = [DecodeCString(first), DecodeCString(second)];
        Assert.Contains("answer", names);
        Assert.Contains("title", names);
    }

    [Fact]
    public void NcGetAttText_ReadsVariableTextAttribute()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)1, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        const string expected = "meters";
        InteropTestCommon.AssertSuccess(Native.nc_put_att_text(hnd.Id, varId, "units", (nuint)expected.Length, expected), "nc_put_att_text");

        byte[] data = new byte[expected.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_text(hnd.Id, varId, "units", data), "nc_get_att_text");
        Assert.Equal(expected, Encoding.ASCII.GetString(data));
    }

    [Fact]
    public void NcPutAttString_RoundTripWithNcGetAttString_AndFree()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        string[] expected = ["alpha", "beta"];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_string(hnd.Id, InteropTestCommon.NcGlobal, "labels", (nuint)expected.Length, expected),
            "nc_put_att_string");

        InteropTestCommon.AssertSuccess(Native.nc_inq_att(hnd.Id, InteropTestCommon.NcGlobal, "labels", out NCType type, out nuint len), "nc_inq_att");
        Assert.Equal(NCType.NC_STRING, type);
        Assert.Equal((nuint)2, len);

        IntPtr[] ptrs = new IntPtr[expected.Length];
        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_get_att_string(hnd.Id, InteropTestCommon.NcGlobal, "labels", ptrs), "nc_get_att_string");
            string[] actual = ptrs.Select(p => Marshal.PtrToStringAnsi(p) ?? string.Empty).ToArray();
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_free_string((nuint)ptrs.Length, ptrs), "nc_free_string");
        }
    }

    [Fact(Skip = "nc_show_metadata writes to native stdio; test is kept but skipped by default to avoid noisy/non-portable output suppression.")]
    public void NcShowMetadata_ReturnsSuccessWhenAvailable()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)2, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out _), "nc_def_var");

        int status = Native.nc_show_metadata(hnd.Id);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_show_metadata");
    }

    private static string DecodeCString(byte[] bytes)
    {
        int nul = Array.IndexOf(bytes, (byte)0);
        int len = nul >= 0 ? nul : bytes.Length;
        return Encoding.ASCII.GetString(bytes, 0, len);
    }
}
