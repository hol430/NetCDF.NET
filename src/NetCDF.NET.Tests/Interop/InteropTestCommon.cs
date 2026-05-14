using NC.Net.Interop;
using Xunit.Sdk;

namespace NetCDF.Tests.Interop;

internal static class InteropTestCommon
{
    internal const int NcNoErr = 0;
    internal const int NcGlobal = -1;

    internal static void AssertSuccess(int status, string operation)
    {
        if (status == NcNoErr)
        {
            return;
        }

        string message = Native.nc_strerror(status);
        throw new XunitException($"{operation} failed with status {status}: {message}");
    }

    internal static void CloseIfOpen(ref int ncid)
    {
        if (ncid >= 0)
        {
            _ = Native.nc_close(ncid);
            ncid = -1;
        }
    }
}
