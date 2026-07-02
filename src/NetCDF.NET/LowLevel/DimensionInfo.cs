namespace NetCDF.LowLevel;

/// <summary>
/// Describes a netCDF dimension.
/// </summary>
/// <param name="Name">The dimension name.</param>
/// <param name="Length">The dimension length.</param>
public readonly record struct DimensionInfo(string Name, nuint Length);
