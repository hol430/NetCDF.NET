namespace NetCDF.LowLevel;

/// <summary>
/// Describes zlib deflate settings for a netCDF variable.
/// </summary>
/// <param name="Shuffle">Whether the shuffle filter is enabled.</param>
/// <param name="Deflate">Whether deflate compression is enabled.</param>
/// <param name="DeflateLevel">The deflate compression level.</param>
public readonly record struct VariableDeflateInfo(bool Shuffle, bool Deflate, int DeflateLevel);
