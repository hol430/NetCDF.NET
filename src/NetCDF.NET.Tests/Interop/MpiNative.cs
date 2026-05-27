using System.Runtime.InteropServices;

namespace NetCDF.Tests.Interop;

internal static class MpiNative
{
    private const string library = "mpi";

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int MPI_Initialized(out int flag);

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int MPI_Init(IntPtr argc, IntPtr argv);

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int MPI_Finalized(out int flag);

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int MPI_Finalize();

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int MPI_Comm_rank(IntPtr comm, out int rank);

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern int MPI_Barrier(IntPtr comm);

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr MPI_Comm_f2c(int comm);

    [DllImport(library, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr MPI_Info_f2c(int info);
}
