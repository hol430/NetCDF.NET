using NC.Net.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class VariableTests
{
    [Fact]
    public void DefineVariable_InquiryMatches()
    {
        using var temp = new TempFile();
        int ncid = -1;

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_create(temp.FilePath, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)5, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(ncid, "v", out int lookedUpVarId), "nc_inq_varid");
            Assert.Equal(varId, lookedUpVarId);

            InteropTestCommon.AssertSuccess(Native.nc_inq_vartype(ncid, varId, out NCType varType), "nc_inq_vartype");
            Assert.Equal(NCType.NC_INT, varType);

            InteropTestCommon.AssertSuccess(Native.nc_inq_varndims(ncid, varId, out int ndims), "nc_inq_varndims");
            Assert.Equal(1, ndims);

            int[] dimIds = new int[ndims];
            InteropTestCommon.AssertSuccess(Native.nc_inq_vardimid(ncid, varId, dimIds), "nc_inq_vardimid");
            Assert.Equal(dimId, dimIds[0]);
        }
        finally
        {
            InteropTestCommon.CloseIfOpen(ref ncid);
        }
    }
}
