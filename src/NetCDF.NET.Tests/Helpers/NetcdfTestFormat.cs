using NetCDF.Interop;
using NetCDF.Tests.Interop;

namespace NetCDF.Tests.Helpers;

public sealed record NetcdfTestFormat(string Name, CreateMode CreateMode, string? FeatureName = null)
{
    public override string ToString() => Name;
}

public static class NetcdfTestFormats
{
    public static readonly NetcdfTestFormat Classic = new("classic", CreateMode.NC_CLOBBER);
    public static readonly NetcdfTestFormat Offset64 = new("64-bit offset", CreateMode.NC_64BIT_OFFSET);
    public static readonly NetcdfTestFormat Cdf5 = new("CDF-5", CreateMode.NC_64BIT_DATA, InteropTestCommon.FeatureCdf5);
    public static readonly NetcdfTestFormat Netcdf4Classic = new(
        "netCDF-4 classic model",
        CreateMode.NC_NETCDF4 | CreateMode.NC_CLASSIC_MODEL,
        InteropTestCommon.FeatureNetcdf4);
    public static readonly NetcdfTestFormat Netcdf4 = new(
        "netCDF-4 enhanced model",
        CreateMode.NC_NETCDF4,
        InteropTestCommon.FeatureNetcdf4);

    public static TheoryData<NetcdfTestFormat> AllFormats => new()
    {
        Classic,
        Offset64,
        Cdf5,
        Netcdf4Classic,
        Netcdf4
    };

    public static TheoryData<NetcdfTestFormat> ClassicModelFormats => new()
    {
        Classic,
        Offset64,
        Cdf5,
        Netcdf4Classic
    };
}
