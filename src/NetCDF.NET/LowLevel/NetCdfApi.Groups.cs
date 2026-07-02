using NetCDF.Interop;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Defines a child group in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The group name.</param>
    /// <returns>The ID of the newly defined group.</returns>
    public GroupId DefineGroup(NetCdfHandle handle, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}", nameof(Native.nc_def_grp), ncid, name);

        int status = Native.nc_def_grp(ncid, name, out int groupNcid);
        LogReturned(nameof(Native.nc_def_grp), status);
        Check(status, nameof(Native.nc_def_grp));

        return new GroupId(groupNcid);
    }

    /// <summary>
    /// Inquires dimension IDs visible in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="includeParents">Whether to include parent group dimensions.</param>
    /// <returns>The visible dimension IDs.</returns>
    public IReadOnlyList<DimensionId> InquireDimensionIds(NetCdfHandle handle, bool includeParents)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, includeParents={IncludeParents}", nameof(Native.nc_inq_dimids), ncid, includeParents);

        int status = Native.nc_inq_dimids(ncid, out int count, null!, includeParents ? 1 : 0);
        LogReturned(nameof(Native.nc_inq_dimids), status);
        Check(status, nameof(Native.nc_inq_dimids));

        int[] ids = new int[count];
        status = Native.nc_inq_dimids(ncid, out _, ids, includeParents ? 1 : 0);
        LogReturned(nameof(Native.nc_inq_dimids), status);
        Check(status, nameof(Native.nc_inq_dimids));

        return ids.Select(static id => new DimensionId(id)).ToArray();
    }

    /// <summary>
    /// Inquires a group ID by full group path.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="fullName">The full group path.</param>
    /// <returns>The matching group ID.</returns>
    public GroupId InquireGroupByFullName(NetCdfHandle handle, string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, fullName={FullName}", nameof(Native.nc_inq_grp_full_ncid), ncid, fullName);

        int status = Native.nc_inq_grp_full_ncid(ncid, fullName, out int groupNcid);
        LogReturned(nameof(Native.nc_inq_grp_full_ncid), status);
        Check(status, nameof(Native.nc_inq_grp_full_ncid));

        return new GroupId(groupNcid);
    }

    /// <summary>
    /// Inquires a child group ID by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The group name.</param>
    /// <returns>The matching group ID.</returns>
    public GroupId InquireGroup(NetCdfHandle handle, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}", nameof(Native.nc_inq_grp_ncid), ncid, name);

        int status = Native.nc_inq_grp_ncid(ncid, name, out int groupNcid);
        LogReturned(nameof(Native.nc_inq_grp_ncid), status);
        Check(status, nameof(Native.nc_inq_grp_ncid));

        return new GroupId(groupNcid);
    }

    /// <summary>
    /// Inquires the parent group ID for a group.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <returns>The parent group ID.</returns>
    public GroupId InquireGroupParent(GroupId groupId)
    {
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_grp_parent), groupId.Value);

        int status = Native.nc_inq_grp_parent(groupId.Value, out int parentNcid);
        LogReturned(nameof(Native.nc_inq_grp_parent), status);
        Check(status, nameof(Native.nc_inq_grp_parent));

        return new GroupId(parentNcid);
    }

    /// <summary>
    /// Inquires a group name.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <returns>The group name.</returns>
    public string InquireGroupName(GroupId groupId)
    {
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_grpname), groupId.Value);

        int status = Native.nc_inq_grpname(groupId.Value, name);
        LogReturned(nameof(Native.nc_inq_grpname), status);
        Check(status, nameof(Native.nc_inq_grpname));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires the full path name for a group.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <returns>The full group path name.</returns>
    public string InquireGroupFullName(GroupId groupId)
    {
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_grpname_full), groupId.Value);

        int status = Native.nc_inq_grpname_len(groupId.Value, out nuint length);
        LogReturned(nameof(Native.nc_inq_grpname_len), status);
        Check(status, nameof(Native.nc_inq_grpname_len));

        byte[] name = new byte[(int)length + 1];
        status = Native.nc_inq_grpname_full(groupId.Value, out _, name);
        LogReturned(nameof(Native.nc_inq_grpname_full), status);
        Check(status, nameof(Native.nc_inq_grpname_full));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires the full path name length for a group.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <returns>The full group path name length.</returns>
    public nuint InquireGroupFullNameLength(GroupId groupId)
    {
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_grpname_len), groupId.Value);

        int status = Native.nc_inq_grpname_len(groupId.Value, out nuint length);
        LogReturned(nameof(Native.nc_inq_grpname_len), status);
        Check(status, nameof(Native.nc_inq_grpname_len));

        return length;
    }

    /// <summary>
    /// Inquires child group IDs for an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The child group IDs.</returns>
    public IReadOnlyList<GroupId> InquireGroups(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_grps), ncid);

        int status = Native.nc_inq_grps(ncid, out int count, null);
        LogReturned(nameof(Native.nc_inq_grps), status);
        Check(status, nameof(Native.nc_inq_grps));

        int[] ids = new int[count];
        status = Native.nc_inq_grps(ncid, out _, ids);
        LogReturned(nameof(Native.nc_inq_grps), status);
        Check(status, nameof(Native.nc_inq_grps));

        return ids.Select(static id => new GroupId(id)).ToArray();
    }

    /// <summary>
    /// Inquires a child group ID by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The group name.</param>
    /// <returns>The matching group ID.</returns>
    public GroupId InquireNcid(NetCdfHandle handle, string name)
        => InquireGroup(handle, name);

    /// <summary>
    /// Inquires user-defined type IDs visible in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The visible type IDs.</returns>
    public IReadOnlyList<NCType> InquireTypeIds(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_typeids), ncid);

        int status = Native.nc_inq_typeids(ncid, out int count, null!);
        LogReturned(nameof(Native.nc_inq_typeids), status);
        Check(status, nameof(Native.nc_inq_typeids));

        int[] ids = new int[count];
        status = Native.nc_inq_typeids(ncid, out _, ids);
        LogReturned(nameof(Native.nc_inq_typeids), status);
        Check(status, nameof(Native.nc_inq_typeids));

        return ids.Select(static id => (NCType)id).ToArray();
    }

    /// <summary>
    /// Inquires variable IDs visible in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The visible variable IDs.</returns>
    public IReadOnlyList<VariableId> InquireVariableIds(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_varids), ncid);

        int status = Native.nc_inq_varids(ncid, out int count, null!);
        LogReturned(nameof(Native.nc_inq_varids), status);
        Check(status, nameof(Native.nc_inq_varids));

        int[] ids = new int[count];
        status = Native.nc_inq_varids(ncid, out _, ids);
        LogReturned(nameof(Native.nc_inq_varids), status);
        Check(status, nameof(Native.nc_inq_varids));

        return ids.Select(static id => new VariableId(id)).ToArray();
    }

    /// <summary>
    /// Renames a group.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <param name="newName">The new group name.</param>
    public void RenameGroup(GroupId groupId, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}", nameof(Native.nc_rename_grp), groupId.Value, newName);

        int status = Native.nc_rename_grp(groupId.Value, newName);
        LogReturned(nameof(Native.nc_rename_grp), status);
        Check(status, nameof(Native.nc_rename_grp));
    }

    /// <summary>
    /// Writes libnetcdf metadata for an open file to the native library's diagnostic output.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    public void ShowMetadata(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_show_metadata), ncid);

        int status = Native.nc_show_metadata(ncid);
        LogReturned(nameof(Native.nc_show_metadata), status);
        Check(status, nameof(Native.nc_show_metadata));
    }
}
