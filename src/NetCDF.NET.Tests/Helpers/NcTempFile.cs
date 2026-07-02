using NetCDF.Interop;

namespace NetCDF.Tests.Helpers;

public sealed class NcTempFile : IDisposable
{
    private readonly NcFileHandle handle;
    private readonly TempFile tempFile;
    private bool handleClosed;

    public int Id => handle.Id;
    public string Path => tempFile.FilePath;

    public NcTempFile(CreateMode mode = CreateMode.NC_CLOBBER, string? featureName = null)
    {
        tempFile = new TempFile();
        handle = NcFileHandle.Create(tempFile.FilePath, mode, featureName);
    }

    public NcTempFile(NetcdfTestFormat format)
        : this(format.CreateMode, format.FeatureName)
    {
    }

    public void CloseHandle()
    {
        if (handleClosed)
        {
            return;
        }

        handle.Dispose();
        handleClosed = true;
    }

    public void Dispose()
    {
        CloseHandle();
        tempFile.Dispose();
    }
}
