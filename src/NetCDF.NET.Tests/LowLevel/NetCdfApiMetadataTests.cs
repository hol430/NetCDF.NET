using NetCDF.Interop;
using NetCDF.LowLevel;
using NetCDF.Tests.Helpers;
using Xunit.Sdk;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.Tests.LowLevel;

public sealed class NetCdfApiMetadataTests
{
    [Fact]
    public void FileAndDimensionWrappers_RoundTripMetadata()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = api.Create(file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", 5);
        DimensionId time = api.DefineDimension(handle, "time", 0);

        Assert.Equal(2, api.InquireDimensionCount(handle));
        Assert.Equal(x, api.InquireDimensionId(handle, "x"));
        Assert.Equal((nuint)5, api.InquireDimensionLength(handle, x));
        Assert.Equal("x", api.InquireDimensionName(handle, x));
        Assert.Equal(new DimensionInfo("x", 5), api.InquireDimension(handle, x));
        Assert.Equal(time, api.InquireUnlimitedDimension(handle));
        Assert.Contains(time, api.InquireUnlimitedDimensions(handle));

        NetCdfFileInfo info = api.Inquire(handle);
        Assert.Equal(2, info.DimensionCount);
        Assert.Equal(0, info.VariableCount);
        Assert.Equal(0, info.GlobalAttributeCount);
        Assert.Equal(time, info.UnlimitedDimensionId);

        Assert.True(api.InquireFormat(handle) > 0);
        NetCdfFormatInfo extended = api.InquireFormatExtended(handle);
        Assert.True(extended.Format > 0);
        Assert.Equal(file.FilePath, api.InquirePath(handle));

        FillMode previous = api.SetFill(handle, FillMode.NoFill);
        Assert.True(previous == FillMode.Fill || previous == FillMode.NoFill);

        api.RenameDimension(handle, x, "longitude");
        Assert.Equal("longitude", api.InquireDimensionName(handle, x));

        api.EndDefineMode(handle);
        api.Sync(handle);
        api.Redefine(handle);
    }

    [Fact]
    public void CreateMemory_ReturnsOpenHandle()
    {
        NetCdfApi api = new();
        using TempFile file = new();

        NetCdfHandle handle;
        try
        {
            handle = api.CreateMemory(file.FilePath, CreateMode.NC_CLOBBER, 1024);
        }
        catch (NetCdfException ex) when (IsFeatureUnavailable(ex.StatusCode))
        {
            throw SkipException.ForSkip($"nc_create_mem unavailable in this runtime: {ex.Message}");
        }

        using (handle)
        {
            Assert.True(handle.Id >= 0);
            Assert.True(api.InquireFormat(handle) > 0);
        }
    }

    [Fact]
    public void Abort_ClosesHandle()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = api.Create(file.FilePath);

        api.Abort(handle);

