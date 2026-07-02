using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using NetCDF.Interop;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.LowLevel;

/// <summary>
/// Low-level, exception-based wrapper around the raw netCDF native API.
/// </summary>
public sealed partial class NetCdfApi
{
    private const int NameBufferSize = 256;
    private readonly ILogger<NetCdfApi>? logger;

    public NetCdfApi()
        : this(null)
    {
    }

    public NetCdfApi(ILogger<NetCdfApi>? logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Inquires the native libnetcdf version string.
    /// </summary>
    /// <returns>The native libnetcdf version string.</returns>
    public string GetLibraryVersion()
    {
        logger?.LogDebug("{FunctionName}", nameof(Native.nc_inq_libvers));

        nint versionPtr = Native.nc_inq_libvers();
        string version = Marshal.PtrToStringUTF8(versionPtr) ?? string.Empty;

        logger?.LogTrace("{FunctionName} returned version: {Version}", nameof(Native.nc_inq_libvers), version);
        return version;
    }

    public NetCdfHandle Create(string path, CreateMode mode = CreateMode.NC_CLOBBER)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        logger?.LogDebug(
            "{FunctionName}: path={Path}, mode={Mode}",
            nameof(Native.nc_create),
            path,
            mode);

        int status = Native.nc_create(path, mode, out int ncid);
        LogReturned(nameof(Native.nc_create), status, ncid);
        Check(status, nameof(Native.nc_create));

        return new NetCdfHandle(ncid, this);
    }

    public NetCdfHandle Open(string path, OpenMode mode = OpenMode.NC_NOWRITE)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        logger?.LogDebug(
            "{FunctionName}: path={Path}, mode={Mode}",
            nameof(Native.nc_open),
            path,
            mode);

        int status = Native.nc_open(path, mode, out int ncid);
        LogReturned(nameof(Native.nc_open), status, ncid);
        Check(status, nameof(Native.nc_open));

        return new NetCdfHandle(ncid, this);
    }

    internal void Close(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug(
            "{FunctionName}: ncid={Ncid}",
            nameof(Native.nc_close),
            ncid);

        int status = Native.nc_close(ncid);
        LogReturned(nameof(Native.nc_close), status);
        Check(status, nameof(Native.nc_close));
    }

    public string GetErrorMessage(int statusCode)
    {
        logger?.LogDebug(
            "{FunctionName}: statusCode={StatusCode}",
            nameof(Native.nc_strerror),
            statusCode);

        nint messagePtr = Native.nc_strerror(statusCode);
        string message = Marshal.PtrToStringUTF8(messagePtr) ?? string.Empty;

        logger?.LogTrace(
            "{FunctionName} returned message: {Message}",
            nameof(Native.nc_strerror),
            message);

        return message;
    }

    private void LogReturned(string functionName, int status)
        => logger?.LogTrace(
            "{FunctionName} returned status {StatusCode}.",
            functionName,
            status);

    private void LogReturned(string functionName, int status, int ncid)
        => logger?.LogTrace(
            "{FunctionName} returned status {StatusCode}, ncid={Ncid}.",
            functionName,
            status,
            ncid);

    private void Check(int status, string functionName)
    {
        if (status == NcNoErr)
            return;

        throw new NetCdfException(status, functionName, GetErrorMessage(status));
    }
}
