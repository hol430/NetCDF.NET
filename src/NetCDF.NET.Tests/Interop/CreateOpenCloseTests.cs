using System.Formats.Asn1;
using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class CreateOpenCloseTests
{
    [Fact]
    public void NcInqLibvers_ReturnsNonEmptyString()
    {
        string version = Marshal.PtrToStringUTF8(Native.nc_inq_libvers()) ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.Contains('.', version);
    }

    [Fact]
    public void NcStrerror_ForNegativeCode_ReturnsMessage()
    {
        string message = Marshal.PtrToStringUTF8(Native.nc_strerror(-1)) ?? string.Empty;
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void NcOpen_MissingFile_ReturnsError()
    {
        using var temp = new TempFile();
        int status = Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Theory]
    [MemberData(nameof(NetcdfTestFormats.AllFormats), MemberType = typeof(NetcdfTestFormats))]
    public void NcCreateSyncClose_Reopen_Succeeds(NetcdfTestFormat format)
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
                Native.nc_create(temp.FilePath, format.CreateMode, out ncid),
                "nc_create",
                format.FeatureName,
                InteropTestCommon.NcEinval);
            InteropTestCommon.AssertSuccess(Native.nc_enddef(ncid), "nc_enddef");
            InteropTestCommon.AssertSuccess(Native.nc_sync(ncid), "nc_sync");
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close(create)");
            ncid = -1;

            InteropTestCommon.AssertSuccess(Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out ncid), "nc_open");
        }
        finally
        {
            if (ncid != -1)
            {
                InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close(final)");
            }
        }

        Assert.True(File.Exists(temp.FilePath));
    }

    [Fact]
    public void NcInq_ReturnsExpectedAggregateCounts()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", (nuint)3, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "values", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "units", NCType.NC_INT, 1, [1]), "nc_put_att_int(var)");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1, [42]), "nc_put_att_int(global)");

        InteropTestCommon.AssertSuccess(Native.nc_inq(hnd.Id, out int ndims, out int nvars, out int ngatts, out int unlimdimid), "nc_inq");
        Assert.Equal(1, ndims);
        Assert.Equal(1, nvars);
        Assert.Equal(1, ngatts);
        Assert.Equal(-1, unlimdimid);
    }

    [Theory]
    [MemberData(nameof(NetcdfTestFormats.AllFormats), MemberType = typeof(NetcdfTestFormats))]
    public void NcInqFormat_APIs_ReturnSaneValues(NetcdfTestFormat format)
    {
        using NcTempFile hnd = new(format);

        InteropTestCommon.AssertSuccess(Native.nc_inq_format(hnd.Id, out int actualFormat), "nc_inq_format");
        Assert.True(actualFormat > 0);

        InteropTestCommon.AssertSuccess(Native.nc_inq_format_extended(hnd.Id, out int extFormat, out int mode), "nc_inq_format_extended");
        Assert.True(extFormat > 0);
        Assert.True(mode >= 0);
    }

    [Fact]
    public void NcRedef_AllowsReturningToDefineMode()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)3, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef(first)");
            InteropTestCommon.AssertSuccess(Native.nc_redef(hnd.Id), "nc_redef");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out _), "nc_def_var(after redef)");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef(second)");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        InteropTestCommon.AssertSuccess(Native.nc_inq_varid(read.Id, "v", out _), "nc_inq_varid(after reopen)");
    }

    [Fact]
    public void NcInqNdimsAndNvars_ReturnExpectedCounts()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)2, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out _), "nc_def_var");

        InteropTestCommon.AssertSuccess(Native.nc_inq_ndims(hnd.Id, out int ndims), "nc_inq_ndims");
        Assert.Equal(1, ndims);

        InteropTestCommon.AssertSuccess(Native.nc_inq_nvars(hnd.Id, out int nvars), "nc_inq_nvars");
        Assert.Equal(1, nvars);
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
    public void NcAbort_AfterCreateInvalidatesHandle()
    {
        using var temp = new TempFile();

        InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_CLOBBER, out int ncid), "nc_create");
        InteropTestCommon.AssertSuccess(Native.nc_abort(ncid), "nc_abort");

        int statusAfterAbort = Native.nc_inq_ndims(ncid, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, statusAfterAbort);
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
}
