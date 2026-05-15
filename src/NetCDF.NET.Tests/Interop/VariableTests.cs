using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class VariableTests
{
    [Fact]
    public void DefineVariable_InquiryMatches()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)5, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "v", out int lookedUpVarId), "nc_inq_varid");
        Assert.Equal(varId, lookedUpVarId);

        InteropTestCommon.AssertSuccess(Native.nc_inq_vartype(hnd.Id, varId, out NCType varType), "nc_inq_vartype");
        Assert.Equal(NCType.NC_INT, varType);

        InteropTestCommon.AssertSuccess(Native.nc_inq_varndims(hnd.Id, varId, out int ndims), "nc_inq_varndims");
        Assert.Equal(1, ndims);

        int[] dimIds = new int[ndims];
        InteropTestCommon.AssertSuccess(Native.nc_inq_vardimid(hnd.Id, varId, dimIds), "nc_inq_vardimid");
        Assert.Equal(dimId, dimIds[0]);
    }
}
