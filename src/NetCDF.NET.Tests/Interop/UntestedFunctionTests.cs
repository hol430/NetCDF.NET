using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class UntestedFunctionTests
{
    [Fact]
    public void NcAbort_AfterCreateInvalidatesHandle()
    {
        using var temp = new TempFile();

        InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_CLOBBER, out int ncid), "nc_create");
        InteropTestCommon.AssertSuccess(Native.nc_abort(ncid), "nc_abort");

        int statusAfterAbort = Native.nc_inq_ndims(ncid, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, statusAfterAbort);
    }

    [Fact]
    public void NcCreateMem_CreatesDataset_WhenSupported()
    {
        string path = "mem.nc";
        int status = Native.nc_create_mem(path, CreateMode.NC_CLOBBER, 0, out int ncid);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_create_mem");

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)2, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out _), "nc_def_var");
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close");
        }
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void NcInqPath_ReturnsOpenedFilePath()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_inq_path(hnd.Id, out nuint len, null), "nc_inq_path(count)");
        Assert.True(len > 0);

        byte[] buffer = new byte[(int)len + 1];
        InteropTestCommon.AssertSuccess(Native.nc_inq_path(hnd.Id, out nuint lenAgain, buffer), "nc_inq_path(path)");
        Assert.Equal(len, lenAgain);

        string actual = Encoding.UTF8.GetString(buffer, 0, (int)lenAgain).TrimEnd('\0');
        Assert.Equal(hnd.Path, actual);
    }

    [Fact]
    public void NcInqType_ForAtomicInt_ReturnsExpectedNameAndSize()
    {
        using NcTempFile hnd = new();

        byte[] name = new byte[64];
        InteropTestCommon.AssertSuccess(Native.nc_inq_type(hnd.Id, NCType.NC_INT, name, out nuint size), "nc_inq_type");

        Assert.Equal((nuint)sizeof(int), size);
        string typeName = Encoding.ASCII.GetString(name).TrimEnd('\0').ToLowerInvariant();
        Assert.Contains("int", typeName);
    }

    [Fact]
    public void NcSetFill_TogglesAndReturnsPreviousMode()
    {
        using NcTempFile hnd = new();

        const int ncFill = 0;
        const int ncNoFill = 0x100;

        InteropTestCommon.AssertSuccess(Native.nc_set_fill(hnd.Id, ncNoFill, out int oldMode1), "nc_set_fill(no_fill)");
        InteropTestCommon.AssertSuccess(Native.nc_set_fill(hnd.Id, ncFill, out int oldMode2), "nc_set_fill(fill)");

        Assert.Equal(ncNoFill, oldMode2);
        Assert.True(oldMode1 == ncFill || oldMode1 == ncNoFill);
    }

    [Fact]
    public void NcInqDimname_ReturnsDimensionName()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", (nuint)4, out int dimId), "nc_def_dim");
        byte[] nameBuffer = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_dimname(hnd.Id, dimId, nameBuffer), "nc_inq_dimname");

        int nulIndex = Array.IndexOf(nameBuffer, (byte)0);
        int length = nulIndex >= 0 ? nulIndex : nameBuffer.Length;
        string actual = Encoding.ASCII.GetString(nameBuffer, 0, length);
        Assert.Equal("time", actual);
    }

    [Fact]
    public void NcDefVarFilter_InvalidFilterId_ReturnsErrorOrFeatureUnavailable()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)3, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        int status = Native.nc_def_var_filter(hnd.Id, varId, uint.MaxValue, 1, [42u]);
        if (status == InteropTestCommon.NcEnotBuilt || status == InteropTestCommon.NcEnotNc4 || status == InteropTestCommon.NcEfilter)
        {
            InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
                status,
                "nc_def_var_filter",
                InteropTestCommon.FeatureFilters,
                InteropTestCommon.NcEfilter);
        }

        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcGetVaraString_ReadsSlice_AndFreesReturnedStrings()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        string[] all = ["zero", "one", "two", "three"];
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)all.Length, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_STRING, 1, [dimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");
        InteropTestCommon.AssertSuccess(Native.nc_put_var_string(hnd.Id, varId, all), "nc_put_var_string");

        IntPtr[] ptrs = new IntPtr[2];
        try
        {
            InteropTestCommon.AssertSuccess(
                Native.nc_get_vara_string(hnd.Id, varId, [(nuint)1], [(nuint)2], ptrs),
                "nc_get_vara_string");

            string[] actual = ptrs.Select(p => Marshal.PtrToStringAnsi(p) ?? string.Empty).ToArray();
            Assert.Equal(["one", "two"], actual);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_free_string((nuint)ptrs.Length, ptrs), "nc_free_string");
        }
    }
}
