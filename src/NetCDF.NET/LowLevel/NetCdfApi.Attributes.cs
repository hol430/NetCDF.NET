using NetCDF.Interop;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Inquires metadata for an attribute.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for global attributes.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The attribute metadata.</returns>
    public AttributeInfo InquireAttribute(NetCdfHandle handle, VariableId variableId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}", nameof(Native.nc_inq_att), ncid, variableId.Value, name);

        int status = Native.nc_inq_att(ncid, variableId.Value, name, out NCType type, out nuint length);
        LogReturned(nameof(Native.nc_inq_att), status);
        Check(status, nameof(Native.nc_inq_att));

        return new AttributeInfo(type, length);
    }

    /// <summary>
    /// Inquires an attribute ID by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for global attributes.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The matching attribute ID.</returns>
    public AttributeId InquireAttributeId(NetCdfHandle handle, VariableId variableId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}", nameof(Native.nc_inq_attid), ncid, variableId.Value, name);

        int status = Native.nc_inq_attid(ncid, variableId.Value, name, out int attid);
        LogReturned(nameof(Native.nc_inq_attid), status);
        Check(status, nameof(Native.nc_inq_attid));

        return new AttributeId(attid);
    }

    /// <summary>
    /// Inquires an attribute name by numeric attribute index.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for global attributes.</param>
    /// <param name="attributeId">The attribute ID.</param>
    /// <returns>The attribute name.</returns>
    public string InquireAttributeName(NetCdfHandle handle, VariableId variableId, AttributeId attributeId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, attid={AttributeId}", nameof(Native.nc_inq_attname), ncid, variableId.Value, attributeId.Value);

        int status = Native.nc_inq_attname(ncid, variableId.Value, attributeId.Value, name);
        LogReturned(nameof(Native.nc_inq_attname), status);
        Check(status, nameof(Native.nc_inq_attname));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires the number of global attributes in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The number of global attributes.</returns>
    public int InquireGlobalAttributeCount(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_natts), ncid);

        int status = Native.nc_inq_natts(ncid, out int count);
        LogReturned(nameof(Native.nc_inq_natts), status);
        Check(status, nameof(Native.nc_inq_natts));

        return count;
    }

    /// <summary>
    /// Inquires an attribute data type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for global attributes.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The netCDF external data type.</returns>
    public NCType InquireAttributeType(NetCdfHandle handle, VariableId variableId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}", nameof(Native.nc_inq_atttype), ncid, variableId.Value, name);

        int status = Native.nc_inq_atttype(ncid, variableId.Value, name, out NCType type);
        LogReturned(nameof(Native.nc_inq_atttype), status);
        Check(status, nameof(Native.nc_inq_atttype));

        return type;
    }

    /// <summary>
    /// Inquires an attribute length.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for global attributes.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The number of attribute values.</returns>
    public nuint InquireAttributeLength(NetCdfHandle handle, VariableId variableId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}", nameof(Native.nc_inq_attlen), ncid, variableId.Value, name);

        int status = Native.nc_inq_attlen(ncid, variableId.Value, name, out nuint length);
        LogReturned(nameof(Native.nc_inq_attlen), status);
        Check(status, nameof(Native.nc_inq_attlen));

        return length;
    }

    /// <summary>
    /// Copies an attribute from one file or variable to another.
    /// </summary>
    /// <param name="source">The source open netCDF file handle.</param>
    /// <param name="sourceVariableId">The source variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="destination">The destination open netCDF file handle.</param>
    /// <param name="destinationVariableId">The destination variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    public void CopyAttribute(NetCdfHandle source, VariableId sourceVariableId, string name, NetCdfHandle destination, VariableId destinationVariableId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int sourceNcid = source.Id;
        int destinationNcid = destination.Id;
        logger?.LogDebug("{FunctionName}: sourceNcid={SourceNcid}, sourceVarid={SourceVariableId}, name={Name}, destinationNcid={DestinationNcid}, destinationVarid={DestinationVariableId}", nameof(Native.nc_copy_att), sourceNcid, sourceVariableId.Value, name, destinationNcid, destinationVariableId.Value);

        int status = Native.nc_copy_att(sourceNcid, sourceVariableId.Value, name, destinationNcid, destinationVariableId.Value);
        LogReturned(nameof(Native.nc_copy_att), status);
        Check(status, nameof(Native.nc_copy_att));
    }

    /// <summary>
    /// Deletes an attribute.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    public void DeleteAttribute(NetCdfHandle handle, VariableId variableId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}", nameof(Native.nc_del_att), ncid, variableId.Value, name);

        int status = Native.nc_del_att(ncid, variableId.Value, name);
        LogReturned(nameof(Native.nc_del_att), status);
        Check(status, nameof(Native.nc_del_att));
    }

    /// <summary>
    /// Renames an attribute.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The current attribute name.</param>
    /// <param name="newName">The new attribute name.</param>
    public void RenameAttribute(NetCdfHandle handle, VariableId variableId, string name, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, newName={NewName}", nameof(Native.nc_rename_att), ncid, variableId.Value, name, newName);

        int status = Native.nc_rename_att(ncid, variableId.Value, name, newName);
        LogReturned(nameof(Native.nc_rename_att), status);
        Check(status, nameof(Native.nc_rename_att));
    }
}
