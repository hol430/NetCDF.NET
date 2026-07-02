using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class VariableTests
{
    private const int NcNoFill = 1;
    private const int NcFletcher32On = 1;
    private const int NcEndianLittle = 1;

    [Fact]
    public void DefineVariable_InquiryMatches()
    {
        using NcTempFile hnd = new();

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)5, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        InteropTestCommon.AssertSuccess(Native.nc_inq_varid(hnd.Id, "v", out int lookedUpVarId), "nc_inq_varid");
        Assert.Equal(varId, lookedUpVarId);

        InteropTestCommon.AssertSuccess(Native.nc_inq_vartype(hnd.Id, varId, out NCType varType), "nc_inq_vartype");
        Assert.Equal(NCType.NC_INT, varType);

        InteropTestCommon.AssertSuccess(Native.nc_inq_varndims(hnd.Id, varId, out int ndims), "nc_inq_varndims");
        Assert.Equal(1, ndims);

        int[] dimIds = new int[ndims];
        InteropTestCommon.AssertSuccess(Native.nc_inq_vardimid(hnd.Id, varId, dimIds), "nc_inq_vardimid");
        Assert.Equal(dimId, dimIds[0]);
    }

    [Fact]
    public unsafe void DefVarChunking_AndInqVarChunking_RoundTrip()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)16, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        nuint[] expectedChunks = [(nuint)4];
        int chunkStatus = Native.nc_def_var_chunking(hnd.Id, varId, InteropTestCommon.NcChunked, expectedChunks);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(chunkStatus, "nc_def_var_chunking");

        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        nuint[] actualChunks = new nuint[1];
        fixed (nuint* chunkPtr = actualChunks)
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_var_chunking(hnd.Id, varId, out int storage, chunkPtr), "nc_inq_var_chunking");
            Assert.Equal(InteropTestCommon.NcChunked, storage);
        }

        Assert.Equal(expectedChunks[0], actualChunks[0]);
    }

    [Fact]
    public void SetAndGetVarChunkCache_RoundTrip()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)64, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        nuint requestedSize = (nuint)1_048_576;
        nuint requestedElements = (nuint)2_047;
        float requestedPreemption = 0.25f;

        int setStatus = Native.nc_set_var_chunk_cache(hnd.Id, varId, requestedSize, requestedElements, requestedPreemption);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(setStatus, "nc_set_var_chunk_cache");

        InteropTestCommon.AssertSuccess(Native.nc_get_var_chunk_cache(hnd.Id, varId, out nuint actualSize, out nuint actualElements, out float actualPreemption), "nc_get_var_chunk_cache");
        Assert.True(actualSize >= requestedSize);
        Assert.True(actualElements >= requestedElements);
        Assert.Equal(requestedPreemption, actualPreemption, 3);
    }

    [Fact]
    public void DeflateAndFilterInquiry_RoundTrip()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)32, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        int deflateStatus = Native.nc_def_var_deflate(hnd.Id, varId, 1, 1, 2);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
            deflateStatus,
            "nc_def_var_deflate",
            InteropTestCommon.FeatureFilters,
            InteropTestCommon.NcEfilter);

        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        InteropTestCommon.AssertSuccess(Native.nc_inq_var_deflate(hnd.Id, varId, out int shuffle, out int deflate, out int deflateLevel), "nc_inq_var_deflate");
        Assert.Equal(1, shuffle);
        Assert.Equal(1, deflate);
        Assert.Equal(2, deflateLevel);

        int genericFilterStatus = Native.nc_inq_var_filter(hnd.Id, varId, out uint filterId, out nuint paramCount, null);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
            genericFilterStatus,
            "nc_inq_var_filter(count)",
            InteropTestCommon.FeatureFilters,
            InteropTestCommon.NcEnoFilter,
            InteropTestCommon.NcEfilter);

        Assert.True(filterId > 0);

        uint[] parameters = new uint[paramCount];
        InteropTestCommon.AssertSuccess(Native.nc_inq_var_filter(hnd.Id, varId, out uint filterIdAgain, out nuint paramCountAgain, parameters), "nc_inq_var_filter(values)");
        Assert.Equal(filterId, filterIdAgain);
        Assert.Equal(paramCount, paramCountAgain);
        if (filterId == 1 && parameters.Length > 0)
        {
            Assert.Contains(2u, parameters);
        }
    }

    [Fact]
    public unsafe void DefVarFill_AndInqVarFill_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4, InteropTestCommon.FeatureNetcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)8, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        InteropTestCommon.AssertSuccess(Native.nc_def_var_fill(hnd.Id, varId, NcNoFill, null), "nc_def_var_fill");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        int actualFillValue = 0;
        InteropTestCommon.AssertSuccess(Native.nc_inq_var_fill(hnd.Id, varId, out int noFill, &actualFillValue), "nc_inq_var_fill");
        Assert.Equal(NcNoFill, noFill);
    }

    [Fact]
    public void DefVarFletcher32_AndInqVarFletcher32_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4, InteropTestCommon.FeatureNetcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)8, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        int setStatus = Native.nc_def_var_fletcher32(hnd.Id, varId, NcFletcher32On);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
            setStatus,
            "nc_def_var_fletcher32",
            InteropTestCommon.FeatureFilters,
            InteropTestCommon.NcEfilter);
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        InteropTestCommon.AssertSuccess(Native.nc_inq_var_fletcher32(hnd.Id, varId, out int fletcher32), "nc_inq_var_fletcher32");
        Assert.Equal(NcFletcher32On, fletcher32);
    }

    [Fact]
    public void DefVarEndian_AndInqVarEndian_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4, InteropTestCommon.FeatureNetcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)8, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        int setStatus = Native.nc_def_var_endian(hnd.Id, varId, NcEndianLittle);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(setStatus, "nc_def_var_endian");
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        InteropTestCommon.AssertSuccess(Native.nc_inq_var_endian(hnd.Id, varId, out int endian), "nc_inq_var_endian");
        Assert.Equal(NcEndianLittle, endian);
    }

    [Fact]
    public void SetVarSzip_AndInqVarSzip_RoundTrip()
    {
        using var temp = new TempFile();
        using NcFileHandle hnd = NcFileHandle.Create(temp.FilePath, CreateMode.NC_NETCDF4, InteropTestCommon.FeatureNetcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)32, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        const int optionsMask = 32;
        const int pixelsPerBlock = 8;
        int setStatus = Native.nc_def_var_szip(hnd.Id, varId, optionsMask, pixelsPerBlock);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
            setStatus,
            "nc_set_var_szip",
            InteropTestCommon.FeatureFilters,
            InteropTestCommon.NcEfilter);
        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        int inqStatus = Native.nc_inq_var_szip(hnd.Id, varId, out int actualOptionsMask, out int actualPixelsPerBlock);
        InteropTestCommon.AssertSuccess(inqStatus, "nc_inq_var_szip");
        Assert.Equal(optionsMask, actualOptionsMask);
        Assert.Equal(pixelsPerBlock, actualPixelsPerBlock);
    }

    [Fact]
    public void NcDefVarFilter_InvalidFilterId_ReturnsErrorOrFeatureUnavailable()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        InteropTestCommon.AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)3, out int dimId), "nc_def_dim");
        InteropTestCommon.AssertSuccess(Native.nc_def_var(hnd.Id, "v", NCType.NC_INT, 1, [dimId], out int varId), "nc_def_var");

        int status = Native.nc_def_var_filter(hnd.Id, varId, uint.MaxValue, 1, [42u]);
        if (status == InteropTestCommon.NcEnotBuilt || status == InteropTestCommon.NcEnotNc4 || status == InteropTestCommon.NcEfilter)
        {
            InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
                status,
                "nc_def_var_filter",
                InteropTestCommon.FeatureFilters,
                InteropTestCommon.NcEfilter);
        }

        Assert.NotEqual(InteropTestCommon.NcNoErr, status);
    }
}
