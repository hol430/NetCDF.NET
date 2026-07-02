namespace NetCDF.LowLevel;

/// <summary>
/// Describes the format and mode values reported by libnetcdf for an open file.
/// </summary>
/// <param name="Format">The netCDF format value reported by libnetcdf.</param>
/// <param name="Mode">The mode flags reported by libnetcdf.</param>
public readonly record struct NetCdfFormatInfo(int Format, int Mode);
