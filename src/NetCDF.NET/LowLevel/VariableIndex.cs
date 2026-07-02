namespace NetCDF.LowLevel;

/// <summary>
/// Represents a zero-based index into a netCDF variable.
/// </summary>
public readonly record struct VariableIndex
{
    private readonly long[] coordinates;

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableIndex"/> struct.
    /// </summary>
    /// <param name="coordinates">The zero-based coordinates.</param>
    public VariableIndex(params long[] coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);
        if (coordinates.Length == 0)
        {
            throw new ArgumentException("At least one coordinate is required.", nameof(coordinates));
        }

        this.coordinates = ValidateNonNegative(coordinates, nameof(coordinates));
    }

    /// <summary>
    /// Gets the zero-based coordinates.
    /// </summary>
    public IReadOnlyList<long> Coordinates => coordinates;

    /// <summary>
    /// Creates a variable index.
    /// </summary>
    /// <param name="coordinates">The zero-based coordinates.</param>
    /// <returns>The variable index.</returns>
    public static VariableIndex At(params long[] coordinates)
        => new(coordinates);

    internal nuint[] ToNative()
        => coordinates.Select(ToNativeSize).ToArray();

    private static long[] ValidateNonNegative(long[] values, string parameterName)
    {
        long[] copy = values.ToArray();
        for (int i = 0; i < copy.Length; i++)
        {
            if (copy[i] < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Coordinates must be non-negative.");
            }
        }

        return copy;
    }

    private static nuint ToNativeSize(long value)
    {
        if ((ulong)value > nuint.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Coordinate does not fit in native size_t.");
        }

        return (nuint)value;
    }
}
