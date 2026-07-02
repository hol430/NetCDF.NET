using NetCDF.Interop;

namespace NetCDF.LowLevel;

/// <summary>
/// Describes a netCDF variable.
/// </summary>
/// <param name="Name">The variable name.</param>
/// <param name="Type">The netCDF external data type.</param>
/// <param name="Dimensions">The dimension IDs used by the variable.</param>
/// <param name="AttributeCount">The number of attributes attached to the variable.</param>
public readonly record struct VariableInfo(
    string Name,
    NCType Type,
    IReadOnlyList<DimensionId> Dimensions,
    int AttributeCount);
