using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Writes all values of a variable.
    /// </summary>
    /// <typeparam name="T">The managed element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="values">The values to write.</param>
    public void WriteVariable<T>(NetCdfHandle handle, VariableId variableId, T[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, elementType={ElementType}, elementCount={ElementCount}", "nc_put_var", ncid, variableId.Value, typeof(T).Name, values.Length);

        int status = PutVariable(ncid, variableId.Value, values);
        LogReturned("nc_put_var", status);
        Check(status, "nc_put_var");
    }

    /// <summary>
    /// Reads all values of a variable into a destination array.
    /// </summary>
    /// <typeparam name="T">The managed element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="destination">The destination array.</param>
    public void ReadVariable<T>(NetCdfHandle handle, VariableId variableId, T[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, elementType={ElementType}, elementCount={ElementCount}", "nc_get_var", ncid, variableId.Value, typeof(T).Name, destination.Length);

        int status = GetVariable(ncid, variableId.Value, destination);
        LogReturned("nc_get_var", status);
        Check(status, "nc_get_var");
    }

    /// <summary>
    /// Writes one value of a variable.
    /// </summary>
    /// <typeparam name="T">The managed element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="index">The zero-based element index.</param>
    /// <param name="value">The value to write.</param>
    public void WriteVariableValue<T>(NetCdfHandle handle, VariableId variableId, VariableIndex index, T value)
    {
        int ncid = handle.Id;
        nuint[] nativeIndex = index.ToNative();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, index=[{Index}], elementType={ElementType}", "nc_put_var1", ncid, variableId.Value, string.Join(", ", index.Coordinates), typeof(T).Name);

        int status = PutVariableValue(ncid, variableId.Value, nativeIndex, value);
        LogReturned("nc_put_var1", status);
        Check(status, "nc_put_var1");
    }

    /// <summary>
    /// Reads one value of a variable.
    /// </summary>
    /// <typeparam name="T">The managed element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="index">The zero-based element index.</param>
    /// <returns>The value at the requested index.</returns>
    public T ReadVariableValue<T>(NetCdfHandle handle, VariableId variableId, VariableIndex index)
    {
        int ncid = handle.Id;
        nuint[] nativeIndex = index.ToNative();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, index=[{Index}], elementType={ElementType}", "nc_get_var1", ncid, variableId.Value, string.Join(", ", index.Coordinates), typeof(T).Name);

        int status = GetVariableValue(ncid, variableId.Value, nativeIndex, out T value);
        LogReturned("nc_get_var1", status);
        Check(status, "nc_get_var1");
        return value;
    }

    /// <summary>
    /// Writes a hyperslab selection of a variable.
    /// </summary>
    /// <typeparam name="T">The managed element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="selection">The hyperslab selection.</param>
    /// <param name="values">The values to write.</param>
    public void WriteVariable<T>(NetCdfHandle handle, VariableId variableId, Hyperslab selection, T[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        nuint[] nativeStart = selection.ToNativeStart();
        nuint[] nativeCount = selection.ToNativeCount();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, start=[{Start}], count=[{Count}], stride=[{Stride}], elementType={ElementType}, elementCount={ElementCount}", selection.IsStrided ? "nc_put_vars" : "nc_put_vara", ncid, variableId.Value, string.Join(", ", selection.Start), string.Join(", ", selection.Count), selection.Stride is null ? string.Empty : string.Join(", ", selection.Stride), typeof(T).Name, values.Length);

        int status = selection.IsStrided
            ? PutVariableStrided(ncid, variableId.Value, nativeStart, nativeCount, selection.ToNativeStride(), values)
            : PutVariableSection(ncid, variableId.Value, nativeStart, nativeCount, values);
        string functionName = selection.IsStrided ? "nc_put_vars" : "nc_put_vara";
        LogReturned(functionName, status);
        Check(status, functionName);
    }

    /// <summary>
    /// Reads a hyperslab selection of a variable.
    /// </summary>
    /// <typeparam name="T">The managed element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="selection">The hyperslab selection.</param>
    /// <param name="destination">The destination array.</param>
    public void ReadVariable<T>(NetCdfHandle handle, VariableId variableId, Hyperslab selection, T[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        nuint[] nativeStart = selection.ToNativeStart();
        nuint[] nativeCount = selection.ToNativeCount();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, start=[{Start}], count=[{Count}], stride=[{Stride}], elementType={ElementType}, elementCount={ElementCount}", selection.IsStrided ? "nc_get_vars" : "nc_get_vara", ncid, variableId.Value, string.Join(", ", selection.Start), string.Join(", ", selection.Count), selection.Stride is null ? string.Empty : string.Join(", ", selection.Stride), typeof(T).Name, destination.Length);

        int status = selection.IsStrided
            ? GetVariableStrided(ncid, variableId.Value, nativeStart, nativeCount, selection.ToNativeStride(), destination)
            : GetVariableSection(ncid, variableId.Value, nativeStart, nativeCount, destination);
        string functionName = selection.IsStrided ? "nc_get_vars" : "nc_get_vara";
        LogReturned(functionName, status);
        Check(status, functionName);
    }

    /// <summary>
    /// Writes all text bytes of an <see cref="NCType.NC_CHAR"/> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="values">The text bytes to write.</param>
    public void WriteTextVariable(NetCdfHandle handle, VariableId variableId, byte[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, byteCount={ByteCount}", nameof(Native.nc_put_var_text), ncid, variableId.Value, values.Length);

        int status = Native.nc_put_var_text(ncid, variableId.Value, values);
        LogReturned(nameof(Native.nc_put_var_text), status);
        Check(status, nameof(Native.nc_put_var_text));
    }

    /// <summary>
    /// Reads all text bytes of an <see cref="NCType.NC_CHAR"/> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="destination">The destination text byte array.</param>
    public void ReadTextVariable(NetCdfHandle handle, VariableId variableId, byte[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, byteCount={ByteCount}", nameof(Native.nc_get_var_text), ncid, variableId.Value, destination.Length);

        int status = Native.nc_get_var_text(ncid, variableId.Value, destination);
        LogReturned(nameof(Native.nc_get_var_text), status);
        Check(status, nameof(Native.nc_get_var_text));
    }

    /// <summary>
    /// Writes one text byte of an <see cref="NCType.NC_CHAR"/> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="index">The zero-based character index.</param>
    /// <param name="value">The text byte to write.</param>
    public void WriteTextVariableValue(NetCdfHandle handle, VariableId variableId, VariableIndex index, byte value)
    {
        int ncid = handle.Id;
        nuint[] nativeIndex = index.ToNative();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, index=[{Index}]", nameof(Native.nc_put_var1_text), ncid, variableId.Value, string.Join(", ", index.Coordinates));

        int status = Native.nc_put_var1_text(ncid, variableId.Value, nativeIndex, ref value);
        LogReturned(nameof(Native.nc_put_var1_text), status);
        Check(status, nameof(Native.nc_put_var1_text));
    }

    /// <summary>
    /// Reads one text byte of an <see cref="NCType.NC_CHAR"/> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="index">The zero-based character index.</param>
    /// <returns>The text byte at the requested index.</returns>
    public byte ReadTextVariableValue(NetCdfHandle handle, VariableId variableId, VariableIndex index)
    {
        int ncid = handle.Id;
        nuint[] nativeIndex = index.ToNative();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, index=[{Index}]", nameof(Native.nc_get_var1_text), ncid, variableId.Value, string.Join(", ", index.Coordinates));

        int status = Native.nc_get_var1_text(ncid, variableId.Value, nativeIndex, out byte value);
        LogReturned(nameof(Native.nc_get_var1_text), status);
        Check(status, nameof(Native.nc_get_var1_text));
        return value;
    }

    /// <summary>
    /// Writes a text hyperslab of an <see cref="NCType.NC_CHAR"/> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="selection">The hyperslab selection.</param>
    /// <param name="values">The text bytes to write.</param>
    public void WriteTextVariable(NetCdfHandle handle, VariableId variableId, Hyperslab selection, byte[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        nuint[] start = selection.ToNativeStart();
        nuint[] count = selection.ToNativeCount();
        string functionName = selection.IsStrided ? nameof(Native.nc_put_vars_text) : nameof(Native.nc_put_vara_text);
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, start=[{Start}], count=[{Count}], stride=[{Stride}], byteCount={ByteCount}", functionName, ncid, variableId.Value, string.Join(", ", selection.Start), string.Join(", ", selection.Count), selection.Stride is null ? string.Empty : string.Join(", ", selection.Stride), values.Length);

        int status = selection.IsStrided
            ? Native.nc_put_vars_text(ncid, variableId.Value, start, count, selection.ToNativeStride(), values)
            : Native.nc_put_vara_text(ncid, variableId.Value, start, count, values);
        LogReturned(functionName, status);
        Check(status, functionName);
    }

    /// <summary>
    /// Reads a text hyperslab of an <see cref="NCType.NC_CHAR"/> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="selection">The hyperslab selection.</param>
    /// <param name="destination">The destination text byte array.</param>
    public void ReadTextVariable(NetCdfHandle handle, VariableId variableId, Hyperslab selection, byte[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        nuint[] start = selection.ToNativeStart();
        nuint[] count = selection.ToNativeCount();
        string functionName = selection.IsStrided ? nameof(Native.nc_get_vars_text) : nameof(Native.nc_get_vara_text);
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, start=[{Start}], count=[{Count}], stride=[{Stride}], byteCount={ByteCount}", functionName, ncid, variableId.Value, string.Join(", ", selection.Start), string.Join(", ", selection.Count), selection.Stride is null ? string.Empty : string.Join(", ", selection.Stride), destination.Length);

        int status = selection.IsStrided
            ? Native.nc_get_vars_text(ncid, variableId.Value, start, count, selection.ToNativeStride(), destination)
            : Native.nc_get_vara_text(ncid, variableId.Value, start, count, destination);
        LogReturned(functionName, status);
        Check(status, functionName);
    }

    /// <summary>
    /// Writes all values of a native C <c>long</c> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="values">The values to write.</param>
    public void WriteNativeLongVariable(NetCdfHandle handle, VariableId variableId, long[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, elementCount={ElementCount}", "nc_put_var_long", ncid, variableId.Value, values.Length);

        int status = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Native.Windows.nc_put_var_long(ncid, variableId.Value, ToNativeLong32(values))
            : Native.Unix.nc_put_var_long(ncid, variableId.Value, values);
        LogReturned("nc_put_var_long", status);
        Check(status, "nc_put_var_long");
    }

    /// <summary>
    /// Reads all values of a native C <c>long</c> variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="destination">The destination values.</param>
    public void ReadNativeLongVariable(NetCdfHandle handle, VariableId variableId, long[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, elementCount={ElementCount}", "nc_get_var_long", ncid, variableId.Value, destination.Length);

        int status;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int[] values = new int[destination.Length];
            status = Native.Windows.nc_get_var_long(ncid, variableId.Value, values);
            for (int i = 0; i < values.Length; i++)
            {
                destination[i] = values[i];
            }
        }
        else
        {
            status = Native.Unix.nc_get_var_long(ncid, variableId.Value, destination);
        }

        LogReturned("nc_get_var_long", status);
        Check(status, "nc_get_var_long");
    }

    /// <summary>
    /// Writes one native C <c>long</c> value.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="index">The zero-based element index.</param>
    /// <param name="value">The value to write.</param>
    public void WriteNativeLongVariableValue(NetCdfHandle handle, VariableId variableId, VariableIndex index, long value)
    {
        int ncid = handle.Id;
        nuint[] nativeIndex = index.ToNative();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, index=[{Index}]", "nc_put_var1_long", ncid, variableId.Value, string.Join(", ", index.Coordinates));

        int status;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int nativeValue = checked((int)value);
            status = Native.Windows.nc_put_var1_long(ncid, variableId.Value, nativeIndex, ref nativeValue);
        }
        else
        {
            status = Native.Unix.nc_put_var1_long(ncid, variableId.Value, nativeIndex, ref value);
        }

        LogReturned("nc_put_var1_long", status);
        Check(status, "nc_put_var1_long");
    }

    /// <summary>
    /// Reads one native C <c>long</c> value.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="index">The zero-based element index.</param>
    /// <returns>The value at the requested index.</returns>
    public long ReadNativeLongVariableValue(NetCdfHandle handle, VariableId variableId, VariableIndex index)
    {
        int ncid = handle.Id;
        nuint[] nativeIndex = index.ToNative();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, index=[{Index}]", "nc_get_var1_long", ncid, variableId.Value, string.Join(", ", index.Coordinates));

        int status;
        long value;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            status = Native.Windows.nc_get_var1_long(ncid, variableId.Value, nativeIndex, out int nativeValue);
            value = nativeValue;
        }
        else
        {
            status = Native.Unix.nc_get_var1_long(ncid, variableId.Value, nativeIndex, out value);
        }

        LogReturned("nc_get_var1_long", status);
        Check(status, "nc_get_var1_long");
        return value;
    }

    /// <summary>
    /// Writes a native C <c>long</c> hyperslab selection.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="selection">The hyperslab selection.</param>
    /// <param name="values">The values to write.</param>
    public void WriteNativeLongVariable(NetCdfHandle handle, VariableId variableId, Hyperslab selection, long[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        nuint[] start = selection.ToNativeStart();
        nuint[] count = selection.ToNativeCount();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, start=[{Start}], count=[{Count}], stride=[{Stride}], elementCount={ElementCount}", selection.IsStrided ? "nc_put_vars_long" : "nc_put_vara_long", ncid, variableId.Value, string.Join(", ", selection.Start), string.Join(", ", selection.Count), selection.Stride is null ? string.Empty : string.Join(", ", selection.Stride), values.Length);

        int status;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int[] nativeValues = ToNativeLong32(values);
            status = selection.IsStrided
                ? Native.Windows.nc_put_vars_long(ncid, variableId.Value, start, count, selection.ToNativeStride(), nativeValues)
                : Native.Windows.nc_put_vara_long(ncid, variableId.Value, start, count, nativeValues);
        }
        else
        {
            status = selection.IsStrided
                ? Native.Unix.nc_put_vars_long(ncid, variableId.Value, start, count, selection.ToNativeStride(), values)
                : Native.Unix.nc_put_vara_long(ncid, variableId.Value, start, count, values);
        }

        string functionName = selection.IsStrided ? "nc_put_vars_long" : "nc_put_vara_long";
        LogReturned(functionName, status);
        Check(status, functionName);
    }

    /// <summary>
    /// Reads a native C <c>long</c> hyperslab selection.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="selection">The hyperslab selection.</param>
    /// <param name="destination">The destination values.</param>
    public void ReadNativeLongVariable(NetCdfHandle handle, VariableId variableId, Hyperslab selection, long[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        nuint[] start = selection.ToNativeStart();
        nuint[] count = selection.ToNativeCount();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, start=[{Start}], count=[{Count}], stride=[{Stride}], elementCount={ElementCount}", selection.IsStrided ? "nc_get_vars_long" : "nc_get_vara_long", ncid, variableId.Value, string.Join(", ", selection.Start), string.Join(", ", selection.Count), selection.Stride is null ? string.Empty : string.Join(", ", selection.Stride), destination.Length);

        int status;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int[] nativeValues = new int[destination.Length];
            status = selection.IsStrided
                ? Native.Windows.nc_get_vars_long(ncid, variableId.Value, start, count, selection.ToNativeStride(), nativeValues)
                : Native.Windows.nc_get_vara_long(ncid, variableId.Value, start, count, nativeValues);
            for (int i = 0; i < nativeValues.Length; i++)
            {
                destination[i] = nativeValues[i];
            }
        }
        else
        {
            status = selection.IsStrided
                ? Native.Unix.nc_get_vars_long(ncid, variableId.Value, start, count, selection.ToNativeStride(), destination)
                : Native.Unix.nc_get_vara_long(ncid, variableId.Value, start, count, destination);
        }

        string functionName = selection.IsStrided ? "nc_get_vars_long" : "nc_get_vara_long";
        LogReturned(functionName, status);
        Check(status, functionName);
    }

    private int PutVariable<T>(int ncid, int varid, T[] values)
        => values switch
        {
            sbyte[] v => Native.nc_put_var_schar(ncid, varid, v),
            byte[] v => Native.nc_put_var_ubyte(ncid, varid, v),
            short[] v => Native.nc_put_var_short(ncid, varid, v),
            int[] v => Native.nc_put_var_int(ncid, varid, v),
            float[] v => Native.nc_put_var_float(ncid, varid, v),
            double[] v => Native.nc_put_var_double(ncid, varid, v),
            ushort[] v => Native.nc_put_var_ushort(ncid, varid, v),
            uint[] v => Native.nc_put_var_uint(ncid, varid, v),
            long[] v => Native.nc_put_var_longlong(ncid, varid, v),
            ulong[] v => Native.nc_put_var_ulonglong(ncid, varid, v),
            string[] v => Native.nc_put_var_string(ncid, varid, v),
            _ => throw UnsupportedElementType<T>()
        };

    private int GetVariable<T>(int ncid, int varid, T[] destination)
    {
        if (destination is string[] strings)
        {
            return GetNativeStrings(strings.Length, ptrs => Native.nc_get_var_string(ncid, varid, ptrs), strings);
        }

        return destination switch
        {
            sbyte[] v => Native.nc_get_var_schar(ncid, varid, v),
            byte[] v => Native.nc_get_var_ubyte(ncid, varid, v),
            short[] v => Native.nc_get_var_short(ncid, varid, v),
            int[] v => Native.nc_get_var_int(ncid, varid, v),
            float[] v => Native.nc_get_var_float(ncid, varid, v),
            double[] v => Native.nc_get_var_double(ncid, varid, v),
            ushort[] v => Native.nc_get_var_ushort(ncid, varid, v),
            uint[] v => Native.nc_get_var_uint(ncid, varid, v),
            long[] v => Native.nc_get_var_longlong(ncid, varid, v),
            ulong[] v => Native.nc_get_var_ulonglong(ncid, varid, v),
            _ => throw UnsupportedElementType<T>()
        };
    }

    private int PutVariableSection<T>(int ncid, int varid, nuint[] start, nuint[] count, T[] values)
        => values switch
        {
            sbyte[] v => Native.nc_put_vara_schar(ncid, varid, start, count, v),
            byte[] v => Native.nc_put_vara_ubyte(ncid, varid, start, count, v),
            short[] v => Native.nc_put_vara_short(ncid, varid, start, count, v),
            int[] v => Native.nc_put_vara_int(ncid, varid, start, count, v),
            float[] v => Native.nc_put_vara_float(ncid, varid, start, count, v),
            double[] v => Native.nc_put_vara_double(ncid, varid, start, count, v),
            ushort[] v => Native.nc_put_vara_ushort(ncid, varid, start, count, v),
            uint[] v => Native.nc_put_vara_uint(ncid, varid, start, count, v),
            long[] v => Native.nc_put_vara_longlong(ncid, varid, start, count, v),
            ulong[] v => Native.nc_put_vara_ulonglong(ncid, varid, start, count, v),
            string[] v => Native.nc_put_vara_string(ncid, varid, start, count, v),
            _ => throw UnsupportedElementType<T>()
        };

    private int GetVariableSection<T>(int ncid, int varid, nuint[] start, nuint[] count, T[] destination)
    {
        if (destination is string[] strings)
        {
            return GetNativeStrings(strings.Length, ptrs => Native.nc_get_vara_string(ncid, varid, start, count, ptrs), strings);
        }

        return destination switch
        {
            sbyte[] v => Native.nc_get_vara_schar(ncid, varid, start, count, v),
            byte[] v => Native.nc_get_vara_ubyte(ncid, varid, start, count, v),
            short[] v => Native.nc_get_vara_short(ncid, varid, start, count, v),
            int[] v => Native.nc_get_vara_int(ncid, varid, start, count, v),
            float[] v => Native.nc_get_vara_float(ncid, varid, start, count, v),
            double[] v => Native.nc_get_vara_double(ncid, varid, start, count, v),
            ushort[] v => Native.nc_get_vara_ushort(ncid, varid, start, count, v),
            uint[] v => Native.nc_get_vara_uint(ncid, varid, start, count, v),
            long[] v => Native.nc_get_vara_longlong(ncid, varid, start, count, v),
            ulong[] v => Native.nc_get_vara_ulonglong(ncid, varid, start, count, v),
            _ => throw UnsupportedElementType<T>()
        };
    }

    private int PutVariableStrided<T>(int ncid, int varid, nuint[] start, nuint[] count, nint[] stride, T[] values)
        => values switch
        {
            sbyte[] v => Native.nc_put_vars_schar(ncid, varid, start, count, stride, v),
            byte[] v => Native.nc_put_vars_uchar(ncid, varid, start, count, stride, v),
            short[] v => Native.nc_put_vars_short(ncid, varid, start, count, stride, v),
            int[] v => Native.nc_put_vars_int(ncid, varid, start, count, stride, v),
            float[] v => Native.nc_put_vars_float(ncid, varid, start, count, stride, v),
            double[] v => Native.nc_put_vars_double(ncid, varid, start, count, stride, v),
            ushort[] v => Native.nc_put_vars_ushort(ncid, varid, start, count, stride, v),
            uint[] v => Native.nc_put_vars_uint(ncid, varid, start, count, stride, v),
            long[] v => Native.nc_put_vars_longlong(ncid, varid, start, count, stride, v),
            ulong[] v => Native.nc_put_vars_ulonglong(ncid, varid, start, count, stride, v),
            string[] v => Native.nc_put_vars_string(ncid, varid, start, count, stride, v),
            _ => throw UnsupportedElementType<T>()
        };

    private int GetVariableStrided<T>(int ncid, int varid, nuint[] start, nuint[] count, nint[] stride, T[] destination)
    {
        if (destination is string[] strings)
        {
            return GetNativeStrings(strings.Length, ptrs => Native.nc_get_vars_string(ncid, varid, start, count, stride, ptrs), strings);
        }

        return destination switch
        {
            sbyte[] v => Native.nc_get_vars_schar(ncid, varid, start, count, stride, v),
            byte[] v => Native.nc_get_vars_uchar(ncid, varid, start, count, stride, v),
            short[] v => Native.nc_get_vars_short(ncid, varid, start, count, stride, v),
            int[] v => Native.nc_get_vars_int(ncid, varid, start, count, stride, v),
            float[] v => Native.nc_get_vars_float(ncid, varid, start, count, stride, v),
            double[] v => Native.nc_get_vars_double(ncid, varid, start, count, stride, v),
            ushort[] v => Native.nc_get_vars_ushort(ncid, varid, start, count, stride, v),
            uint[] v => Native.nc_get_vars_uint(ncid, varid, start, count, stride, v),
            long[] v => Native.nc_get_vars_longlong(ncid, varid, start, count, stride, v),
            ulong[] v => Native.nc_get_vars_ulonglong(ncid, varid, start, count, stride, v),
            _ => throw UnsupportedElementType<T>()
        };
    }

    private int PutVariableValue<T>(int ncid, int varid, nuint[] index, T value)
    {
        switch (value)
        {
            case sbyte v: return Native.nc_put_var1_schar(ncid, varid, index, ref v);
            case byte v: return Native.nc_put_var1_ubyte(ncid, varid, index, ref v);
            case short v: return Native.nc_put_var1_short(ncid, varid, index, ref v);
            case int v: return Native.nc_put_var1_int(ncid, varid, index, ref v);
            case float v: return Native.nc_put_var1_float(ncid, varid, index, ref v);
            case double v: return Native.nc_put_var1_double(ncid, varid, index, ref v);
            case ushort v: return Native.nc_put_var1_ushort(ncid, varid, index, ref v);
            case uint v: return Native.nc_put_var1_uint(ncid, varid, index, ref v);
            case long v: return Native.nc_put_var1_longlong(ncid, varid, index, ref v);
            case ulong v: return Native.nc_put_var1_ulonglong(ncid, varid, index, ref v);
            case string v: return Native.nc_put_var1_string(ncid, varid, index, [v]);
            default: throw UnsupportedElementType<T>();
        }
    }

    private int GetVariableValue<T>(int ncid, int varid, nuint[] index, out T value)
    {
        object boxed;
        int status;
        if (typeof(T) == typeof(sbyte)) { status = Native.nc_get_var1_schar(ncid, varid, index, out sbyte v); boxed = v; }
        else if (typeof(T) == typeof(byte)) { status = Native.nc_get_var1_ubyte(ncid, varid, index, out byte v); boxed = v; }
        else if (typeof(T) == typeof(short)) { status = Native.nc_get_var1_short(ncid, varid, index, out short v); boxed = v; }
        else if (typeof(T) == typeof(int)) { status = Native.nc_get_var1_int(ncid, varid, index, out int v); boxed = v; }
        else if (typeof(T) == typeof(float)) { status = Native.nc_get_var1_float(ncid, varid, index, out float v); boxed = v; }
        else if (typeof(T) == typeof(double)) { status = Native.nc_get_var1_double(ncid, varid, index, out double v); boxed = v; }
        else if (typeof(T) == typeof(ushort)) { status = Native.nc_get_var1_ushort(ncid, varid, index, out ushort v); boxed = v; }
        else if (typeof(T) == typeof(uint)) { status = Native.nc_get_var1_uint(ncid, varid, index, out uint v); boxed = v; }
        else if (typeof(T) == typeof(long)) { status = Native.nc_get_var1_longlong(ncid, varid, index, out long v); boxed = v; }
        else if (typeof(T) == typeof(ulong)) { status = Native.nc_get_var1_ulonglong(ncid, varid, index, out ulong v); boxed = v; }
        else if (typeof(T) == typeof(string))
        {
            string[] result = new string[1];
            status = GetNativeStrings(1, ptrs => Native.nc_get_var1_string(ncid, varid, index, ptrs), result);
            boxed = result[0];
        }
        else
        {
            throw UnsupportedElementType<T>();
        }

        value = (T)boxed;
        return status;
    }

    private int GetNativeStrings(int length, Func<IntPtr[], int> read, string[] destination)
    {
        IntPtr[] ptrs = new IntPtr[length];
        int status = read(ptrs);
        if (status == NcNoErr)
        {
            for (int i = 0; i < ptrs.Length; i++)
            {
                destination[i] = Marshal.PtrToStringUTF8(ptrs[i]) ?? string.Empty;
            }
        }

        int freeStatus = Native.nc_free_string((nuint)ptrs.Length, ptrs);
        return status == NcNoErr ? freeStatus : status;
    }

    private static NotSupportedException UnsupportedElementType<T>()
        => new($"The element type {typeof(T).FullName} is not supported by this wrapper.");

    private static int[] ToNativeLong32(long[] values)
    {
        int[] result = new int[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            result[i] = checked((int)values[i]);
        }

        return result;
    }
}
