namespace NetCDF.LowLevel;

/// <summary>
/// Represents an MPI communicator handle used by parallel netCDF operations.
/// </summary>
/// <param name="Value">The native MPI communicator handle.</param>
public readonly record struct MpiCommunicator(IntPtr Value);

/// <summary>
/// Represents an MPI info handle used by parallel netCDF operations.
/// </summary>
/// <param name="Value">The native MPI info handle.</param>
public readonly record struct MpiInfo(IntPtr Value);
