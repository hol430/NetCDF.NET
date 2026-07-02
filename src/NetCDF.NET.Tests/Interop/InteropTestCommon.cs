using System.Runtime.InteropServices;
using NetCDF.Interop;
using Xunit.Sdk;

namespace NetCDF.Tests.Interop;

internal static class InteropTestCommon
{
    internal const string FeatureFilters = "filters";
    internal const string FeatureCdf5 = "cdf5";
    internal const string FeatureMpi = "mpi";
    internal const string FeatureMpiClassic = "pnetcdf";
    internal const string FeatureNetcdf4 = "netcdf4";
    internal const string FeatureParallel4 = "parallel4";

    internal const int NcNoErr = 0;
    internal const int NcGlobal = -1;
    internal const int NcChunked = 0;
    internal const int NcContiguous = 1;

    internal const int NcEnotNc4 = -111;
    internal const int NcEnopar = -114;
    internal const int NcEnotBuilt = -128;
    internal const int NcEfilter = -132;
    internal const int NcEnoFilter = -136;
    internal const int NcEinval = -36;

    internal static void AssertSuccess(int status, string operation)
    {
        if (status == NcNoErr)
        {
            return;
        }

        nint messagePtr = Native.nc_strerror(status);
        string message = Marshal.PtrToStringUTF8(messagePtr) ?? string.Empty;
        throw new XunitException($"{operation} failed with status {status}: {message}");
    }

    internal static void AssertSuccessOrSkipIfFeatureUnavailable(int status, string operation, params int[] additionalUnavailableStatuses)
        => AssertSuccessOrSkipIfFeatureUnavailable(status, operation, null, additionalUnavailableStatuses);

    internal static void AssertSuccessOrSkipIfFeatureUnavailable(int status, string operation, string? featureName, params int[] additionalUnavailableStatuses)
    {
        if (status == NcNoErr)
        {
            return;
        }

        if (status == NcEnotNc4 || status == NcEnotBuilt || IsIn(status, additionalUnavailableStatuses))
        {
            string message = GetErrorMessage(status);
            SkipOrFailFeatureUnavailable(featureName, operation, $"status {status}: {message}");
        }

        AssertSuccess(status, operation);
    }

    internal static void SkipOrFailFeatureUnavailable(string? featureName, string operation, string reason)
    {
        string message = $"{operation} unavailable in this runtime ({reason}).";
        if (IsFeatureRequired(featureName))
        {
            throw new XunitException(message);
        }

        throw SkipException.ForSkip(message);
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

    private static bool IsFeatureRequired(string? featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
        {
            return false;
        }

        string? requiredFeatures = Environment.GetEnvironmentVariable("NETCDF_TEST_REQUIRE_FEATURES");
        if (string.IsNullOrWhiteSpace(requiredFeatures))
        {
            return false;
        }

        string[] tokens = requiredFeatures.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (string token in tokens)
        {
            if (string.Equals(token, featureName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "all", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string GetErrorMessage(int status)
    {
        nint messagePtr = Native.nc_strerror(status);
        return Marshal.PtrToStringUTF8(messagePtr) ?? string.Empty;
    }
}
