using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class StringInteropTests
{
    [Fact]
    public void PutAndGetAttText_Global_RoundTrip()
    {
        using NcTempFile hnd = new();

        const string expected = "netcdf-title";
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_text(hnd.Id, InteropTestCommon.NcGlobal, "title", (nuint)expected.Length, expected),
            "nc_put_att_text");

        byte[] data = new byte[expected.Length];
        InteropTestCommon.AssertSuccess(
            Native.nc_get_att_text(hnd.Id, InteropTestCommon.NcGlobal, "title", data),
            "nc_get_att_text");

        string actual = Encoding.ASCII.GetString(data);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void PutAndGetVarString_RoundTrip_AndFree()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        string[] expected = ["alpha", "bravo", "charlie"];
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)expected.Length, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_STRING, 1, [dimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        InteropTestCommon.AssertSuccess(Native.nc_put_var_string(hnd.Id, varId, expected), "nc_put_var_string");

        IntPtr[] ptrs = new IntPtr[expected.Length];
        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_get_var_string(hnd.Id, varId, ptrs), "nc_get_var_string");
            string[] actual = ptrs.Select(p => Marshal.PtrToStringAnsi(p) ?? string.Empty).ToArray();
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_free_string((nuint)ptrs.Length, ptrs), "nc_free_string");
        }
    }

    [Fact]
    public void TextVariable_AllTextApis_RoundTrip()
    {
        using NcTempFile hnd = new();

        byte[] initial = [(byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f'];
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)initial.Length, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_CHAR, 1, [dimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        InteropTestCommon.AssertSuccess(Native.nc_put_var_text(hnd.Id, varId, initial), "nc_put_var_text");

        byte overwrite = (byte)'X';
        InteropTestCommon.AssertSuccess(Native.nc_put_var1_text(hnd.Id, varId, [(nuint)2], ref overwrite), "nc_put_var1_text");

        byte[] block = [(byte)'1', (byte)'2'];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_vara_text(hnd.Id, varId, [(nuint)3], [(nuint)2], block),
            "nc_put_vara_text");

        byte[] strideWrite = [(byte)'Q', (byte)'R'];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_vars_text(hnd.Id, varId, [(nuint)0], [(nuint)2], [(nint)2], strideWrite),
            "nc_put_vars_text");

        byte[] full = new byte[initial.Length];
        InteropTestCommon.AssertSuccess(Native.nc_get_var_text(hnd.Id, varId, full), "nc_get_var_text");

        InteropTestCommon.AssertSuccess(Native.nc_get_var1_text(hnd.Id, varId, [(nuint)2], out byte var1), "nc_get_var1_text");
        Assert.Equal(full[2], var1);

        byte[] subset = new byte[2];
        InteropTestCommon.AssertSuccess(
            Native.nc_get_vara_text(hnd.Id, varId, [(nuint)3], [(nuint)2], subset),
            "nc_get_vara_text");
        Assert.Equal(full[3], subset[0]);
        Assert.Equal(full[4], subset[1]);

        byte[] strided = new byte[2];
        InteropTestCommon.AssertSuccess(
            Native.nc_get_vars_text(hnd.Id, varId, [(nuint)0], [(nuint)2], [(nint)2], strided),
            "nc_get_vars_text");
        Assert.Equal(full[0], strided[0]);
        Assert.Equal(full[2], strided[1]);
    }
}
