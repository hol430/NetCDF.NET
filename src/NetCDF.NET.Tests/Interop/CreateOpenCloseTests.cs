using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class CreateOpenCloseTests
{
    [Fact]
    public void NcInqLibvers_ReturnsNonEmptyString()
    {
        string version = Native.nc_inq_libvers();
        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.Contains('.', version);
    }

    [Fact]
    public void NcStrerror_ForNegativeCode_ReturnsMessage()
    {
        string message = Native.nc_strerror(-1);
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void NcOpen_MissingFile_ReturnsError()
    {
        using var temp = new TempFile();
        int status = Native.nc_open(temp.FilePath, OpenMode.NC_NOWRITE, out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }

    [Fact]
    public void NcCreateSyncClose_Reopen_Succeeds()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
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

    [Fact]
    public void NcInqFormat_APIs_ReturnSaneValues()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_inq_format(hnd.Id, out int format), "nc_inq_format");
        Assert.True(format > 0);

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
}
