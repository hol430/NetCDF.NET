using NetCDF.Interop;
using NetCDF.LowLevel;
using NetCDF.Tests.Interop;
using Xunit.Sdk;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.Tests.LowLevel;

public sealed class NetCdfApiMpiTests
{
    private static readonly object InitLock = new();
    private static bool mpiInitialized;

    [Fact]
    [Trait("Category", "MPI")]
    public void ParallelOpenCreateWrappers_WorkWhenParallelNetcdfIsAvailable()
    {
        using MpiContext mpi = MpiContext.Require();
        NetCdfApi api = new();
        string path = Path.Combine(Path.GetTempPath(), "nc-lowlevel-mpi.nc");

        if (mpi.Rank == 0 && File.Exists(path))
        {
            File.Delete(path);
        }

        MpiNative.MPI_Barrier(mpi.World.Value);

        NetCdfHandle created;
        try
        {
            created = api.CreateParallel(path, CreateMode.NC_CLOBBER | CreateMode.NC_MPIIO, mpi.World, mpi.InfoNull);
        }
        catch (NetCdfException ex) when (IsFeatureUnavailable(ex.StatusCode))
        {
            throw SkipException.ForSkip($"parallel netCDF unavailable in this runtime: {ex.Message}");
        }

        using (created)
        {
            DimensionId x = api.DefineDimension(created, "x", 2);
            VariableId v = api.DefineVariable(created, "v", NCType.NC_INT, [x]);
            api.SetVariableParallelAccess(created, v, ParallelAccess.NC_COLLECTIVE);
            api.EndDefineMode(created);
            api.WriteVariable(created, v, [mpi.Rank, mpi.Rank + 1]);
        }

        MpiNative.MPI_Barrier(mpi.World.Value);

        using NetCdfHandle opened = api.OpenParallel(path, OpenMode.NC_NOWRITE | OpenMode.NC_MPIIO, mpi.World, mpi.InfoNull);
        VariableId variable = api.InquireVariableId(opened, "v");
        int[] values = new int[2];
        api.ReadVariable(opened, variable, values);
        Assert.Equal(mpi.Rank, values[0]);

        MpiNative.MPI_Barrier(mpi.World.Value);
        if (mpi.Rank == 0 && File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static bool IsFeatureUnavailable(int status)
        => status is NcEnopar or NcEnotBuilt or NcEinval;

    private static bool TryEnsureMpi(out MpiCommunicator world, out MpiInfo infoNull, out int rank, out string? reason)
    {
        world = default;
        infoNull = default;
        rank = 0;
        reason = null;

        try
        {
            lock (InitLock)
            {
                if (MpiNative.MPI_Finalized(out int finalized) != 0 || finalized != 0)
                {
                    reason = "MPI is finalized or MPI_Finalized failed.";
                    return false;
                }

                if (!mpiInitialized)
                {
                    if (MpiNative.MPI_Initialized(out int initialized) != 0)
                    {
                        reason = "MPI_Initialized failed.";
                        return false;
                    }

                    if (initialized == 0 && MpiNative.MPI_Init(IntPtr.Zero, IntPtr.Zero) != 0)
                    {
                        reason = "MPI_Init failed.";
                        return false;
                    }

                    mpiInitialized = true;
                }
            }

            IntPtr worldPtr = MpiNative.MPI_Comm_f2c(0);
            IntPtr infoPtr = MpiNative.MPI_Info_f2c(0);
            if (worldPtr == IntPtr.Zero)
            {
                reason = "MPI_Comm_f2c(0) returned null.";
                return false;
            }

            if (MpiNative.MPI_Comm_rank(worldPtr, out rank) != 0)
            {
                reason = "MPI_Comm_rank failed.";
                return false;
            }

            world = new MpiCommunicator(worldPtr);
            infoNull = new MpiInfo(infoPtr);
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
        private MpiContext(MpiCommunicator world, MpiInfo infoNull, int rank)
        {
            World = world;
            InfoNull = infoNull;
            Rank = rank;
        }

        public MpiCommunicator World { get; }

        public MpiInfo InfoNull { get; }

        public int Rank { get; }

        public static MpiContext Require()
        {
            if (!TryEnsureMpi(out MpiCommunicator world, out MpiInfo infoNull, out int rank, out string? reason))
            {
                throw SkipException.ForSkip(reason ?? "MPI runtime unavailable.");
            }

            return new MpiContext(world, infoNull, rank);
        }

        public void Dispose()
        {
        }
    }
}
