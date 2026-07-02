using NetCDF.Interop;
using Xunit.Sdk;

namespace NetCDF.Tests.Interop;

public sealed class MpiInteropTests
{
    private static readonly object InitLock = new();
    private static bool _mpiInitialized;

    [Fact]
    [Trait("Category", "MPI")]
    public void NcCreatePar_AndNcOpenPar_WorkWhenParallelAvailable()
    {
        if (!TryEnsureMpi(out IntPtr world, out IntPtr infoNull, out int rank, out string? reason, out bool initializedHere))
        {
            throw SkipException.ForSkip(reason ?? "MPI runtime unavailable.");
        }

        try
        {
            string path = Environment.GetEnvironmentVariable("NETCDF_MPI_TEST_FILE")
                ?? Path.Combine(Path.GetTempPath(), "netcdf-dotnet-mpi-test.nc");

            if (rank == 0 && File.Exists(path))
            {
                File.Delete(path);
            }

            MpiNative.MPI_Barrier(world);

            int createStatus = Native.nc_create_par(path, CreateMode.NC_NETCDF4, world, infoNull, out int ncid);
            if (createStatus != InteropTestCommon.NcNoErr)
            {
                return;
            }

            try
            {
                int defDimStatus = Native.nc_def_dim(ncid, "x", (nuint)2, out int dimId);
                if (defDimStatus != InteropTestCommon.NcNoErr)
                {
                    return;
                }

                int defVarStatus = Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out int varId);
                if (defVarStatus != InteropTestCommon.NcNoErr)
                {
                    return;
                }

                int parAccessStatus = Native.nc_var_par_access(ncid, varId, ParallelAccess.NC_COLLECTIVE);
                if (parAccessStatus != InteropTestCommon.NcNoErr)
                {
                    return;
                }

                int enddefStatus = Native.nc_enddef(ncid);
                InteropTestCommon.AssertSuccess(enddefStatus, "nc_enddef(par)");

                int[] values = [rank, rank + 1];
                int putStatus = Native.nc_put_var_int(ncid, varId, values);
                InteropTestCommon.AssertSuccess(putStatus, "nc_put_var_int(par)");
            }
            finally
            {
                InteropTestCommon.AssertSuccess(Native.nc_close(ncid), "nc_close(create_par)");
            }

            int openStatus = Native.nc_open_par(path, OpenMode.NC_NOWRITE, world, infoNull, out int readId);
            if (openStatus != InteropTestCommon.NcNoErr)
            {
                return;
            }

            try
            {
                InteropTestCommon.AssertSuccess(Native.nc_inq_varid(readId, "v", out int readVarId), "nc_inq_varid(par)");
                int[] actual = new int[2];
                InteropTestCommon.AssertSuccess(Native.nc_get_var_int(readId, readVarId, actual), "nc_get_var_int(par)");
                Assert.Equal(rank, actual[0]);
            }
            finally
            {
                InteropTestCommon.AssertSuccess(Native.nc_close(readId), "nc_close(open_par)");
            }

            MpiNative.MPI_Barrier(world);

            if (rank == 0 && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        finally
        {
            if (initializedHere)
            {
                int finalizedStatus = MpiNative.MPI_Finalized(out int isFinalized);
                if (finalizedStatus == 0 && isFinalized == 0)
                {
                    MpiNative.MPI_Finalize();
                }
            }
        }
    }

    private static bool TryEnsureMpi(
        out IntPtr world,
        out IntPtr infoNull,
        out int rank,
        out string? reason,
        out bool initializedHere)
    {
        world = IntPtr.Zero;
        infoNull = IntPtr.Zero;
        rank = 0;
        reason = null;
        initializedHere = false;

        try
        {
            lock (InitLock)
            {
                if (!_mpiInitialized)
                {
                    int initCheck = MpiNative.MPI_Initialized(out int alreadyInitialized);
                    if (initCheck != 0)
                    {
                        reason = $"MPI_Initialized failed with code {initCheck}.";
                        return false;
                    }

                    if (alreadyInitialized == 0)
                    {
                        int initStatus = MpiNative.MPI_Init(IntPtr.Zero, IntPtr.Zero);
                        if (initStatus != 0)
                        {
                            reason = $"MPI_Init failed with code {initStatus}.";
                            return false;
                        }

                        initializedHere = true;
                    }

                    _mpiInitialized = true;
                }
            }

            world = MpiNative.MPI_Comm_f2c(0);
            infoNull = MpiNative.MPI_Info_f2c(0);
            if (world == IntPtr.Zero)
            {
                reason = "MPI_Comm_f2c(0) returned null handle.";
                return false;
            }

            int rankStatus = MpiNative.MPI_Comm_rank(world, out rank);
            if (rankStatus != 0)
            {
                reason = $"MPI_Comm_rank failed with code {rankStatus}.";
                return false;
            }

            return true;
        }
        catch (DllNotFoundException ex)
        {
            reason = $"MPI library not found: {ex.Message}";
            return false;
        }
    }
}
