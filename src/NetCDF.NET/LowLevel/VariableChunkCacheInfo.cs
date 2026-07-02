namespace NetCDF.LowLevel;

/// <summary>
/// Describes per-variable chunk cache settings.
/// </summary>
/// <param name="Size">The cache size in bytes.</param>
/// <param name="ElementCount">The number of cache elements.</param>
/// <param name="Preemption">The cache preemption value.</param>
public readonly record struct VariableChunkCacheInfo(nuint Size, nuint ElementCount, float Preemption);
