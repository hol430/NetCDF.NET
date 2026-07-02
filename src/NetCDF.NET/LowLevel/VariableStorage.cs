namespace NetCDF.LowLevel;

/// <summary>
/// Represents the storage type of a netCDF variable.
/// </summary>
public enum VariableStorage
{
    /// <summary>
    /// The variable is stored in chunks in the file.
    /// </summary>
    Chunked = Constants.NcChunked,

    /// <summary>
    /// The variable is stored contiguously in the file.
    /// </summary>
    Contiguous = Constants.NcContiguous
}