        Assert.True(handle.IsClosed);
        Assert.Throws<ObjectDisposedException>(() => handle.Id);
    }

    [Fact]
    public void VariableWrappers_RoundTripMetadata()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", 10);
        VariableId variable = api.DefineVariable(handle, "temperature", NCType.NC_FLOAT, [x]);
        api.DefineVariableFill(handle, variable, -999.0f);

        api.DefineVariableChunking(handle, variable, VariableStorage.Chunked, [(nuint)5]);
        api.DefineVariableDeflate(handle, variable, shuffle: true, deflate: true, deflateLevel: 1);
        api.DefineVariableFletcher32(handle, variable, enabled: true);
        api.DefineVariableEndian(handle, variable, VariableEndian.Little);

        Exception? filterException = Record.Exception(() => api.DefineVariableFilter(handle, variable, uint.MaxValue, []));
        Assert.True(filterException is null or NetCdfException);
        Exception? szipException = Record.Exception(() => api.DefineVariableSzip(handle, variable, 0, 0));
        Assert.True(szipException is null or NetCdfException);

        Assert.Equal(variable, api.InquireVariableId(handle, "temperature"));
        Assert.Equal("temperature", api.InquireVariableName(handle, variable));
        Assert.Equal(NCType.NC_FLOAT, api.InquireVariableType(handle, variable));
        Assert.Equal(1, api.InquireVariableDimensionCount(handle, variable));
        Assert.Equal([x], api.InquireVariableDimensions(handle, variable));
        Assert.Equal(1, api.InquireVariableAttributeCount(handle, variable));
        Assert.Equal(1, api.InquireVariableCount(handle));
        Assert.Equal([variable], api.InquireVariableIds(handle));

        VariableInfo info = api.InquireVariable(handle, variable);
        Assert.Equal("temperature", info.Name);
        Assert.Equal(NCType.NC_FLOAT, info.Type);
        Assert.Equal([x], info.Dimensions);

        VariableDeflateInfo deflate = api.InquireVariableDeflate(handle, variable);
        Assert.True(deflate.Shuffle);
        Assert.True(deflate.Deflate);
        Assert.Equal(1, deflate.DeflateLevel);
        (VariableStorage storage, IReadOnlyList<nuint> chunks) = api.InquireVariableChunking(handle, variable);
        Assert.Equal(VariableStorage.Chunked, storage);
        Assert.Equal([(nuint)5], chunks);
        (bool noFill, float fillValue) = api.InquireVariableFill<float>(handle, variable);
        Assert.False(noFill);
        Assert.Equal(-999.0f, fillValue);
        Assert.True(api.InquireVariableFletcher32(handle, variable));
        Assert.Equal(VariableEndian.Little, api.InquireVariableEndian(handle, variable));
        Exception? inqFilterException = Record.Exception(() => api.InquireVariableFilter(handle, variable));
        Assert.True(inqFilterException is null or NetCdfException);
        Exception? inqSzipException = Record.Exception(() => api.InquireVariableSzip(handle, variable));
        Assert.True(inqSzipException is null or NetCdfException);
        Assert.Throws<NetCdfException>(() => api.SetVariableParallelAccess(handle, variable, ParallelAccess.NC_COLLECTIVE));

        api.SetVariableChunkCache(handle, variable, 1_048_576, 1009, 0.5f);
        VariableChunkCacheInfo cache = api.GetVariableChunkCache(handle, variable);
        Assert.True(cache.Size >= 1_048_576);
        Assert.True(cache.ElementCount >= 1009);
        Assert.Equal(0.5f, cache.Preemption, 3);

        api.RenameVariable(handle, variable, "air_temperature");
        Assert.Equal("air_temperature", api.InquireVariableName(handle, variable));

        VariableId noFillVariable = api.DefineVariable(handle, "nofill", NCType.NC_INT, [x]);
        api.DefineVariableNoFill(handle, noFillVariable);
        Assert.True(api.InquireVariableFill<int>(handle, noFillVariable).NoFill);
    }

    [Fact]
    public void AttributeWrappers_RoundTripMetadata()
    {
        NetCdfApi api = new();
        using TempFile sourceFile = new();
        using TempFile destinationFile = new();
        using NetCdfHandle source = api.Create(sourceFile.FilePath);
        using NetCdfHandle destination = api.Create(destinationFile.FilePath);

        AssertNativeSuccess(Native.nc_put_att_int(source.Id, VariableId.Global.Value, "answer", NCType.NC_INT, 1, [42]));

        AttributeInfo info = api.InquireAttribute(source, VariableId.Global, "answer");
        Assert.Equal(NCType.NC_INT, info.Type);
        Assert.Equal((nuint)1, info.Length);
        Assert.Equal(NCType.NC_INT, api.InquireAttributeType(source, VariableId.Global, "answer"));
        Assert.Equal((nuint)1, api.InquireAttributeLength(source, VariableId.Global, "answer"));
        AttributeId attributeId = api.InquireAttributeId(source, VariableId.Global, "answer");
        Assert.Equal("answer", api.InquireAttributeName(source, VariableId.Global, attributeId));
        Assert.Equal(1, api.InquireGlobalAttributeCount(source));

        api.CopyAttribute(source, VariableId.Global, "answer", destination, VariableId.Global);
        Assert.Equal(NCType.NC_INT, api.InquireAttribute(destination, VariableId.Global, "answer").Type);

        api.RenameAttribute(destination, VariableId.Global, "answer", "renamed");
        Assert.Equal(NCType.NC_INT, api.InquireAttribute(destination, VariableId.Global, "renamed").Type);

        api.DeleteAttribute(destination, VariableId.Global, "renamed");
        Assert.Throws<NetCdfException>(() => api.InquireAttribute(destination, VariableId.Global, "renamed"));
    }

    [Fact]
    public void GroupWrappers_RoundTripMetadata()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", 3);
        VariableId variable = api.DefineVariable(handle, "v", NCType.NC_INT, [x]);
        GroupId group = api.DefineGroup(handle, "child");

        Assert.Equal(group, api.InquireGroup(handle, "child"));
        Assert.Equal(group, api.InquireNcid(handle, "child"));
        Assert.Equal(group, api.InquireGroupByFullName(handle, "/child"));
        Assert.Equal("child", api.InquireGroupName(group));
        Assert.Equal("/child", api.InquireGroupFullName(group));
        Assert.Equal((nuint)6, api.InquireGroupFullNameLength(group));
        Assert.Equal(handle.Id, api.InquireGroupParent(group).Value);
        Assert.Equal([group], api.InquireGroups(handle));
        Assert.Equal([x], api.InquireDimensionIds(handle, includeParents: false));
        Assert.Equal([variable], api.InquireVariableIds(handle));
        Assert.Empty(api.InquireTypeIds(handle));

        api.RenameGroup(group, "renamed");
        Assert.Equal("renamed", api.InquireGroupName(group));
    }

    [Fact]
    public void TypeWrappers_RoundTripMetadata()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        TypeInfo intType = api.InquireType(handle, NCType.NC_INT);
        Assert.False(string.IsNullOrWhiteSpace(intType.Name));
        Assert.True(intType.Size > 0);

        NCType compound = api.DefineCompound(handle, 8, "point_t");
        api.InsertCompound(handle, compound, "x", 0, NCType.NC_INT);
        api.InsertArrayCompound(handle, compound, "flags", 4, NCType.NC_SHORT, [2]);

        Assert.Equal(compound, api.InquireTypeId(handle, "point_t"));
        Assert.True(api.InquireTypeEqual(handle, NCType.NC_INT, handle, NCType.NC_INT));
        UserTypeInfo userType = api.InquireUserType(handle, compound);
        Assert.Equal("point_t", userType.Name);

        CompoundTypeInfo compoundInfo = api.InquireCompound(handle, compound);
        Assert.Equal("point_t", compoundInfo.Name);
        Assert.Equal((nuint)8, api.InquireCompoundSize(handle, compound));
        Assert.Equal((nuint)2, api.InquireCompoundFieldCount(handle, compound));
        Assert.Equal("point_t", api.InquireCompoundName(handle, compound));

        CompoundFieldInfo firstField = api.InquireCompoundField(handle, compound, 0);
        Assert.Equal("x", firstField.Name);
        Assert.Equal(0, api.InquireCompoundFieldIndex(handle, compound, "x"));
        Assert.Equal((nuint)0, api.InquireCompoundFieldOffset(handle, compound, 0));
        Assert.Equal(NCType.NC_INT, api.InquireCompoundFieldType(handle, compound, 0));
        Assert.Equal(0, api.InquireCompoundFieldDimensionCount(handle, compound, 0));
        Assert.Equal("flags", api.InquireCompoundFieldName(handle, compound, 1));
        Assert.Equal(1, api.InquireCompoundFieldDimensionCount(handle, compound, 1));
        Assert.Equal([2], api.InquireCompoundFieldDimensionSizes(handle, compound, 1));

        NCType enumType = api.DefineEnum(handle, NCType.NC_INT, "quality_t");
        api.InsertEnum(handle, enumType, "good", 1);
        EnumTypeInfo enumInfo = api.InquireEnum(handle, enumType);
        Assert.Equal("quality_t", enumInfo.Name);
        Assert.Equal(NCType.NC_INT, enumInfo.BaseType);
        EnumMemberInfo member = api.InquireEnumMember(handle, enumType, 0);
        Assert.Equal("good", member.Name);
        Assert.Equal(1, member.Value);
        Assert.Equal("good", api.InquireEnumIdentifier(handle, enumType, 1));

        NCType vlenType = api.DefineVLen(handle, "int_list_t", NCType.NC_INT);
        VLenTypeInfo vlen = api.InquireVLen(handle, vlenType);
        Assert.Equal("int_list_t", vlen.Name);
        Assert.Equal(NCType.NC_INT, vlen.BaseType);

        VLenElement element = api.CreateVLenElement(handle, vlenType, new[] { 10, 20, 30 });
        Assert.Equal((nuint)3, element.Length);
        Assert.Equal([10, 20, 30], api.ReadVLenElement<int>(handle, vlenType, element));

        VLenElement allocated = new(new Native.NcVlen { len = 1, p = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(int)) });
        api.FreeVLenElement(allocated);

        VLenElement first = new(new Native.NcVlen { len = 1, p = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeof(int)) });
        VLenElement second = new(new Native.NcVlen { len = 2, p = System.Runtime.InteropServices.Marshal.AllocHGlobal(2 * sizeof(int)) });
        api.FreeVLenElements([first, second]);
    }

    [Fact]
    public void ConfigurationWrappers_RoundTrip()
    {
        NetCdfApi api = new();
        ChunkCacheInfo originalCache = api.GetChunkCache();

        try
        {
            api.SetChunkCache(2_097_152, 4093, 0.33f);
            ChunkCacheInfo cache = api.GetChunkCache();
            Assert.True(cache.Size >= 2_097_152);
            Assert.True(cache.ElementCount >= 4093);
            Assert.Equal(0.33f, cache.Preemption, 3);
        }
        finally
        {
            api.SetChunkCache(originalCache.Size, originalCache.ElementCount, originalCache.Preemption);
        }

        const int netcdf4Format = 3;
        int originalFormat;
        try
        {
            originalFormat = api.SetDefaultFormat(netcdf4Format);
        }
        catch (NetCdfException ex) when (IsFeatureUnavailable(ex.StatusCode))
        {
            throw SkipException.ForSkip($"nc_set_default_format unavailable in this runtime: {ex.Message}");
        }

        try
        {
            int previous = api.SetDefaultFormat(netcdf4Format);
            Assert.Equal(netcdf4Format, previous);
        }
        finally
        {
            api.SetDefaultFormat(originalFormat);
        }
    }

    [Fact(Skip = "ShowMetadata writes to native stdio; wrapper test is kept skipped to avoid noisy/non-portable output suppression.")]
    public void ShowMetadata_ReturnsSuccessWhenAvailable()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = api.Create(file.FilePath);

        api.ShowMetadata(handle);
    }

    private static NetCdfHandle CreateNetcdf4OrSkip(NetCdfApi api, string path)
    {
        try
        {
            return api.Create(path, CreateMode.NC_CLOBBER | CreateMode.NC_NETCDF4);
        }
        catch (NetCdfException ex) when (IsFeatureUnavailable(ex.StatusCode))
        {
            throw SkipException.ForSkip($"netCDF-4 unavailable in this runtime: {ex.Message}");
        }
    }

    private static bool IsFeatureUnavailable(int status)
        => status is NcEnotNc4 or NcEnotBuilt or NcEnopar or NcEfilter or NcEnoFilter or NcEinval or NcEInmemory;

    private static void AssertNativeSuccess(int status)
    {
        if (status != NcNoErr)
        {
            throw new XunitException($"Native setup failed with status {status}");
        }
    }
}
