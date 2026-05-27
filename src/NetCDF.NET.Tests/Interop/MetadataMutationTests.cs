using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class MetadataMutationTests
{
    [Fact]
    public void RenameDim_UpdatesLookupByName()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)3, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_rename_dim(hnd.Id, dimId, "x_renamed"), "nc_rename_dim");

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimid(hnd.Id, "x_renamed", out int renamedId), "nc_inq_dimid(renamed)");
        Assert.Equal(dimId, renamedId);

        int oldNameStatus = Native.nc_inq_dimid(hnd.Id, "x", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, oldNameStatus);
    }

    [Fact]
    public void RenameVar_UpdatesLookupByName_AndPreservesData()
    {
        using var temp = new TempFile();

        using (NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)4, out int dimId), "nc_def_dim");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

            int[] expected = [2, 4, 6, 8];
            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(hnd.Id, varId, expected), "nc_put_var_int");

            InteropTestCommon.AssertSuccess(Native.nc_redef(hnd.Id), "nc_redef");
            InteropTestCommon.AssertSuccess(Native.nc_rename_var(hnd.Id, varId, "v_renamed"), "nc_rename_var");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef(after rename)");
        }

        using (NcFileHandle hnd = NcFileHandle.Open(temp.FilePath, OpenMode.NC_NOWRITE))
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "v_renamed", out int renamedId), "nc_inq_varid(renamed)");

            int[] actual = new int[4];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_int(hnd.Id, renamedId, actual), "nc_get_var_int");
            Assert.Equal(new[] { 2, 4, 6, 8 }, actual);

            int oldNameStatus = Native.nc_inq_varid(hnd.Id, "v", out _);
            Assert.NotEqual(InteropTestCommon.NcNoErr, oldNameStatus);
        }
    }

    [Fact]
    public void RenameAtt_UpdatesLookupByName_AndPreservesValue()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer", NCType.NC_INT, 1, [42]),
            "nc_put_att_int");

        InteropTestCommon.AssertSuccess(
            Native.nc_rename_att(hnd.Id, InteropTestCommon.NcGlobal, "answer", "answer_renamed"),
            "nc_rename_att");

        InteropTestCommon.AssertSuccess(
            Native.nc_inq_attid(hnd.Id, InteropTestCommon.NcGlobal, "answer_renamed", out _),
            "nc_inq_attid(renamed)");

        int[] actual = new int[1];
        InteropTestCommon.AssertSuccess(
            Native.nc_get_att_int(hnd.Id, InteropTestCommon.NcGlobal, "answer_renamed", actual),
            "nc_get_att_int(renamed)");
        Assert.Equal(new[] { 42 }, actual);

        int oldNameStatus = Native.nc_inq_attid(hnd.Id, InteropTestCommon.NcGlobal, "answer", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, oldNameStatus);
    }

    [Fact]
    public void DelAtt_RemovesAttributeAndDecrementsGlobalCount()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "a1", NCType.NC_INT, 1, [1]),
            "nc_put_att_int(a1)");
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_int(hnd.Id, InteropTestCommon.NcGlobal, "a2", NCType.NC_INT, 1, [2]),
            "nc_put_att_int(a2)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_natts(hnd.Id, out int before), "nc_inq_natts(before)");
        Assert.Equal(2, before);

        InteropTestCommon.AssertSuccess(Native.nc_del_att(hnd.Id, InteropTestCommon.NcGlobal, "a1"), "nc_del_att");

        InteropTestCommon.AssertSuccess(Native.nc_inq_natts(hnd.Id, out int after), "nc_inq_natts(after)");
        Assert.Equal(1, after);

        int deletedStatus = Native.nc_inq_attid(hnd.Id, InteropTestCommon.NcGlobal, "a1", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, deletedStatus);

        InteropTestCommon.AssertSuccess(Native.nc_inq_attid(hnd.Id, InteropTestCommon.NcGlobal, "a2", out _), "nc_inq_attid(a2)");
    }

    [Fact]
    public void CopyAtt_CopiesAttributeToAnotherVariable()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)1, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "src", NCType.NC_INT, 1, [dimId], out int srcVarId), "nc_def_var(src)");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "dst", NCType.NC_INT, 1, [dimId], out int dstVarId), "nc_def_var(dst)");

        int[] expected = [7, 11];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_int(hnd.Id, srcVarId, "pair", NCType.NC_INT, 2, expected),
            "nc_put_att_int(src)");

        InteropTestCommon.AssertSuccess(
            Native.nc_copy_att(hnd.Id, srcVarId, "pair", hnd.Id, dstVarId),
            "nc_copy_att");

        int[] actual = new int[2];
        InteropTestCommon.AssertSuccess(
            Native.nc_get_att_int(hnd.Id, dstVarId, "pair", actual),
            "nc_get_att_int(dst)");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CopyVar_CopiesDefinitionAndDataToAnotherFile()
    {
        using var source = new TempFile();
        using var dest = new TempFile();

        int sourceVarId;
        using (NcFileHandle src = NcFileHandle.Create(source.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(src.Id, "x", (nuint)5, out int dimId), "nc_def_dim(src)");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(src.Id, "values", NCType.NC_INT, 1, [dimId], out sourceVarId), "nc_def_var(src)");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(src.Id), "nc_enddef(src)");
            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(src.Id, sourceVarId, [3, 1, 4, 1, 5]), "nc_put_var_int(src)");
        }

        using (NcFileHandle src = NcFileHandle.Open(source.FilePath, OpenMode.NC_NOWRITE))
        using (NcFileHandle dst = NcFileHandle.Create(dest.FilePath, CreateMode.NC_NETCDF4))
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(dst.Id, "x", (nuint)5, out _), "nc_def_dim(dst)");
            InteropTestCommon.AssertSuccess(Native.nc_copy_var(src.Id, sourceVarId, dst.Id), "nc_copy_var");
        }

        using (NcFileHandle dst = NcFileHandle.Open(dest.FilePath, OpenMode.NC_NOWRITE))
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(dst.Id, "values", out int copiedVarId), "nc_inq_varid(dst)");
            int[] actual = new int[5];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_int(dst.Id, copiedVarId, actual), "nc_get_var_int(dst)");
            Assert.Equal(new[] { 3, 1, 4, 1, 5 }, actual);
        }
    }
}
