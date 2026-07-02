using NetCDF.Interop;
using NetCDF.Tests.Interop;

namespace NetCDF.Tests.Helpers;

public sealed class NcFileHandle : IDisposable
{
    public int Id { get; }

    public NcFileHandle(int id) => Id = id;

    public static NcFileHandle Create(string path, CreateMode mode, string? featureName = null)
    {
        int res = Native.nc_create(path, mode, out int id);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
            res,
            nameof(Native.nc_create),
            featureName,
            InteropTestCommon.NcEinval);
        return new NcFileHandle(id);
    }

    public static NcFileHandle Create(string path, NetcdfTestFormat format)
        => Create(path, format.CreateMode, format.FeatureName);

    public static NcFileHandle Open(string path, OpenMode mode)
    {
        int res = Native.nc_open(path, mode, out int id);
        InteropTestCommon.AssertSuccess(res, nameof(Native.nc_open));
        return new NcFileHandle(id);
    }

    public void Dispose()
    {
        int res = Native.nc_close(Id);
        InteropTestCommon.AssertSuccess(res, nameof(Native.nc_close));
    }
}
