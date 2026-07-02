namespace NetCDF.LowLevel;

/// <summary>
/// Represents a netCDF dimension ID.
/// </summary>
/// <param name="Value">The ID of the dimension.</param>
public readonly record struct DimensionId(int Value);
