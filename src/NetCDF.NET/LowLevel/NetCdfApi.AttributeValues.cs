using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Writes numeric attribute values.
    /// </summary>
    /// <typeparam name="T">The managed attribute element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF external data type to store.</param>
    /// <param name="values">The attribute values.</param>
    public void WriteAttribute<T>(NetCdfHandle handle, VariableId variableId, string name, NCType type, T[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, type={Type}, elementType={ElementType}, elementCount={ElementCount}", "nc_put_att", ncid, variableId.Value, name, type, typeof(T).Name, values.Length);

        int status = PutAttribute(ncid, variableId.Value, name, type, values);
        LogReturned("nc_put_att", status);
        Check(status, "nc_put_att");
    }

    /// <summary>
    /// Reads numeric attribute values.
    /// </summary>
    /// <typeparam name="T">The managed attribute element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="destination">The destination array.</param>
    public void ReadAttribute<T>(NetCdfHandle handle, VariableId variableId, string name, T[] destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, elementType={ElementType}, elementCount={ElementCount}", "nc_get_att", ncid, variableId.Value, name, typeof(T).Name, destination.Length);

        int status = GetAttribute(ncid, variableId.Value, name, destination);
        LogReturned("nc_get_att", status);
        Check(status, "nc_get_att");
    }

    /// <summary>
    /// Writes a text attribute.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The text value.</param>
    public void WriteTextAttribute(NetCdfHandle handle, VariableId variableId, string name, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(value);
        int ncid = handle.Id;
        nuint length = (nuint)Encoding.UTF8.GetByteCount(value);
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, byteCount={ByteCount}", nameof(Native.nc_put_att_text), ncid, variableId.Value, name, length);

        int status = Native.nc_put_att_text(ncid, variableId.Value, name, length, value);
        LogReturned(nameof(Native.nc_put_att_text), status);
        Check(status, nameof(Native.nc_put_att_text));
    }

    /// <summary>
    /// Reads a text attribute.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The text attribute value.</returns>
    public string ReadTextAttribute(NetCdfHandle handle, VariableId variableId, string name)
    {
        AttributeInfo info = InquireAttribute(handle, variableId, name);
        byte[] data = new byte[info.Length];
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, byteCount={ByteCount}", nameof(Native.nc_get_att_text), ncid, variableId.Value, name, data.Length);

        int status = Native.nc_get_att_text(ncid, variableId.Value, name, data);
        LogReturned(nameof(Native.nc_get_att_text), status);
        Check(status, nameof(Native.nc_get_att_text));

        return Encoding.UTF8.GetString(data);
    }

    /// <summary>
    /// Writes string attribute values.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="values">The string values.</param>
    public void WriteStringAttribute(NetCdfHandle handle, VariableId variableId, string name, string[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, elementCount={ElementCount}", nameof(Native.nc_put_att_string), ncid, variableId.Value, name, values.Length);

        int status = Native.nc_put_att_string(ncid, variableId.Value, name, (nuint)values.Length, values);
        LogReturned(nameof(Native.nc_put_att_string), status);
        Check(status, nameof(Native.nc_put_att_string));
    }

    /// <summary>
    /// Reads string attribute values.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <returns>The string attribute values.</returns>
    public string[] ReadStringAttribute(NetCdfHandle handle, VariableId variableId, string name)
    {
        AttributeInfo info = InquireAttribute(handle, variableId, name);
        string[] values = new string[info.Length];
        IntPtr[] ptrs = new IntPtr[info.Length];
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, elementCount={ElementCount}", nameof(Native.nc_get_att_string), ncid, variableId.Value, name, ptrs.Length);

        int status = Native.nc_get_att_string(ncid, variableId.Value, name, ptrs);
        if (status == NcNoErr)
        {
            for (int i = 0; i < ptrs.Length; i++)
            {
                values[i] = Marshal.PtrToStringUTF8(ptrs[i]) ?? string.Empty;
            }
        }

        int freeStatus = Native.nc_free_string((nuint)ptrs.Length, ptrs);
        LogReturned(nameof(Native.nc_get_att_string), status);
        Check(status == NcNoErr ? freeStatus : status, nameof(Native.nc_get_att_string));

        return values;
    }

    /// <summary>
    /// Writes native C <c>long</c> attribute values.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF external data type to store.</param>
    /// <param name="values">The attribute values.</param>
    public void WriteNativeLongAttribute(NetCdfHandle handle, VariableId variableId, string name, NCType type, long[] values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, type={Type}, elementCount={ElementCount}", "nc_put_att_long", ncid, variableId.Value, name, type, values.Length);

        int status = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Native.Windows.nc_put_att_long(ncid, variableId.Value, name, type, (nuint)values.Length, ToNativeLong32(values))
            : Native.Unix.nc_put_att_long(ncid, variableId.Value, name, type, (nuint)values.Length, values);
        LogReturned("nc_put_att_long", status);
        Check(status, "nc_put_att_long");
    }

    /// <summary>
    /// Reads native C <c>long</c> attribute values.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID, or <see cref="VariableId.Global"/> for a global attribute.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="destination">The destination values.</param>
    public void ReadNativeLongAttribute(NetCdfHandle handle, VariableId variableId, string name, long[] destination)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(destination);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}, elementCount={ElementCount}", "nc_get_att_long", ncid, variableId.Value, name, destination.Length);

        int status;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            int[] values = new int[destination.Length];
            status = Native.Windows.nc_get_att_long(ncid, variableId.Value, name, values);
            for (int i = 0; i < values.Length; i++)
            {
                destination[i] = values[i];
            }
        }
        else
        {
            status = Native.Unix.nc_get_att_long(ncid, variableId.Value, name, destination);
        }

        LogReturned("nc_get_att_long", status);
        Check(status, "nc_get_att_long");
    }

    private int PutAttribute<T>(int ncid, int varid, string name, NCType type, T[] values)
        => values switch
        {
            sbyte[] v => Native.nc_put_att_schar(ncid, varid, name, type, (nuint)v.Length, v),
            byte[] v => Native.nc_put_att_ubyte(ncid, varid, name, type, (nuint)v.Length, v),
            short[] v => Native.nc_put_att_short(ncid, varid, name, type, (nuint)v.Length, v),
            int[] v => Native.nc_put_att_int(ncid, varid, name, type, (nuint)v.Length, v),
            float[] v => Native.nc_put_att_float(ncid, varid, name, type, (nuint)v.Length, v),
            double[] v => Native.nc_put_att_double(ncid, varid, name, type, (nuint)v.Length, v),
            ushort[] v => Native.nc_put_att_ushort(ncid, varid, name, type, (nuint)v.Length, v),
            uint[] v => Native.nc_put_att_uint(ncid, varid, name, type, (nuint)v.Length, v),
            long[] v => Native.nc_put_att_longlong(ncid, varid, name, type, (nuint)v.Length, v),
            ulong[] v => Native.nc_put_att_ulonglong(ncid, varid, name, type, (nuint)v.Length, v),
            _ => throw UnsupportedElementType<T>()
        };

    private int GetAttribute<T>(int ncid, int varid, string name, T[] destination)
        => destination switch
        {
            sbyte[] v => Native.nc_get_att_schar(ncid, varid, name, v),
            byte[] v => Native.nc_get_att_ubyte(ncid, varid, name, v),
            short[] v => Native.nc_get_att_short(ncid, varid, name, v),
            int[] v => Native.nc_get_att_int(ncid, varid, name, v),
            float[] v => Native.nc_get_att_float(ncid, varid, name, v),
            double[] v => Native.nc_get_att_double(ncid, varid, name, v),
            ushort[] v => Native.nc_get_att_ushort(ncid, varid, name, v),
            uint[] v => Native.nc_get_att_uint(ncid, varid, name, v),
            long[] v => Native.nc_get_att_longlong(ncid, varid, name, v),
            ulong[] v => Native.nc_get_att_ulonglong(ncid, varid, name, v),
            _ => throw UnsupportedElementType<T>()
        };
}
