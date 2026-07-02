namespace NetCDF.LowLevel;

/// <summary>
/// Represents netCDF fill behavior.
/// </summary>
public enum FillMode
{
    /// <summary>Use fill values for unwritten data.</summary>
    Fill = 0,

    /// <summary>Do not prefill unwritten data.</summary>
    NoFill = 0x100
}
