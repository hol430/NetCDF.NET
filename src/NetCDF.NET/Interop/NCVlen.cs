using System.Runtime.InteropServices;

namespace NetCDF.Interop;

[StructLayout(LayoutKind.Sequential)]
public struct Vlen
{
    public nuint Len; // size_t - length
    public IntPtr p; // void* - pointer to the data
}
