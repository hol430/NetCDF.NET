using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class GroupTests
{
    [Fact]
    public void InqGrps_UsesTwoCallPattern_AndFindsDefinedChildren()
    {
        using NcTempFile hnd = new();

        int status = Native.nc_def_grp(hnd.Id, "g1", out int g1Id);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(g1)");
        InteropTestCommon.AssertSuccess(Native.nc_def_grp(hnd.Id, "g2", out int g2Id), "nc_def_grp(g2)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_grps(hnd.Id, out int count, null), "nc_inq_grps(count)");
        Assert.Equal(2, count);

        int[] groupIds = new int[count];
        InteropTestCommon.AssertSuccess(Native.nc_inq_grps(hnd.Id, out int countAgain, groupIds), "nc_inq_grps(values)");
        Assert.Equal(count, countAgain);
        Assert.Contains(g1Id, groupIds);
        Assert.Contains(g2Id, groupIds);
    }

    [Fact]
    public void GroupParentAndLookup_InquiriesRoundTrip()
    {
        using NcTempFile hnd = new();

        int status = Native.nc_def_grp(hnd.Id, "parent", out int parentId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(parent)");
        InteropTestCommon.AssertSuccess(Native.nc_def_grp(parentId, "child", out int childId), "nc_def_grp(child)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_parent(parentId, out int rootId), "nc_inq_grp_parent(parent)");
        Assert.Equal(hnd.Id, rootId);

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_parent(childId, out int lookedUpParentId), "nc_inq_grp_parent(child)");
        Assert.Equal(parentId, lookedUpParentId);

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_ncid(hnd.Id, "parent", out int parentByName), "nc_inq_grp_ncid(parent)");
        Assert.Equal(parentId, parentByName);

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_ncid(parentId, "child", out int childByName), "nc_inq_grp_ncid(child)");
        Assert.Equal(childId, childByName);

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_full_ncid(hnd.Id, "/parent/child", out int childByFullPath), "nc_inq_grp_full_ncid");
        Assert.Equal(childId, childByFullPath);

        byte[] groupName = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_grpname(childId, groupName), "nc_inq_grpname");
        Assert.Equal("child", DecodeCString(groupName));
    }

    [Fact]
    public void InqNcid_ResolvesChildAndRootGroups()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_def_grp(hnd.Id, "parent", out int parentId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(parent)");
        InteropTestCommon.AssertSuccess(Native.nc_def_grp(parentId, "child", out int childId), "nc_def_grp(child)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_ncid(hnd.Id, "parent", out int parentLookup), "nc_inq_ncid(parent)");
        Assert.Equal(parentId, parentLookup);

        InteropTestCommon.AssertSuccess(Native.nc_inq_ncid(parentId, "child", out int childLookup), "nc_inq_ncid(child)");
        Assert.Equal(childId, childLookup);

        InteropTestCommon.AssertSuccess(Native.nc_inq_ncid(childId, null!, out int rootLookup), "nc_inq_ncid(NULL)");
        Assert.Equal(hnd.Id, rootLookup);
    }

    [Fact]
    public void InqGrpnameLenAndFull_ReturnExpectedPath()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_def_grp(hnd.Id, "parent", out int parentId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(parent)");
        InteropTestCommon.AssertSuccess(Native.nc_def_grp(parentId, "child", out int childId), "nc_def_grp(child)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_grpname_len(childId, out nuint len), "nc_inq_grpname_len(child)");
        Assert.Equal((nuint)13, len); // "/parent/child"

        byte[] fullName = new byte[(int)len + 1];
        InteropTestCommon.AssertSuccess(Native.nc_inq_grpname_full(childId, out nuint lenAgain, fullName), "nc_inq_grpname_full(child)");
        Assert.Equal(len, lenAgain);
        Assert.Equal("/parent/child", DecodeCString(fullName));
    }

    [Fact]
    public void InqDimids_ReturnsOwnAndParentDimensions()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "root_dim", (nuint)4, out int rootDimId), "nc_def_dim(root)");
        int status = Native.nc_def_grp(hnd.Id, "child", out int childGroupId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(child)");
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(childGroupId, "child_dim", (nuint)2, out int childDimId), "nc_def_dim(child)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimids(childGroupId, out int ownCount, null!, 0), "nc_inq_dimids(own count)");
        int[] ownDimIds = new int[ownCount];
        InteropTestCommon.AssertSuccess(Native.nc_inq_dimids(childGroupId, out int ownCountAgain, ownDimIds, 0), "nc_inq_dimids(own values)");
        Assert.Equal(1, ownCount);
        Assert.Equal(ownCount, ownCountAgain);
        Assert.Equal(childDimId, ownDimIds[0]);

        InteropTestCommon.AssertSuccess(Native.nc_inq_dimids(childGroupId, out int allCount, null!, 1), "nc_inq_dimids(with-parents count)");
        int[] allDimIds = new int[allCount];
        InteropTestCommon.AssertSuccess(Native.nc_inq_dimids(childGroupId, out int allCountAgain, allDimIds, 1), "nc_inq_dimids(with-parents values)");
        Assert.Equal(2, allCount);
        Assert.Equal(allCount, allCountAgain);
        Assert.Contains(rootDimId, allDimIds.AsSpan(0, allCount).ToArray());
        Assert.Contains(childDimId, allDimIds.AsSpan(0, allCount).ToArray());
    }

    [Fact]
    public void InqVarids_ReturnsGroupVariablesOnly()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "root_dim", (nuint)3, out int rootDimId), "nc_def_dim(root)");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "root_v", NCType.NC_INT, 1, [rootDimId], out _), "nc_def_var(root)");

        int status = Native.nc_def_grp(hnd.Id, "child", out int childGroupId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(child)");
        InteropTestCommon.AssertSuccess(Native.nc_def_dim(childGroupId, "child_dim", (nuint)2, out int childDimId), "nc_def_dim(child)");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(childGroupId, "child_v1", NCType.NC_INT, 1, [childDimId], out int childVarId1), "nc_def_var(child_v1)");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(childGroupId, "child_v2", NCType.NC_FLOAT, 1, [childDimId], out int childVarId2), "nc_def_var(child_v2)");

        InteropTestCommon.AssertSuccess(Native.nc_inq_varids(childGroupId, out int childVarCount, null!), "nc_inq_varids(child count)");
        int[] childVarIds = new int[childVarCount];
        InteropTestCommon.AssertSuccess(Native.nc_inq_varids(childGroupId, out int childVarCountAgain, childVarIds), "nc_inq_varids(child values)");
        Assert.Equal(2, childVarCount);
        Assert.Equal(childVarCount, childVarCountAgain);
        Assert.Contains(childVarId1, childVarIds.AsSpan(0, childVarCount).ToArray());
        Assert.Contains(childVarId2, childVarIds.AsSpan(0, childVarCount).ToArray());
    }

    [Fact]
    public void RenameGroup_UpdatesNameAndPathLookups()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4);

        int status = Native.nc_def_grp(hnd.Id, "old_name", out int grpId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_def_grp(old_name)");
        InteropTestCommon.AssertSuccess(Native.nc_rename_grp(grpId, "new_name"), "nc_rename_grp");

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_ncid(hnd.Id, "new_name", out int lookedUpByName), "nc_inq_grp_ncid(new_name)");
        Assert.Equal(grpId, lookedUpByName);

        InteropTestCommon.AssertSuccess(Native.nc_inq_grp_full_ncid(hnd.Id, "/new_name", out int lookedUpByFullPath), "nc_inq_grp_full_ncid(/new_name)");
        Assert.Equal(grpId, lookedUpByFullPath);

        int oldNameStatus = Native.nc_inq_grp_ncid(hnd.Id, "old_name", out _);
        Assert.NotEqual(InteropTestCommon.NcNoErr, oldNameStatus);

        byte[] groupName = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_grpname(grpId, groupName), "nc_inq_grpname(new_name)");
        Assert.Equal("new_name", DecodeCString(groupName));
    }

    private static string DecodeCString(byte[] bytes)
    {
        int nul = Array.IndexOf(bytes, (byte)0);
        int len = nul >= 0 ? nul : bytes.Length;
        return System.Text.Encoding.ASCII.GetString(bytes, 0, len);
    }
}
