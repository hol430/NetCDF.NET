namespace NetCDF.LowLevel;

/// <summary>
/// Exception thrown when a native netCDF call returns a non-success status code.
/// </summary>
public sealed class NetCdfException : Exception
{
    /// <summary>
    /// The status code returned by the native netCDF call.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The name of the native netCDF function that returned the error.
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    /// The error message returned by the native netCDF call.
    /// </summary>
    public string NativeMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetCdfException"/> class
    /// with the specified status code, function name, and native error message.
    /// </summary>
    /// <param name="statusCode">The status code returned by the native netCDF call.</param>
    /// <param name="functionName">The name of the native netCDF function that returned the error.</param>
    /// <param name="nativeMessage">The error message returned by the native netCDF call.</param>
    public NetCdfException(int statusCode, string functionName, string nativeMessage)
        : base($"{functionName} failed with status {statusCode}: {nativeMessage}")
    {
        StatusCode = statusCode;
        FunctionName = functionName;
        NativeMessage = nativeMessage;
    }
}
