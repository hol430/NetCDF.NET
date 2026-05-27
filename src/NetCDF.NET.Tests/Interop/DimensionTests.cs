using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class DimensionTests
{
    [Fact]
    public void DefineAndInquireDimension_RoundTrip()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", (nuint)4, out int dimid), "nc_def_dim");

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimid(hnd.Id, "time", out int lookedUpDimId), "nc_inq_dimid");
        Assert.Equal(dimid, lookedUpDimId);

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimlen(hnd.Id, dimid, out nuint len), "nc_inq_dimlen");
        Assert.Equal((nuint)4, len);
    }

    [Fact]
    public void InqUnlimdims_UsesTwoCallPattern()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", 0, out int unlimDimId), "nc_def_dim(unlimited)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(hnd.Id, out int count, null), "nc_inq_unlimdims(count)");
        Assert.Equal(1, count);

        int[] ids = new int[count];
        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(hnd.Id, out int countAgain, ids), "nc_inq_unlimdims(values)");
        Assert.Equal(count, countAgain);
        Assert.Equal(unlimDimId, ids[0]);
    }

    [Fact]
    public void InqUnlimdim_ReturnsSingleUnlimitedId()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", 0, out int unlimDimId), "nc_def_dim(unlimited)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdim(hnd.Id, out int lookedUp), "nc_inq_unlimdim");

        Assert.Equal(unlimDimId, lookedUp);
    }

    [Fact]
    public void InqDim_ReturnsNameAndLength()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", (nuint)4, out int dimid), "nc_def_dim");

        var name = new StringBuilder(256);
        InteropTestCommon.AssertSuccess(Native.nc_inq_dim(hnd.Id, dimid, name, out nuint len), "nc_inq_dim");

        Assert.Equal("time", name.ToString());
        Assert.Equal((nuint)4, len);
    }

    [Fact]
    public void UnlimitedDimension_WithTwoDimVariable_AppendsRecordsAndUpdatesLength()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "time", 0, out int timeDimId), "nc_def_dim(time)");
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)3, out int xDimId), "nc_def_dim(x)");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "values", NCType.NC_INT, 2, [timeDimId, xDimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        IntPtr[] firstStart = [(IntPtr)0, (IntPtr)0];
        IntPtr[] firstCount = [(IntPtr)1, (IntPtr)3];
        int[] firstRow = [1, 2, 3];
        InteropTestCommon.AssertSuccess(Native.nc_put_vara_int(hnd.Id, varId, firstStart, firstCount, firstRow), "nc_put_vara_int(first)");

        IntPtr[] secondStart = [(IntPtr)1, (IntPtr)0];
        IntPtr[] secondCount = [(IntPtr)1, (IntPtr)3];
        int[] secondRow = [4, 5, 6];
        InteropTestCommon.AssertSuccess(Native.nc_put_vara_int(hnd.Id, varId, secondStart, secondCount, secondRow), "nc_put_vara_int(second)");

        hnd.CloseHandle();

        using NcFileHandle read = NcFileHandle.Open(hnd.Path, OpenMode.NC_NOWRITE);
        InteropTestCommon.AssertSuccess(Native.nc_inq_dimid(read.Id, "time", out timeDimId), "nc_inq_dimid(time)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_dimlen(read.Id, timeDimId, out nuint timeLen), "nc_inq_dimlen(time)");
        Assert.Equal((nuint)2, timeLen);

        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(read.Id, out int unlimCount, null), "nc_inq_unlimdims(count)");
        Assert.Equal(1, unlimCount);

        int[] unlimIds = new int[unlimCount];
        InteropTestCommon.AssertSuccess(Native.nc_inq_unlimdims(read.Id, out int unlimCountAgain, unlimIds), "nc_inq_unlimdims(values)");
        Assert.Equal(unlimCount, unlimCountAgain);
        Assert.Equal(timeDimId, unlimIds[0]);

        InteropTestCommon.AssertSuccess(Native.nc_inq_varid(read.Id, "values", out varId), "nc_inq_varid");
        int[] actual = new int[6];
        InteropTestCommon.AssertSuccess(Native.nc_get_var_int(read.Id, varId, actual), "nc_get_var_int");
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, actual);
    }
}
