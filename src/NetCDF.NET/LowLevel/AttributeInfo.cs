using NetCDF.Interop;

namespace NetCDF.LowLevel;

/// <summary>
/// Describes a netCDF attribute.
/// </summary>
/// <param name="Type">The netCDF external data type of the attribute values.</param>
/// <param name="Length">The number of values in the attribute.</param>
public readonly record struct AttributeInfo(NCType Type, nuint Length);
