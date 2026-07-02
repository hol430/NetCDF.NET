using NetCDF.Interop;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Sets the default file format used by libnetcdf.
    /// </summary>
    /// <param name="format">The requested default format value.</param>
    /// <returns>The previous default format value.</returns>
    public int SetDefaultFormat(int format)
    {
        logger?.LogDebug("{FunctionName}: format={Format}", nameof(Native.nc_set_default_format), format);

        int status = Native.nc_set_default_format(format, out int oldFormat);
        LogReturned(nameof(Native.nc_set_default_format), status);
        Check(status, nameof(Native.nc_set_default_format));

        return oldFormat;
    }

    /// <summary>
    /// Sets global chunk cache settings.
    /// </summary>
    /// <param name="size">The cache size in bytes.</param>
    /// <param name="elementCount">The number of cache elements.</param>
    /// <param name="preemption">The cache preemption value.</param>
    public void SetChunkCache(nuint size, nuint elementCount, float preemption)
    {
        logger?.LogDebug("{FunctionName}: size={Size}, elements={Elements}, preemption={Preemption}", nameof(Native.nc_set_chunk_cache), size, elementCount, preemption);

        int status = Native.nc_set_chunk_cache(size, elementCount, preemption);
        LogReturned(nameof(Native.nc_set_chunk_cache), status);
        Check(status, nameof(Native.nc_set_chunk_cache));
    }

    /// <summary>
    /// Inquires global chunk cache settings.
    /// </summary>
    /// <returns>The global chunk cache settings.</returns>
    public ChunkCacheInfo GetChunkCache()
    {
        logger?.LogDebug("{FunctionName}", nameof(Native.nc_get_chunk_cache));

        int status = Native.nc_get_chunk_cache(out nuint size, out nuint elements, out float preemption);
        LogReturned(nameof(Native.nc_get_chunk_cache), status);
        Check(status, nameof(Native.nc_get_chunk_cache));

        return new ChunkCacheInfo(size, elements, preemption);
    }
}
