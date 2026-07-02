using NetCDF.Interop;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Creates a new in-memory netCDF file.
    /// </summary>
    /// <param name="path">The filesystem path associated with the in-memory file.</param>
    /// <param name="mode">The file creation mode flags.</param>
    /// <param name="initialSize">The initial in-memory file size.</param>
    /// <returns>An owned handle for the created netCDF file.</returns>
    public NetCdfHandle CreateMemory(string path, CreateMode mode, nuint initialSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        logger?.LogDebug(
            "{FunctionName}: path={Path}, mode={Mode}, initialSize={InitialSize}",
            nameof(Native.nc_create_mem),
            path,
            mode,
            initialSize);

        int status = Native.nc_create_mem(path, mode, initialSize, out int ncid);
        LogReturned(nameof(Native.nc_create_mem), status, ncid);
        Check(status, nameof(Native.nc_create_mem));

        return new NetCdfHandle(ncid, this);
    }

    /// <summary>
    /// Creates a new netCDF file for parallel I/O.
    /// </summary>
    /// <param name="path">The path to the file to create.</param>
    /// <param name="mode">The file creation mode flags.</param>
    /// <param name="communicator">The MPI communicator.</param>
    /// <param name="info">The MPI info object.</param>
    /// <returns>An owned handle for the created netCDF file.</returns>
    public NetCdfHandle CreateParallel(string path, CreateMode mode, MpiCommunicator communicator, MpiInfo info)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        logger?.LogDebug("{FunctionName}: path={Path}, mode={Mode}, communicator={Communicator}, info={Info}", nameof(Native.nc_create_par), path, mode, communicator.Value, info.Value);

        int status = Native.nc_create_par(path, mode, communicator.Value, info.Value, out int ncid);
        LogReturned(nameof(Native.nc_create_par), status, ncid);
        Check(status, nameof(Native.nc_create_par));

        return new NetCdfHandle(ncid, this);
    }

    /// <summary>
    /// Opens an existing netCDF file for parallel I/O.
    /// </summary>
    /// <param name="path">The path to the file to open.</param>
    /// <param name="mode">The file open mode flags.</param>
    /// <param name="communicator">The MPI communicator.</param>
    /// <param name="info">The MPI info object.</param>
    /// <returns>An owned handle for the opened netCDF file.</returns>
    public NetCdfHandle OpenParallel(string path, OpenMode mode, MpiCommunicator communicator, MpiInfo info)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        logger?.LogDebug("{FunctionName}: path={Path}, mode={Mode}, communicator={Communicator}, info={Info}", nameof(Native.nc_open_par), path, mode, communicator.Value, info.Value);

        int status = Native.nc_open_par(path, mode, communicator.Value, info.Value, out int ncid);
        LogReturned(nameof(Native.nc_open_par), status, ncid);
        Check(status, nameof(Native.nc_open_par));

        return new NetCdfHandle(ncid, this);
    }

    /// <summary>
    /// Aborts changes made to an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    public void Abort(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_abort), ncid);

        int status = Native.nc_abort(ncid);
        LogReturned(nameof(Native.nc_abort), status);
        Check(status, nameof(Native.nc_abort));
        handle.MarkClosed();
    }

    /// <summary>
    /// Leaves define mode for an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    public void EndDefineMode(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_enddef), ncid);

        int status = Native.nc_enddef(ncid);
        LogReturned(nameof(Native.nc_enddef), status);
        Check(status, nameof(Native.nc_enddef));
    }

    /// <summary>
    /// Puts an open netCDF file into define mode.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    public void Redefine(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_redef), ncid);

        int status = Native.nc_redef(ncid);
        LogReturned(nameof(Native.nc_redef), status);
        Check(status, nameof(Native.nc_redef));
    }

    /// <summary>
    /// Synchronizes an open netCDF file to disk.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    public void Sync(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_sync), ncid);

        int status = Native.nc_sync(ncid);
        LogReturned(nameof(Native.nc_sync), status);
        Check(status, nameof(Native.nc_sync));
    }

    /// <summary>
    /// Inquires counts and unlimited-dimension information for an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>Counts and unlimited-dimension information for the file.</returns>
    public NetCdfFileInfo Inquire(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq), ncid);

        int status = Native.nc_inq(ncid, out int ndims, out int nvars, out int ngatts, out int unlimdimid);
        LogReturned(nameof(Native.nc_inq), status);
        Check(status, nameof(Native.nc_inq));

        DimensionId? unlimited = unlimdimid == NcGlobal ? null : new DimensionId(unlimdimid);
        return new NetCdfFileInfo(ndims, nvars, ngatts, unlimited);
    }

    /// <summary>
    /// Inquires the netCDF format value for an open file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The netCDF format value reported by libnetcdf.</returns>
    public int InquireFormat(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_format), ncid);

        int status = Native.nc_inq_format(ncid, out int format);
        LogReturned(nameof(Native.nc_inq_format), status);
        Check(status, nameof(Native.nc_inq_format));

        return format;
    }

    /// <summary>
    /// Inquires extended format and mode information for an open file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The extended format and mode information.</returns>
    public NetCdfFormatInfo InquireFormatExtended(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_format_extended), ncid);

        int status = Native.nc_inq_format_extended(ncid, out int format, out int mode);
        LogReturned(nameof(Native.nc_inq_format_extended), status);
        Check(status, nameof(Native.nc_inq_format_extended));

        return new NetCdfFormatInfo(format, mode);
    }

    /// <summary>
    /// Inquires the path used to open or create a netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The path associated with the file.</returns>
    public string InquirePath(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_path), ncid);

        int status = Native.nc_inq_path(ncid, out nuint pathLength, null);
        LogReturned(nameof(Native.nc_inq_path), status);
        Check(status, nameof(Native.nc_inq_path));

        byte[] path = new byte[(int)pathLength + 1];
        status = Native.nc_inq_path(ncid, out _, path);
        LogReturned(nameof(Native.nc_inq_path), status);
        Check(status, nameof(Native.nc_inq_path));

        return NativeString.DecodeNullTerminatedUtf8(path);
    }

    /// <summary>
    /// Sets fill behavior for an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="fillMode">The requested fill behavior.</param>
    /// <returns>The previous fill behavior.</returns>
    public FillMode SetFill(NetCdfHandle handle, FillMode fillMode)
    {
        int ncid = handle.Id;
        logger?.LogDebug(
            "{FunctionName}: ncid={Ncid}, fillMode={FillMode}",
            nameof(Native.nc_set_fill),
            ncid,
            fillMode);

        int status = Native.nc_set_fill(ncid, (int)fillMode, out int oldMode);
        LogReturned(nameof(Native.nc_set_fill), status);
        Check(status, nameof(Native.nc_set_fill));

        return (FillMode)oldMode;
    }
}
