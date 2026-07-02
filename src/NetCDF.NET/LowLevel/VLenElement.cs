using NetCDF.Interop;

namespace NetCDF.LowLevel;

/// <summary>
/// Represents a native netCDF VLen element owned by libnetcdf.
/// </summary>
/// <remarks>
/// This type intentionally does not expose the native pointer carried by
/// nc_vlen_t. Use <see cref="NetCdfApi.FreeVLenElement"/> when the element is no
/// longer needed.
/// </remarks>
public readonly record struct VLenElement
{
    internal VLenElement(Native.NcVlen value)
    {
        Value = value;
    }

    internal Native.NcVlen Value { get; }

    /// <summary>
    /// Gets the number of elements in the VLen value.
    /// </summary>
    public nuint Length => Value.len;
}
