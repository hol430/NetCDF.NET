using System.Text;
using NC.Net.Interop;

namespace NetCDF.NET.Tests;

public sealed class NativeSmokeTests
{
    [Fact]
    public void NcInqLibvers_ReturnsNonEmptyString()
    {
        var version = Native.nc_inq_libvers();

        Assert.False(string.IsNullOrWhiteSpace(version));
        Assert.Contains('.', version);
    }

    [Fact]
    public void NcStrerror_ForNegativeCode_ReturnsMessage()
    {
        string message = Native.nc_strerror(-1);
        Assert.False(string.IsNullOrWhiteSpace(message));
    }

    [Fact]
    public void NcOpen_ForMissingFile_ReturnsError()
    {
        string fileName = $"netcdf-net-tests-missing-{Guid.NewGuid():N}.nc";
        string missingPath = Path.Combine(Path.GetTempPath(), fileName);

        int status = Native.nc_open(missingPath, OpenMode.NC_NOWRITE, out _);

        Assert.NotEqual(0, status);
    }

    [Fact]
    public void CreateDefineAndInquire_MetadataMatches()
    {
        string path = TempFilePath();
        int ncid = -1;

        try
        {
            AssertSuccess(Native.nc_create(path, CreateMode.NC_NETCDF4, out ncid), "nc_create");
            AssertSuccess(Native.nc_def_dim(ncid, "time", (IntPtr)4, out int dimid), "nc_def_dim");
            AssertSuccess(Native.nc_def_var(ncid, "values", NCType.NC_INT, 1, [dimid], out _), "nc_def_var");

            AssertSuccess(Native.nc_inq_ndims(ncid, out int ndims), "nc_inq_ndims");
            Assert.Equal(1, ndims);

            AssertSuccess(Native.nc_inq_nvars(ncid, out int nvars), "nc_inq_nvars");
            Assert.Equal(1, nvars);

            AssertSuccess(Native.nc_inq_dimid(ncid, "time", out int timeDimId), "nc_inq_dimid");
            Assert.Equal(dimid, timeDimId);

            AssertSuccess(Native.nc_inq_dimlen(ncid, dimid, out IntPtr length), "nc_inq_dimlen");
            Assert.Equal((IntPtr)4, length);

            var dimName = new StringBuilder(capacity: 256);
            AssertSuccess(Native.nc_inq_dim(ncid, dimid, dimName, out _), "nc_inq_dim");
            Assert.Equal("time", dimName.ToString());
        }
        finally
        {
            CloseIfOpen(ncid);
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void CreateWriteRead_IntVariableRoundTrip()
    {
        var path = TempFilePath();
        var createNcid = -1;

        try
        {
            AssertSuccess(Native.nc_create(path, CreateMode.NC_NETCDF4, out createNcid), "nc_create");
            AssertSuccess(Native.nc_def_dim(createNcid, "x", (IntPtr)5, out var dimid), "nc_def_dim");
            AssertSuccess(Native.nc_def_var(createNcid, "v", NCType.NC_INT, 1, [dimid], out var varid), "nc_def_var");
            AssertSuccess(Native.nc_enddef(createNcid), "nc_enddef");

            var expected = new[] { 3, 1, 4, 1, 5 };
            AssertSuccess(Native.nc_put_var_int(createNcid, varid, expected), "nc_put_var_int");
            AssertSuccess(Native.nc_close(createNcid), "nc_close(create)");
            createNcid = -1;

            AssertSuccess(Native.nc_open(path, OpenMode.NC_NOWRITE, out var readNcid), "nc_open");
            try
            {
                AssertSuccess(Native.nc_inq_varid(readNcid, "v", out var readVarId), "nc_inq_varid");

                var actual = new int[expected.Length];
                AssertSuccess(Native.nc_get_var_int(readNcid, readVarId, actual), "nc_get_var_int");
                Assert.Equal(expected, actual);
            }
            finally
            {
                CloseIfOpen(readNcid);
            }
        }
        finally
        {
            CloseIfOpen(createNcid);
            DeleteIfExists(path);
        }
    }

    private static string TempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), $"netcdf-net-tests-{Guid.NewGuid():N}.nc");
    }

    private static void AssertSuccess(int status, string operation)
    {
        if (status == 0)
        {
            return;
        }

        var errorMessage = Native.nc_strerror(status);
        throw new Xunit.Sdk.XunitException($"{operation} failed with status {status}: {errorMessage}");
    }

    private static void CloseIfOpen(int ncid)
    {
        if (ncid >= 0)
        {
            _ = Native.nc_close(ncid);
        }
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
