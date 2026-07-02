namespace NetCDF.LowLevel;

/// <summary>
/// Represents a netCDF variable ID.
/// </summary>
/// <param name="Value">The ID of the variable.</param>
public readonly record struct VariableId(int Value)
{
    /// <summary>
    /// Represents the global variable ID, which is used to refer to global
    /// attributes in a netCDF file.
    /// </summary>
    public static VariableId Global { get; } = new(-1);
}
