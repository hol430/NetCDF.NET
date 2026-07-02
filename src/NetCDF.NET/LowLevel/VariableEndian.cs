namespace NetCDF.LowLevel;

/// <summary>
/// Represents the endian storage setting for a netCDF variable.
/// </summary>
public enum VariableEndian
{
    /// <summary>Use native endian storage.</summary>
    Native = 0,

    /// <summary>Use little-endian storage.</summary>
    Little = 1,

    /// <summary>Use big-endian storage.</summary>
    Big = 2
}
