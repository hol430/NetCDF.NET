namespace NetCDF.LowLevel;

/// <summary>
/// Describes global chunk cache settings.
/// </summary>
/// <param name="Size">The cache size in bytes.</param>
/// <param name="ElementCount">The number of cache elements.</param>
/// <param name="Preemption">The cache preemption value.</param>
public readonly record struct ChunkCacheInfo(nuint Size, nuint ElementCount, float Preemption);
