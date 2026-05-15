using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class AttributeTests
{
    [Fact]
    public void GlobalIntAttribute_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int[] expected = [42];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1, expected),
            "nc_put_att_int(global)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_attlen(hnd.Id, InteropTestCommon.NcGlobal, "answer", out nuint len), "nc_inq_attlen");
        Assert.Equal((nuint)1, len);

        InteropTestCommon.AssertSuccess(Native.nc_inq_atttype(hnd.Id, InteropTestCommon.NcGlobal, "answer", out NCType type), "nc_inq_atttype");
        Assert.Equal(NCType.NC_INT, type);

        int[] actual = new int[1];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", actual), "nc_get_att_int(global)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void VariableIntAttribute_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)2, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        int[] expected = [7, 11];
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "pair", NCType.NC_INT, 2, expected), "nc_put_att_int(var)");

        int[] actual = new int[2];
        InteropTestCommon.AssertSuccess(Native.nc_get_att_int(hnd.Id, varId, "pair", actual), "nc_get_att_int(var)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void InqNattAndVarNatts_ReturnExpectedCounts()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)3, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "g1", NCType.NC_INT, 1, [1]), "nc_put_att_int(global)");
        InteropTestCommon.AssertSuccess(Native.nc_put_att_int(hnd.Id, varId, "a1", NCType.NC_INT, 1, [1]), "nc_put_att_int(var)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_natts(hnd.Id, out int ngatts), "nc_inq_natts");
        Assert.Equal(1, ngatts);

        InteropTestCommon.AssertSuccess(Native.nc_inq_varnatts(hnd.Id, varId, out int varNatts), "nc_inq_varnatts");
        Assert.Equal(1, varNatts);
    }
}
