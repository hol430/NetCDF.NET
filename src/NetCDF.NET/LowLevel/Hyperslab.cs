namespace NetCDF.LowLevel;

/// <summary>
/// Represents a netCDF hyperslab selection.
/// </summary>
public readonly record struct Hyperslab
{
    private readonly long[] start;
    private readonly long[] count;
    private readonly long[]? stride;

    /// <summary>
    /// Initializes a new instance of the <see cref="Hyperslab"/> struct.
    /// </summary>
    /// <param name="start">The zero-based starting coordinates.</param>
    /// <param name="count">The element counts in each dimension.</param>
    /// <param name="stride">The optional strides in each dimension.</param>
    public Hyperslab(IReadOnlyList<long> start, IReadOnlyList<long> count, IReadOnlyList<long>? stride = null)
    {
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(count);

        if (start.Count == 0)
            throw new ArgumentException("At least one dimension is required.", nameof(start));

        if (start.Count != count.Count)
            throw new ArgumentException("Start and count must have the same length.", nameof(count));

        if (stride is not null && stride.Count != start.Count)
            throw new ArgumentException("Stride must have the same length as start.", nameof(stride));

        this.start = ValidateNonNegative(start, nameof(start));
        this.count = ValidatePositive(count, nameof(count));
        this.stride = stride is null ? null : ValidatePositive(stride, nameof(stride));
    }

    /// <summary>
    /// Gets the zero-based starting coordinates.
    /// </summary>
    public IReadOnlyList<long> Start => start;

    /// <summary>
    /// Gets the element counts in each dimension.
    /// </summary>
    public IReadOnlyList<long> Count => count;

    /// <summary>
    /// Gets the optional strides in each dimension.
    /// </summary>
    public IReadOnlyList<long>? Stride => stride;

    /// <summary>
    /// Gets a value indicating whether the selection has explicit strides.
    /// </summary>
    public bool IsStrided => stride is not null;

    /// <summary>
    /// Creates a contiguous hyperslab selection.
    /// </summary>
    /// <param name="start">The zero-based starting coordinates.</param>
    /// <param name="count">The element counts in each dimension.</param>
    /// <returns>The hyperslab selection.</returns>
    public static Hyperslab Contiguous(IReadOnlyList<long> start, IReadOnlyList<long> count)
        => new(start, count);

    /// <summary>
    /// Creates a strided hyperslab selection.
    /// </summary>
    /// <param name="start">The zero-based starting coordinates.</param>
    /// <param name="count">The element counts in each dimension.</param>
    /// <param name="stride">The strides in each dimension.</param>
    /// <returns>The hyperslab selection.</returns>
    public static Hyperslab Strided(IReadOnlyList<long> start, IReadOnlyList<long> count, IReadOnlyList<long> stride)
        => new(start, count, stride);

    internal nuint[] ToNativeStart()
        => start.Select(ToNativeSize).ToArray();

    internal nuint[] ToNativeCount()
        => count.Select(ToNativeSize).ToArray();

    internal nint[] ToNativeStride()
    {
        if (stride is null)
            throw new InvalidOperationException("The hyperslab is not strided.");

        return stride.Select(ToNativeSigned).ToArray();
    }

    private static long[] ValidateNonNegative(IReadOnlyList<long> values, string parameterName)
    {
        long[] copy = values.ToArray();
        for (int i = 0; i < copy.Length; i++)
            if (copy[i] < 0)
                throw new ArgumentOutOfRangeException(parameterName, "Values must be non-negative.");

        return copy;
    }

    private static long[] ValidatePositive(IReadOnlyList<long> values, string parameterName)
    {
        long[] copy = values.ToArray();
        for (int i = 0; i < copy.Length; i++)
            if (copy[i] <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "Values must be positive.");

        return copy;
    }

    private static nuint ToNativeSize(long value)
    {
        if ((ulong)value > nuint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), "Value does not fit in native size_t.");

        return (nuint)value;
    }

    private static nint ToNativeSigned(long value)
    {
        if (value > nint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(value), "Value does not fit in native ptrdiff_t.");

        return (nint)value;
    }
}
