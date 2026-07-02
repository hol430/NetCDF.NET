using NetCDF.Interop;

namespace NetCDF.Tests.Interop;

public sealed class MpiInteropTests
{
    private static readonly object InitLock = new();
    private static bool _mpiInitialized;

    [Fact]
    [Trait("Category", "MPI")]
    public void NcCreatePar_AndNcOpenPar_WorkForClassicFiles_WhenPnetcdfAvailable()
    {
        using MpiContext mpi = MpiContext.Require();
        ParallelFileMode mode = new(
            InteropTestCommon.FeatureMpiClassic,
            "classic/PnetCDF parallel I/O",
            CreateMode.NC_CLOBBER | CreateMode.NC_MPIIO,
            OpenMode.NC_NOWRITE | OpenMode.NC_MPIIO);

        ProbeParallelVariableAccess(mpi, mode);
        RoundTripParallelIntVariable(mpi, mode, "classic");
    }

    [Fact]
    [Trait("Category", "MPI")]
    public void NcCreatePar_AndNcOpenPar_WorkForNetcdf4Files_WhenParallelHdf5Available()
    {
        using MpiContext mpi = MpiContext.Require();
        ParallelFileMode mode = new(
            InteropTestCommon.FeatureParallel4,
            "netCDF-4/HDF5 parallel I/O",
            CreateMode.NC_NETCDF4,
            OpenMode.NC_NOWRITE);

        ProbeParallelVariableAccess(mpi, mode);
        RoundTripParallelIntVariable(mpi, mode, "netcdf4");
    }

    private static void ProbeParallelVariableAccess(MpiContext mpi, ParallelFileMode mode)
    {
        string path = BuildTempPath($"probe-{mode.FeatureName}");
        DeleteIfRankZero(mpi, path);
        MpiNative.MPI_Barrier(mpi.World);

        int createStatus = Native.nc_create_par(path, mode.CreateMode, mpi.World, mpi.InfoNull, out int ncid);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
            createStatus,
            $"nc_create_par({mode.Name})",
            mode.FeatureName,
            InteropTestCommon.NcEnopar);

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)1, out int dimId), $"nc_def_dim({mode.Name} probe)");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out int varId), $"nc_def_var({mode.Name} probe)");

            int parAccessStatus = Native.nc_var_par_access(ncid, varId, ParallelAccess.NC_COLLECTIVE);
            InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(
                parAccessStatus,
                $"nc_var_par_access({mode.Name} probe)",
                mode.FeatureName,
                InteropTestCommon.NcEnopar);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), $"nc_close({mode.Name} probe)");
        }

        MpiNative.MPI_Barrier(mpi.World);
        DeleteIfRankZero(mpi, path);
    }

    private static void RoundTripParallelIntVariable(MpiContext mpi, ParallelFileMode mode, string fileNameSuffix)
    {
        string path = Environment.GetEnvironmentVariable("NETCDF_MPI_TEST_FILE")
            ?? BuildTempPath(fileNameSuffix);

        DeleteIfRankZero(mpi, path);
        MpiNative.MPI_Barrier(mpi.World);

        int createStatus = Native.nc_create_par(path, mode.CreateMode, mpi.World, mpi.InfoNull, out int ncid);
        InteropTestCommon.AssertSuccess(createStatus, $"nc_create_par({mode.Name})");

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_def_dim(ncid, "x", (nuint)2, out int dimId), $"nc_def_dim({mode.Name})");
            InteropTestCommon.AssertSuccess(Native.nc_def_var(ncid, "v", NCType.NC_INT, 1, [dimId], out int varId), $"nc_def_var({mode.Name})");
            InteropTestCommon.AssertSuccess(Native.nc_var_par_access(ncid, varId, ParallelAccess.NC_COLLECTIVE), $"nc_var_par_access({mode.Name})");
            InteropTestCommon.AssertSuccess(Native.nc_enddef(ncid), $"nc_enddef({mode.Name})");

            int[] values = [mpi.Rank, mpi.Rank + 1];
            InteropTestCommon.AssertSuccess(Native.nc_put_var_int(ncid, varId, values), $"nc_put_var_int({mode.Name})");
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_close(ncid), $"nc_close(create_par {mode.Name})");
        }

        int openStatus = Native.nc_open_par(path, mode.OpenMode, mpi.World, mpi.InfoNull, out int readId);
        InteropTestCommon.AssertSuccess(openStatus, $"nc_open_par({mode.Name})");

        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_inq_varid(readId, "v", out int readVarId), $"nc_inq_varid({mode.Name})");
            int[] actual = new int[2];
            InteropTestCommon.AssertSuccess(Native.nc_get_var_int(readId, readVarId, actual), $"nc_get_var_int({mode.Name})");
            Assert.Equal(mpi.Rank, actual[0]);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_close(readId), $"nc_close(open_par {mode.Name})");
        }

        MpiNative.MPI_Barrier(mpi.World);
        DeleteIfRankZero(mpi, path);
    }

    private static string BuildTempPath(string suffix)
        => Path.Combine(Path.GetTempPath(), $"nc-mpi-{suffix}.nc");

    private static void DeleteIfRankZero(MpiContext mpi, string path)
    {
        if (mpi.Rank == 0 && File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static bool TryEnsureMpi(out IntPtr world, out IntPtr infoNull, out int rank, out string? reason)
    {
        world = IntPtr.Zero;
        infoNull = IntPtr.Zero;
        rank = 0;
        reason = null;

        try
        {
            lock (InitLock)
            {
                int finalizedStatus = MpiNative.MPI_Finalized(out int isFinalized);
                if (finalizedStatus != 0)
                {
                    reason = $"MPI_Finalized failed with code {finalizedStatus}.";
                    return false;
                }

                if (isFinalized != 0)
                {
                    reason = "MPI has already been finalized.";
                    return false;
                }

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

    private sealed class MpiContext : IDisposable
    {
        private MpiContext(IntPtr world, IntPtr infoNull, int rank)
        {
            World = world;
            InfoNull = infoNull;
            Rank = rank;
        }

        public IntPtr World { get; }
        public IntPtr InfoNull { get; }
        public int Rank { get; }

        public static MpiContext Require()
        {
            if (!TryEnsureMpi(out IntPtr world, out IntPtr infoNull, out int rank, out string? reason))
            {
                InteropTestCommon.SkipOrFailFeatureUnavailable(
                    InteropTestCommon.FeatureMpi,
                    "MPI runtime",
                    reason ?? "MPI runtime unavailable");
            }

            return new MpiContext(world, infoNull, rank);
        }

        public void Dispose()
        {
            // MPI is process-global. Leave it initialized so multiple MPI tests can run in one test process.
        }
    }

    private sealed record ParallelFileMode(string FeatureName, string Name, CreateMode CreateMode, OpenMode OpenMode);
}
