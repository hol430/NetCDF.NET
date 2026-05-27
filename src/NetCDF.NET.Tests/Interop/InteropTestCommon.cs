using NetCDF.Interop;
using Xunit.Sdk;

namespace NetCDF.Tests.Interop;

internal static class InteropTestCommon
{
    internal const int NcNoErr = 0;
    internal const int NcGlobal = -1;
    internal const int NcChunked = 0;
    internal const int NcContiguous = 1;

    internal const int NcEnotNc4 = -111;
    internal const int NcEnopar = -114;
    internal const int NcEnotBuilt = -128;
    internal const int NcEfilter = -132;
    internal const int NcEnoFilter = -136;

    internal static void AssertSuccess(int status, string operation)
    {
        if (status == NcNoErr)
        {
            return;
        }

        string message = Native.nc_strerror(status);
        throw new XunitException($"{operation} failed with status {status}: {message}");
    }

    internal static void AssertSuccessOrSkipIfFeatureUnavailable(int status, string operation, params int[] additionalUnavailableStatuses)
    {
        if (status == NcNoErr)
        {
            return;
        }

        if (status == NcEnotNc4 || status == NcEnotBuilt || IsIn(status, additionalUnavailableStatuses))
        {
            string message = Native.nc_strerror(status);
            throw SkipException.ForSkip($"{operation} unavailable in this runtime (status {status}: {message}).");
        }

        AssertSuccess(status, operation);
    }

    private static bool IsIn(int status, int[] candidates)
    {
        foreach (int candidate in candidates)
        {
            if (status == candidate)
            {
                return true;
            }
        }

        return false;
    }
}
