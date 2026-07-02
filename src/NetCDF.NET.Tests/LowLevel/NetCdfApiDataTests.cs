using System.Text;
using NetCDF.Interop;
using NetCDF.LowLevel;
using NetCDF.Tests.Helpers;
using Xunit.Sdk;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.Tests.LowLevel;

public sealed class NetCdfApiDataTests
{
    [Fact]
    public void VariableDataWrappers_RoundTripFullScalarSectionAndStridedData()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", 6);
        VariableId ints = api.DefineVariable(handle, "ints", NCType.NC_INT, [x]);
        api.EndDefineMode(handle);

        api.WriteVariable(handle, ints, new[] { 0, 0, 0, 0, 0, 0 });
        api.WriteVariableValue(handle, ints, VariableIndex.At(1), 42);
        Assert.Equal(42, api.ReadVariableValue<int>(handle, ints, VariableIndex.At(1)));

        api.WriteVariable(handle, ints, Hyperslab.Contiguous([2], [3]), new[] { 9, 8, 7 });
        int[] section = new int[3];
        api.ReadVariable(handle, ints, Hyperslab.Contiguous([2], [3]), section);
        Assert.Equal([9, 8, 7], section);

        api.WriteVariable(handle, ints, Hyperslab.Strided([0], [3], [2]), new[] { 1, 2, 3 });
        int[] strided = new int[3];
        api.ReadVariable(handle, ints, Hyperslab.Strided([0], [3], [2]), strided);
        Assert.Equal([1, 2, 3], strided);

