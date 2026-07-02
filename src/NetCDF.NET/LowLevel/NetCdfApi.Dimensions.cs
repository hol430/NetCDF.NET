using NetCDF.Interop;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Defines a new dimension in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The dimension name.</param>
    /// <param name="length">The dimension length.</param>
    /// <returns>The ID of the newly defined dimension.</returns>
    public DimensionId DefineDimension(NetCdfHandle handle, string name, nuint length)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}, length={Length}", nameof(Native.nc_def_dim), ncid, name, length);

        int status = Native.nc_def_dim(ncid, name, length, out int dimid);
        LogReturned(nameof(Native.nc_def_dim), status);
        Check(status, nameof(Native.nc_def_dim));

        return new DimensionId(dimid);
    }

    /// <summary>
    /// Inquires a dimension name and length.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="dimensionId">The dimension ID.</param>
    /// <returns>The dimension metadata.</returns>
    public DimensionInfo InquireDimension(NetCdfHandle handle, DimensionId dimensionId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, dimid={DimensionId}", nameof(Native.nc_inq_dim), ncid, dimensionId.Value);

        int status = Native.nc_inq_dim(ncid, dimensionId.Value, name, out nuint length);
        LogReturned(nameof(Native.nc_inq_dim), status);
        Check(status, nameof(Native.nc_inq_dim));

        return new DimensionInfo(NativeString.DecodeNullTerminatedUtf8(name), length);
    }

    /// <summary>
    /// Inquires a dimension ID by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The dimension name.</param>
    /// <returns>The matching dimension ID.</returns>
    public DimensionId InquireDimensionId(NetCdfHandle handle, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}", nameof(Native.nc_inq_dimid), ncid, name);

        int status = Native.nc_inq_dimid(ncid, name, out int dimid);
        LogReturned(nameof(Native.nc_inq_dimid), status);
        Check(status, nameof(Native.nc_inq_dimid));

        return new DimensionId(dimid);
    }

    /// <summary>
    /// Inquires a dimension length.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="dimensionId">The dimension ID.</param>
    /// <returns>The dimension length.</returns>
    public nuint InquireDimensionLength(NetCdfHandle handle, DimensionId dimensionId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, dimid={DimensionId}", nameof(Native.nc_inq_dimlen), ncid, dimensionId.Value);

        int status = Native.nc_inq_dimlen(ncid, dimensionId.Value, out nuint length);
        LogReturned(nameof(Native.nc_inq_dimlen), status);
        Check(status, nameof(Native.nc_inq_dimlen));

        return length;
    }

    /// <summary>
    /// Inquires a dimension name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="dimensionId">The dimension ID.</param>
    /// <returns>The dimension name.</returns>
    public string InquireDimensionName(NetCdfHandle handle, DimensionId dimensionId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, dimid={DimensionId}", nameof(Native.nc_inq_dimname), ncid, dimensionId.Value);

        int status = Native.nc_inq_dimname(ncid, dimensionId.Value, name);
        LogReturned(nameof(Native.nc_inq_dimname), status);
        Check(status, nameof(Native.nc_inq_dimname));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires the number of dimensions in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The number of dimensions.</returns>
    public int InquireDimensionCount(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_ndims), ncid);

        int status = Native.nc_inq_ndims(ncid, out int count);
        LogReturned(nameof(Native.nc_inq_ndims), status);
        Check(status, nameof(Native.nc_inq_ndims));

        return count;
    }

    /// <summary>
    /// Inquires the unlimited dimension ID, if one exists.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The unlimited dimension ID, or <see langword="null"/> when none exists.</returns>
    public DimensionId? InquireUnlimitedDimension(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_unlimdim), ncid);

        int status = Native.nc_inq_unlimdim(ncid, out int dimid);
        LogReturned(nameof(Native.nc_inq_unlimdim), status);
        Check(status, nameof(Native.nc_inq_unlimdim));

        return dimid == NcGlobal ? null : new DimensionId(dimid);
    }

    /// <summary>
    /// Inquires all unlimited dimension IDs visible in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The unlimited dimension IDs.</returns>
    public IReadOnlyList<DimensionId> InquireUnlimitedDimensions(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_unlimdims), ncid);

        int status = Native.nc_inq_unlimdims(ncid, out int count, null);
        LogReturned(nameof(Native.nc_inq_unlimdims), status);
        Check(status, nameof(Native.nc_inq_unlimdims));

        int[] ids = new int[count];
        status = Native.nc_inq_unlimdims(ncid, out _, ids);
        LogReturned(nameof(Native.nc_inq_unlimdims), status);
        Check(status, nameof(Native.nc_inq_unlimdims));

        return ids.Select(static id => new DimensionId(id)).ToArray();
    }

    /// <summary>
    /// Renames a dimension.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="dimensionId">The dimension ID.</param>
    /// <param name="newName">The new dimension name.</param>
    public void RenameDimension(NetCdfHandle handle, DimensionId dimensionId, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, dimid={DimensionId}, name={Name}", nameof(Native.nc_rename_dim), ncid, dimensionId.Value, newName);

        int status = Native.nc_rename_dim(ncid, dimensionId.Value, newName);
        LogReturned(nameof(Native.nc_rename_dim), status);
        Check(status, nameof(Native.nc_rename_dim));
    }
}
