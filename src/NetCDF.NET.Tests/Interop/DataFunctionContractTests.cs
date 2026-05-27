using System.Runtime.InteropServices;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;
using Xunit.Sdk;

using static NetCDF.Tests.Interop.InteropTestCommon;

namespace NetCDF.Tests.Interop;

public sealed class DataFunctionContractTests
{
    [Fact]
    public void PutGetVar_RoundTrip_AllPublicDataBindings()
    {
        foreach (DataBinding binding in DataBindings.All)
        {
            using NcTempFile hnd = new();

            AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)binding.Length, out int dimId), $"nc_def_dim({binding.Name})");
            AssertSuccess(Native.nc_def_var(hnd.Id, binding.VarName, binding.Type, 1, [dimId], out int varId), $"nc_def_var({binding.Name})");
            AssertSuccess(Native.nc_enddef(hnd.Id), $"nc_enddef({binding.Name})");

            binding.PutVar(hnd.Id, varId, binding.SourceValues);

            object actual = binding.NewArray(binding.Length);
            binding.GetVar(hnd.Id, varId, actual);
            binding.AssertEqual(binding.SourceValues, actual);
        }
    }

    [Fact]
    public void PutGetVar1_RoundTrip_AllPublicDataBindings()
    {
        const int length = 6;
        const int index = 3;

        foreach (DataBinding binding in DataBindings.Var1Stable)
        {
            using NcTempFile hnd = new();

            AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)length, out int dimId), $"nc_def_dim({binding.Name})");
            AssertSuccess(Native.nc_def_var(hnd.Id, binding.VarName, binding.Type, 1, [dimId], out int varId), $"nc_def_var({binding.Name})");
            AssertSuccess(Native.nc_enddef(hnd.Id), $"nc_enddef({binding.Name})");

            binding.PutVar(hnd.Id, varId, binding.ZeroArray(length));
            binding.PutVar1(hnd.Id, varId, index, binding.Var1Value);

            object full = binding.NewArray(length);
            binding.GetVar(hnd.Id, varId, full);
            binding.AssertSingleIndexEquals(full, index, binding.Var1Value);

            object var1Value = binding.NewScalarBox();
            binding.GetVar1(hnd.Id, varId, index, var1Value);
            binding.AssertScalarEquals(binding.Var1Value, var1Value);
        }
    }

    [Fact]
    public void PutGetVara_RoundTrip_AllPublicDataBindings()
    {
        const int length = 6;
        const int start = 2;
        const int count = 3;

        foreach (DataBinding binding in DataBindings.All)
        {
            using NcTempFile hnd = new();

            AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)length, out int dimId), $"nc_def_dim({binding.Name})");
            AssertSuccess(Native.nc_def_var(hnd.Id, binding.VarName, binding.Type, 1, [dimId], out int varId), $"nc_def_var({binding.Name})");
            AssertSuccess(Native.nc_enddef(hnd.Id), $"nc_enddef({binding.Name})");

            object baseline = binding.ZeroArray(length);
            binding.PutVar(hnd.Id, varId, baseline);

            object window = binding.NewArray(count);
            binding.CopyWindowSource(window);
            binding.PutVara(hnd.Id, varId, start, count, window);

            object subset = binding.NewArray(count);
            binding.GetVara(hnd.Id, varId, start, count, subset);
            binding.AssertEqual(window, subset);

            object full = binding.NewArray(length);
            binding.GetVar(hnd.Id, varId, full);
            binding.AssertWindowEquals(full, start, window);
        }
    }

    [Fact]
    public void PutGetVars_RoundTrip_AllPublicDataBindings()
    {
        const int length = 7;
        const int start = 1;
        const int count = 3;
        const int stride = 2;

        foreach (DataBinding binding in DataBindings.All.Where(b => b.SupportsVars))
        {
            using NcTempFile hnd = new();

            AssertSuccess(Native.nc_def_dim(hnd.Id, "x", (nuint)length, out int dimId), $"nc_def_dim({binding.Name})");
            AssertSuccess(Native.nc_def_var(hnd.Id, binding.VarName, binding.Type, 1, [dimId], out int varId), $"nc_def_var({binding.Name})");
            AssertSuccess(Native.nc_enddef(hnd.Id), $"nc_enddef({binding.Name})");

            binding.PutVar(hnd.Id, varId, binding.ZeroArray(length));

            object stridedWrite = binding.NewArray(count);
            binding.CopyWindowSource(stridedWrite);
            binding.PutVars(hnd.Id, varId, start, count, stride, stridedWrite);

            object stridedRead = binding.NewArray(count);
            binding.GetVars(hnd.Id, varId, start, count, stride, stridedRead);
            binding.AssertEqual(stridedWrite, stridedRead);

            object full = binding.NewArray(length);
            binding.GetVar(hnd.Id, varId, full);
            binding.AssertStridedPositionsEqual(full, start, stride, stridedWrite);
        }
    }

    private sealed class Box<T>
    {
        public T Value = default!;
    }

    private sealed class DataBinding(
        string name,
        string varName,
        NCType type,
        object sourceValues,
        object var1Value,
        Func<int, object> newArray,
        Func<int, object> zeroArray,
        Func<object> newScalarBox,
        Action<int, int, object> putVar,
        Action<int, int, object> getVar,
        Action<int, int, int, object> putVar1,
        Action<int, int, int, object> getVar1,
        Action<int, int, int, int, object> putVara,
        Action<int, int, int, int, object> getVara,
        Action<int, int, int, int, int, object> putVars,
        Action<int, int, int, int, int, object> getVars,
        bool supportsVar1 = true,
        bool supportsVars = true)
    {
        public string Name { get; } = name;
        public string VarName { get; } = varName;
        public NCType Type { get; } = type;
        public object SourceValues { get; } = sourceValues;
        public object Var1Value { get; } = var1Value;
        public int Length => ((Array)SourceValues).Length;
        public Func<int, object> NewArray { get; } = newArray;
        public Func<int, object> ZeroArray { get; } = zeroArray;
        public Func<object> NewScalarBox { get; } = newScalarBox;
        public Action<int, int, object> PutVar { get; } = putVar;
        public Action<int, int, object> GetVar { get; } = getVar;
        public Action<int, int, int, object> PutVar1 { get; } = putVar1;
        public Action<int, int, int, object> GetVar1 { get; } = getVar1;
        public Action<int, int, int, int, object> PutVara { get; } = putVara;
        public Action<int, int, int, int, object> GetVara { get; } = getVara;
        public Action<int, int, int, int, int, object> PutVars { get; } = putVars;
        public Action<int, int, int, int, int, object> GetVars { get; } = getVars;
        public bool SupportsVar1 { get; } = supportsVar1;
        public bool SupportsVars { get; } = supportsVars;

        public void CopyWindowSource(object destination)
        {
            Array.Copy((Array)SourceValues, 0, (Array)destination, 0, ((Array)destination).Length);
        }

        public void AssertEqual(object expected, object actual)
        {
            if (expected is string[] es && actual is string[] @as)
            {
                Assert.Equal(es, @as);
                return;
            }

            Assert.Equal((Array)expected, (Array)actual);
        }

        public void AssertSingleIndexEquals(object full, int index, object expectedValue)
        {
            Array values = (Array)full;
            object? at = values.GetValue(index);
            Assert.Equal(expectedValue, at);

            for (int i = 0; i < values.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }

                object? current = values.GetValue(i);
                object? zero = ZeroArray(1) is Array z ? z.GetValue(0) : null;
                Assert.Equal(zero, current);
            }
        }

        public void AssertScalarEquals(object expectedValue, object box)
        {
            switch (box)
            {
                case Box<byte> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<sbyte> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<short> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<ushort> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<int> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<uint> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<long> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<ulong> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<float> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<double> b: Assert.Equal(expectedValue, b.Value); break;
                case Box<string> b: Assert.Equal(expectedValue, b.Value); break;
                default: throw new InvalidOperationException($"Unknown scalar box {box.GetType().Name}");
            }
        }

        public void AssertWindowEquals(object full, int start, object window)
        {
            Array fullArray = (Array)full;
            Array windowArray = (Array)window;

            for (int i = 0; i < windowArray.Length; i++)
            {
                Assert.Equal(windowArray.GetValue(i), fullArray.GetValue(start + i));
            }
        }

        public void AssertStridedPositionsEqual(object full, int start, int stride, object values)
        {
            Array fullArray = (Array)full;
            Array valuesArray = (Array)values;

            for (int i = 0; i < valuesArray.Length; i++)
            {
                Assert.Equal(valuesArray.GetValue(i), fullArray.GetValue(start + i * stride));
            }
        }
    }

    private static class DataBindings
    {
        public static readonly IReadOnlyList<DataBinding> All =
        [
            ForInt(),
            ForShort(),
            ForFloat(),
            ForDouble(),
            ForLong(),
            ForSByte(),
            ForByteAsUChar(),
            ForByteAsUByte(),
            ForUShort(),
            ForUInt(),
            ForLongLong(),
            ForULong(),
            ForText(),
            ForString(),
        ];

        public static readonly IReadOnlyList<DataBinding> Var1Stable =
        [
            ForInt(),
            ForShort(),
            ForFloat(),
            ForDouble(),
            ForLong(),
            ForSByte(),
            ForByteAsUChar(),
            ForByteAsUByte(),
            ForUShort(),
            ForUInt(),
            ForLongLong(),
            ForULong(),
            ForText(),
        ];

        private static DataBinding ForInt() => new(
            "int", "v_int", NCType.NC_INT,
            new[] { 3, 1, 4, 1, 5, 9, 2 },
            42,
            len => new int[len],
            len => new int[len],
            () => new Box<int>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_int(id, varid, (int[])a), "nc_put_var_int"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_int(id, varid, (int[])a), "nc_get_var_int"),
            (id, varid, idx, v) => { int value = (int)v; AssertSuccess(Native.nc_put_var1_int(id, varid, [(nuint)idx], ref value), "nc_put_var1_int"); },
            (id, varid, idx, box) =>
            {
                AssertSuccess(Native.nc_get_var1_int(id, varid, [(nuint)idx], out int v), "nc_get_var1_int");
                ((Box<int>)box).Value = v;
            },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_int(id, varid, [(IntPtr)start], [(IntPtr)count], (int[])a), "nc_put_vara_int"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_int(id, varid, [(IntPtr)start], [(IntPtr)count], (int[])a), "nc_get_vara_int"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_int(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (int[])a), "nc_put_vars_int"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_int(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (int[])a), "nc_get_vars_int"),
            true);

        private static DataBinding ForShort() => new(
            "short", "v_short", NCType.NC_SHORT,
            new short[] { -2, 0, 7, 1024, -11, 22, 3 },
            (short)12,
            len => new short[len],
            len => new short[len],
            () => new Box<short>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_short(id, varid, (short[])a), "nc_put_var_short"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_short(id, varid, (short[])a), "nc_get_var_short"),
            (id, varid, idx, v) => { short value = (short)v; AssertSuccess(Native.nc_put_var1_short(id, varid, [(nuint)idx], ref value), "nc_put_var1_short"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_short(id, varid, [(nuint)idx], out short v), "nc_get_var1_short"); ((Box<short>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_short(id, varid, [(IntPtr)start], [(IntPtr)count], (short[])a), "nc_put_vara_short"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_short(id, varid, [(IntPtr)start], [(IntPtr)count], (short[])a), "nc_get_vara_short"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_short(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (short[])a), "nc_put_vars_short"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_short(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (short[])a), "nc_get_vars_short"),
            true);

        private static DataBinding ForFloat() => new(
            "float", "v_float", NCType.NC_FLOAT,
            new[] { 0.5f, -3.25f, 9.0f, 11.5f, -0.25f, 7.75f, 2.5f },
            1.75f,
            len => new float[len],
            len => new float[len],
            () => new Box<float>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_float(id, varid, (float[])a), "nc_put_var_float"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_float(id, varid, (float[])a), "nc_get_var_float"),
            (id, varid, idx, v) => { float value = (float)v; AssertSuccess(Native.nc_put_var1_float(id, varid, [(nuint)idx], ref value), "nc_put_var1_float"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_float(id, varid, [(nuint)idx], out float v), "nc_get_var1_float"); ((Box<float>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_float(id, varid, [(IntPtr)start], [(IntPtr)count], (float[])a), "nc_put_vara_float"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_float(id, varid, [(IntPtr)start], [(IntPtr)count], (float[])a), "nc_get_vara_float"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_float(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (float[])a), "nc_put_vars_float"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_float(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (float[])a), "nc_get_vars_float"),
            true);

        private static DataBinding ForDouble() => new(
            "double", "v_double", NCType.NC_DOUBLE,
            new[] { 1.5d, -2.25d, 0d, 99.5d, -11d, 8.75d, 2d },
            123.25d,
            len => new double[len],
            len => new double[len],
            () => new Box<double>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_double(id, varid, (double[])a), "nc_put_var_double"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_double(id, varid, (double[])a), "nc_get_var_double"),
            (id, varid, idx, v) => { double value = (double)v; AssertSuccess(Native.nc_put_var1_double(id, varid, [(nuint)idx], ref value), "nc_put_var1_double"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_double(id, varid, [(nuint)idx], out double v), "nc_get_var1_double"); ((Box<double>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_double(id, varid, [(IntPtr)start], [(IntPtr)count], (double[])a), "nc_put_vara_double"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_double(id, varid, [(IntPtr)start], [(IntPtr)count], (double[])a), "nc_get_vara_double"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_double(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (double[])a), "nc_put_vars_double"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_double(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (double[])a), "nc_get_vars_double"),
            true);

        private static DataBinding ForLong() => new(
            "long", "v_long", NCType.NC_INT64,
            new long[] { -8, 4, 1024, -4096, 77, 1, 12 },
            451L,
            len => new long[len],
            len => new long[len],
            () => new Box<long>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_long(id, varid, (long[])a), "nc_put_var_long"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_long(id, varid, (long[])a), "nc_get_var_long"),
            (id, varid, idx, v) => { long value = (long)v; AssertSuccess(Native.nc_put_var1_long(id, varid, [(nuint)idx], ref value), "nc_put_var1_long"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_long(id, varid, [(nuint)idx], out long v), "nc_get_var1_long"); ((Box<long>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_long(id, varid, [(IntPtr)start], [(IntPtr)count], (long[])a), "nc_put_vara_long"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_long(id, varid, [(IntPtr)start], [(IntPtr)count], (long[])a), "nc_get_vara_long"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_long(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (long[])a), "nc_put_vars_long"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_long(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (long[])a), "nc_get_vars_long"),
            false,
            true);

        private static DataBinding ForSByte() => new(
            "schar", "v_schar", NCType.NC_BYTE,
            new sbyte[] { -4, 0, 11, 7, -9, 2, 1 },
            (sbyte)101,
            len => new sbyte[len],
            len => new sbyte[len],
            () => new Box<sbyte>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_schar(id, varid, (sbyte[])a), "nc_put_var_schar"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_schar(id, varid, (sbyte[])a), "nc_get_var_schar"),
            (id, varid, idx, v) => { sbyte value = (sbyte)v; AssertSuccess(Native.nc_put_var1_schar(id, varid, [(nuint)idx], ref value), "nc_put_var1_schar"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_schar(id, varid, [(nuint)idx], out sbyte v), "nc_get_var1_schar"); ((Box<sbyte>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_schar(id, varid, [(IntPtr)start], [(IntPtr)count], (sbyte[])a), "nc_put_vara_schar"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_schar(id, varid, [(IntPtr)start], [(IntPtr)count], (sbyte[])a), "nc_get_vara_schar"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_schar(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (sbyte[])a), "nc_put_vars_schar"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_schar(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (sbyte[])a), "nc_get_vars_schar"),
            true);

        private static DataBinding ForByteAsUChar() => new(
            "uchar", "v_uchar", NCType.NC_UBYTE,
            new byte[] { 1, 2, 3, 4, 5, 6, 7 },
            (byte)201,
            len => new byte[len],
            len => new byte[len],
            () => new Box<byte>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_uchar(id, varid, (byte[])a), "nc_put_var_uchar"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_uchar(id, varid, (byte[])a), "nc_get_var_uchar"),
            (id, varid, idx, v) => { byte value = (byte)v; AssertSuccess(Native.nc_put_var1_uchar(id, varid, [(nuint)idx], ref value), "nc_put_var1_uchar"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_uchar(id, varid, [(nuint)idx], out byte v), "nc_get_var1_uchar"); ((Box<byte>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_uchar(id, varid, [(IntPtr)start], [(IntPtr)count], (byte[])a), "nc_put_vara_uchar"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_uchar(id, varid, [(IntPtr)start], [(IntPtr)count], (byte[])a), "nc_get_vara_uchar"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_uchar(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (byte[])a), "nc_put_vars_uchar"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_uchar(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (byte[])a), "nc_get_vars_uchar"),
            true);

        private static DataBinding ForByteAsUByte() => new(
            "ubyte", "v_ubyte", NCType.NC_UBYTE,
            new byte[] { 8, 7, 6, 5, 4, 3, 2 },
            (byte)155,
            len => new byte[len],
            len => new byte[len],
            () => new Box<byte>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_ubyte(id, varid, (byte[])a), "nc_put_var_ubyte"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_ubyte(id, varid, (byte[])a), "nc_get_var_ubyte"),
            (id, varid, idx, v) => { byte value = (byte)v; AssertSuccess(Native.nc_put_var1_ubyte(id, varid, [(nuint)idx], ref value), "nc_put_var1_ubyte"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_ubyte(id, varid, [(nuint)idx], out byte v), "nc_get_var1_ubyte"); ((Box<byte>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_ubyte(id, varid, [(IntPtr)start], [(IntPtr)count], (byte[])a), "nc_put_vara_ubyte"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_ubyte(id, varid, [(IntPtr)start], [(IntPtr)count], (byte[])a), "nc_get_vara_ubyte"),
            (id, varid, start, count, stride, a) => throw SkipException.ForSkip("nc_put_vars_ubyte is not exposed in Native.cs"),
            (id, varid, start, count, stride, a) => throw SkipException.ForSkip("nc_get_vars_ubyte is not exposed in Native.cs"),
            false,
            false);

        private static DataBinding ForUShort() => new(
            "ushort", "v_ushort", NCType.NC_USHORT,
            new ushort[] { 1, 3, 5, 7, 9, 11, 13 },
            (ushort)321,
            len => new ushort[len],
            len => new ushort[len],
            () => new Box<ushort>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_ushort(id, varid, (ushort[])a), "nc_put_var_ushort"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_ushort(id, varid, (ushort[])a), "nc_get_var_ushort"),
            (id, varid, idx, v) => { ushort value = (ushort)v; AssertSuccess(Native.nc_put_var1_ushort(id, varid, [(nuint)idx], ref value), "nc_put_var1_ushort"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_ushort(id, varid, [(nuint)idx], out ushort v), "nc_get_var1_ushort"); ((Box<ushort>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_ushort(id, varid, [(IntPtr)start], [(IntPtr)count], (ushort[])a), "nc_put_vara_ushort"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_ushort(id, varid, [(IntPtr)start], [(IntPtr)count], (ushort[])a), "nc_get_vara_ushort"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_ushort(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (ushort[])a), "nc_put_vars_ushort"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_ushort(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (ushort[])a), "nc_get_vars_ushort"),
            true);

        private static DataBinding ForUInt() => new(
            "uint", "v_uint", NCType.NC_UINT,
            new uint[] { 2, 4, 8, 16, 32, 64, 128 },
            202u,
            len => new uint[len],
            len => new uint[len],
            () => new Box<uint>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_uint(id, varid, (uint[])a), "nc_put_var_uint"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_uint(id, varid, (uint[])a), "nc_get_var_uint"),
            (id, varid, idx, v) => { uint value = (uint)v; AssertSuccess(Native.nc_put_var1_uint(id, varid, [(nuint)idx], ref value), "nc_put_var1_uint"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_uint(id, varid, [(nuint)idx], out uint v), "nc_get_var1_uint"); ((Box<uint>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_uint(id, varid, [(IntPtr)start], [(IntPtr)count], (uint[])a), "nc_put_vara_uint"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_uint(id, varid, [(IntPtr)start], [(IntPtr)count], (uint[])a), "nc_get_vara_uint"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_uint(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (uint[])a), "nc_put_vars_uint"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_uint(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (uint[])a), "nc_get_vars_uint"),
            true);

        private static DataBinding ForLongLong() => new(
            "longlong", "v_longlong", NCType.NC_INT64,
            new long[] { -1, -2, 3, 5, 8, 13, 21 },
            999L,
            len => new long[len],
            len => new long[len],
            () => new Box<long>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_longlong(id, varid, (long[])a), "nc_put_var_longlong"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_longlong(id, varid, (long[])a), "nc_get_var_longlong"),
            (id, varid, idx, v) => { long value = (long)v; AssertSuccess(Native.nc_put_var1_longlong(id, varid, [(nuint)idx], ref value), "nc_put_var1_longlong"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_longlong(id, varid, [(nuint)idx], out long v), "nc_get_var1_longlong"); ((Box<long>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_longlong(id, varid, [(IntPtr)start], [(IntPtr)count], (long[])a), "nc_put_vara_longlong"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_longlong(id, varid, [(IntPtr)start], [(IntPtr)count], (long[])a), "nc_get_vara_longlong"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_longlong(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (long[])a), "nc_put_vars_longlong"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_longlong(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (long[])a), "nc_get_vars_longlong"),
            false,
            true);

        private static DataBinding ForULong() => new(
            "ulonglong", "v_ulonglong", NCType.NC_UINT64,
            new ulong[] { 1, 2, 3, 5, 8, 13, 21 },
            333UL,
            len => new ulong[len],
            len => new ulong[len],
            () => new Box<ulong>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_ulonglong(id, varid, (ulong[])a), "nc_put_var_ulonglong"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_ulonglong(id, varid, (ulong[])a), "nc_get_var_ulonglong"),
            (id, varid, idx, v) => { ulong value = (ulong)v; AssertSuccess(Native.nc_put_var1_ulonglong(id, varid, [(nuint)idx], ref value), "nc_put_var1_ulonglong"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_ulonglong(id, varid, [(nuint)idx], out ulong v), "nc_get_var1_ulonglong"); ((Box<ulong>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_ulonglong(id, varid, [(IntPtr)start], [(IntPtr)count], (ulong[])a), "nc_put_vara_ulonglong"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_ulonglong(id, varid, [(IntPtr)start], [(IntPtr)count], (ulong[])a), "nc_get_vara_ulonglong"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_ulonglong(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (ulong[])a), "nc_put_vars_ulonglong"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_ulonglong(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (ulong[])a), "nc_get_vars_ulonglong"),
            false,
            true);

        private static DataBinding ForText() => new(
            "text", "v_text", NCType.NC_CHAR,
            new byte[] { (byte)'n', (byte)'e', (byte)'t', (byte)'c', (byte)'d', (byte)'f', (byte)'!' },
            (byte)'X',
            len => new byte[len],
            len => new byte[len],
            () => new Box<byte>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_text(id, varid, (byte[])a), "nc_put_var_text"),
            (id, varid, a) => AssertSuccess(Native.nc_get_var_text(id, varid, (byte[])a), "nc_get_var_text"),
            (id, varid, idx, v) => { byte value = (byte)v; AssertSuccess(Native.nc_put_var1_text(id, varid, [(nuint)idx], ref value), "nc_put_var1_text"); },
            (id, varid, idx, box) => { AssertSuccess(Native.nc_get_var1_text(id, varid, [(nuint)idx], out byte v), "nc_get_var1_text"); ((Box<byte>)box).Value = v; },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_text(id, varid, [(IntPtr)start], [(IntPtr)count], (byte[])a), "nc_put_vara_text"),
            (id, varid, start, count, a) => AssertSuccess(Native.nc_get_vara_text(id, varid, [(IntPtr)start], [(IntPtr)count], (byte[])a), "nc_get_vara_text"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_put_vars_text(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (byte[])a), "nc_put_vars_text"),
            (id, varid, start, count, stride, a) => AssertSuccess(Native.nc_get_vars_text(id, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], (byte[])a), "nc_get_vars_text"),
            false,
            true);

        private static DataBinding ForString() => new(
            "string", "v_string", NCType.NC_STRING,
            new[] { "alpha", "bravo", "charlie", "delta", "echo", "foxtrot", "golf" },
            "zulu",
            len => new string[len],
            len => Enumerable.Repeat(string.Empty, len).ToArray(),
            () => new Box<string>(),
            (id, varid, a) => AssertSuccess(Native.nc_put_var_string(id, varid, (string[])a), "nc_put_var_string"),
            (id, varid, a) => ReadVarString(id, varid, (string[])a),
            (id, varid, idx, v) => AssertSuccess(Native.nc_put_var1_string(id, varid, [(nuint)idx], (string)v), "nc_put_var1_string"),
            (id, varid, idx, box) =>
            {
                IntPtr[] ptrs = [IntPtr.Zero];
                try
                {
                    AssertSuccess(Native.nc_get_var1_string(id, varid, [(nuint)idx], ptrs), "nc_get_var1_string");
                    ((Box<string>)box).Value = Marshal.PtrToStringAnsi(ptrs[0]) ?? string.Empty;
                }
                finally
                {
                    AssertSuccess(Native.nc_free_string((IntPtr)1, ptrs), "nc_free_string(var1)");
                }
            },
            (id, varid, start, count, a) => AssertSuccess(Native.nc_put_vara_string(id, varid, [(IntPtr)start], [(IntPtr)count], (string[])a), "nc_put_vara_string"),
            (id, varid, start, count, a) => ReadVaraStringViaGetVars(id, varid, start, count, (string[])a),
            (id, varid, start, count, stride, a) =>
            {
                AssertSuccess(Native.nc_put_vars_string(id, varid, [(nuint)start], [(nuint)count], [(nint)stride], (string[])a), "nc_put_vars_string");
            },
            (id, varid, start, count, stride, a) => ReadVarsString(id, varid, start, count, stride, (string[])a),
            false,
            true);

        private static void ReadVarString(int ncid, int varid, string[] destination)
        {
            IntPtr[] ptrs = new IntPtr[destination.Length];
            try
            {
                AssertSuccess(Native.nc_get_var_string(ncid, varid, ptrs), "nc_get_var_string");
                for (int i = 0; i < ptrs.Length; i++)
                {
                    destination[i] = Marshal.PtrToStringAnsi(ptrs[i]) ?? string.Empty;
                }
            }
            finally
            {
                AssertSuccess(Native.nc_free_string((IntPtr)ptrs.Length, ptrs), "nc_free_string(var)");
            }
        }

        private static void ReadVaraStringViaGetVars(int ncid, int varid, int start, int count, string[] destination)
        {
            ReadVarsString(ncid, varid, start, count, 1, destination);
        }

        private static void ReadVarsString(int ncid, int varid, int start, int count, int stride, string[] destination)
        {
            IntPtr[] ptrs = new IntPtr[count];
            try
            {
                AssertSuccess(Native.nc_get_vars_string(ncid, varid, [(IntPtr)start], [(IntPtr)count], [(IntPtr)stride], ptrs), "nc_get_vars_string");
                for (int i = 0; i < count; i++)
                {
                    destination[i] = Marshal.PtrToStringAnsi(ptrs[i]) ?? string.Empty;
                }
            }
            finally
            {
                AssertSuccess(Native.nc_free_string((IntPtr)ptrs.Length, ptrs), "nc_free_string(vars)");
            }
        }
    }
}
