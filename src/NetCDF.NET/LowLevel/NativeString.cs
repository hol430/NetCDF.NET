using System.Text;

namespace NetCDF.LowLevel;

internal static class NativeString
{
    public static string DecodeNullTerminatedUtf8(byte[] buffer)
    {
        int length = Array.IndexOf(buffer, (byte)0);
        if (length < 0)
        {
            length = buffer.Length;
        }

        return Encoding.UTF8.GetString(buffer, 0, length);
    }
}
