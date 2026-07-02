namespace NetCDF.LowLevel;

/// <summary>
/// Describes counts and unlimited-dimension information for a netCDF file or group.
/// </summary>
/// <param name="DimensionCount">The number of dimensions visible in the file or group.</param>
/// <param name="VariableCount">The number of variables visible in the file or group.</param>
/// <param name="GlobalAttributeCount">The number of global attributes visible in the file or group.</param>
/// <param name="UnlimitedDimensionId">The unlimited dimension ID, or <see langword="null"/> when none exists.</param>
public readonly record struct NetCdfFileInfo(
    int DimensionCount,
    int VariableCount,
    int GlobalAttributeCount,
    DimensionId? UnlimitedDimensionId);