        int[] all = new int[6];
        api.ReadVariable(handle, ints, all);
        Assert.Equal([1, 42, 2, 8, 3, 0], all);
    }

    [Theory]
    [MemberData(nameof(PrimitiveVariableCases))]
    public void VariableDataWrappers_RoundTripPrimitiveArrays<T>(NCType type, T[] expected)
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", (nuint)expected.Length);
        VariableId variable = api.DefineVariable(handle, "values", type, [x]);
        api.EndDefineMode(handle);

        api.WriteVariable(handle, variable, expected);
        T[] actual = new T[expected.Length];
        api.ReadVariable(handle, variable, actual);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TextVariableWrappers_RoundTripBytes()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = api.Create(file.FilePath);

        byte[] expected = Encoding.ASCII.GetBytes("netcdf");
        DimensionId chars = api.DefineDimension(handle, "chars", (nuint)expected.Length);
        VariableId text = api.DefineVariable(handle, "text", NCType.NC_CHAR, [chars]);
        api.EndDefineMode(handle);

        api.WriteTextVariable(handle, text, expected);
        api.WriteTextVariableValue(handle, text, VariableIndex.At(0), (byte)'N');
        Assert.Equal((byte)'N', api.ReadTextVariableValue(handle, text, VariableIndex.At(0)));
        api.WriteTextVariable(handle, text, Hyperslab.Contiguous([1], [2]), Encoding.ASCII.GetBytes("ET"));
        byte[] section = new byte[2];
        api.ReadTextVariable(handle, text, Hyperslab.Contiguous([1], [2]), section);
        Assert.Equal("ET", Encoding.ASCII.GetString(section));
        api.WriteTextVariable(handle, text, Hyperslab.Strided([3], [2], [2]), Encoding.ASCII.GetBytes("DF"));
        byte[] strided = new byte[2];
        api.ReadTextVariable(handle, text, Hyperslab.Strided([3], [2], [2]), strided);
        Assert.Equal("DF", Encoding.ASCII.GetString(strided));
        byte[] actual = new byte[expected.Length];
        api.ReadTextVariable(handle, text, actual);

        Assert.Equal("NETDdF", Encoding.ASCII.GetString(actual));
    }

    [Fact]
    public void StringVariableWrappers_RoundTripFullScalarSectionAndStridedData()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", 4);
        VariableId labels = api.DefineVariable(handle, "labels", NCType.NC_STRING, [x]);
        api.EndDefineMode(handle);

        api.WriteVariable(handle, labels, new[] { "a", "b", "c", "d" });
        api.WriteVariableValue(handle, labels, VariableIndex.At(1), "bee");
        Assert.Equal("bee", api.ReadVariableValue<string>(handle, labels, VariableIndex.At(1)));

        api.WriteVariable(handle, labels, Hyperslab.Contiguous([2], [2]), new[] { "see", "dee" });
        string[] section = new string[2];
        api.ReadVariable(handle, labels, Hyperslab.Contiguous([2], [2]), section);
        Assert.Equal(["see", "dee"], section);

        api.WriteVariable(handle, labels, Hyperslab.Strided([0], [2], [2]), new[] { "aye", "sea" });
        string[] strided = new string[2];
        api.ReadVariable(handle, labels, Hyperslab.Strided([0], [2], [2]), strided);
        Assert.Equal(["aye", "sea"], strided);

        string[] all = new string[4];
        api.ReadVariable(handle, labels, all);
        Assert.Equal(["aye", "bee", "sea", "dee"], all);
    }

    [Fact]
    public void AttributeDataWrappers_RoundTripNumericTextAndStrings()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = CreateNetcdf4OrSkip(api, file.FilePath);

        api.WriteAttribute(handle, VariableId.Global, "answer", NCType.NC_INT, new[] { 42, 43 });
        int[] ints = new int[2];
        api.ReadAttribute(handle, VariableId.Global, "answer", ints);
        Assert.Equal([42, 43], ints);

        api.WriteAttribute(handle, VariableId.Global, "weights", NCType.NC_DOUBLE, new[] { 1.25, 2.5 });
        double[] doubles = new double[2];
        api.ReadAttribute(handle, VariableId.Global, "weights", doubles);
        Assert.Equal([1.25, 2.5], doubles);

        api.WriteTextAttribute(handle, VariableId.Global, "title", "ocean");
        Assert.Equal("ocean", api.ReadTextAttribute(handle, VariableId.Global, "title"));

        api.WriteStringAttribute(handle, VariableId.Global, "labels", ["north", "south"]);
        Assert.Equal(["north", "south"], api.ReadStringAttribute(handle, VariableId.Global, "labels"));
    }

    [Fact]
    public void NativeLongWrappers_RoundTripVariableAndAttribute()
    {
        NetCdfApi api = new();
        using TempFile file = new();
        using NetCdfHandle handle = api.Create(file.FilePath);

        DimensionId x = api.DefineDimension(handle, "x", 2);
        VariableId variable = api.DefineVariable(handle, "longs", NCType.NC_INT, [x]);
        api.WriteNativeLongAttribute(handle, VariableId.Global, "native_longs", NCType.NC_INT, [5, 6]);
        api.EndDefineMode(handle);

        api.WriteNativeLongVariable(handle, variable, [7, 8]);
        api.WriteNativeLongVariableValue(handle, variable, VariableIndex.At(1), 9);
        Assert.Equal(9, api.ReadNativeLongVariableValue(handle, variable, VariableIndex.At(1)));
        api.WriteNativeLongVariable(handle, variable, Hyperslab.Contiguous([0], [2]), [10, 11]);
        long[] section = new long[2];
        api.ReadNativeLongVariable(handle, variable, Hyperslab.Contiguous([0], [2]), section);
        Assert.Equal([10, 11], section);
        api.WriteNativeLongVariable(handle, variable, Hyperslab.Strided([0], [1], [2]), [12]);
        long[] strided = new long[1];
        api.ReadNativeLongVariable(handle, variable, Hyperslab.Strided([0], [1], [2]), strided);
        Assert.Equal([12], strided);
        long[] variableValues = new long[2];
        api.ReadNativeLongVariable(handle, variable, variableValues);
        Assert.Equal([12, 11], variableValues);

        long[] attributeValues = new long[2];
        api.ReadNativeLongAttribute(handle, VariableId.Global, "native_longs", attributeValues);
        Assert.Equal([5, 6], attributeValues);
    }

    [Fact]
    public void SelectionTypes_ValidateShapeAndRanges()
    {
        Assert.Throws<ArgumentException>(() => VariableIndex.At());
        Assert.Throws<ArgumentOutOfRangeException>(() => VariableIndex.At(-1));
        Assert.Throws<ArgumentException>(() => Hyperslab.Contiguous([0, 0], [1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Hyperslab.Contiguous([0], [0]));
        Assert.Throws<ArgumentException>(() => Hyperslab.Strided([0], [1], [1, 1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Hyperslab.Strided([0], [1], [-1]));
    }

    public static IEnumerable<object[]> PrimitiveVariableCases()
    {
        yield return [NCType.NC_BYTE, new sbyte[] { -1, 0, 2 }];
        yield return [NCType.NC_UBYTE, new byte[] { 1, 2, 120 }];
        yield return [NCType.NC_SHORT, new short[] { -2, 0, 7 }];
        yield return [NCType.NC_INT, new[] { -3, 0, 9 }];
        yield return [NCType.NC_FLOAT, new[] { 1.25f, -2.5f }];
        yield return [NCType.NC_DOUBLE, new[] { 1.25, -2.5 }];
        yield return [NCType.NC_USHORT, new ushort[] { 2, 7 }];
        yield return [NCType.NC_UINT, new uint[] { 2, 7 }];
        yield return [NCType.NC_INT64, new long[] { -8, 9 }];
        yield return [NCType.NC_UINT64, new ulong[] { 8, 9 }];
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
}
