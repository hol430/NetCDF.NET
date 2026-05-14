namespace NetCDF.Tests.Helpers;

public sealed class TempFile : IDisposable
{
    public string FilePath { get; }

    public TempFile(string? extension = ".nc")
    {
        string suffix = string.IsNullOrWhiteSpace(extension) ? ".tmp" : extension;
        FilePath = Path.Combine(Path.GetTempPath(), $"netcdf-tests-{Guid.NewGuid():N}{suffix}");
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
        catch
        {
            // Best-effort cleanup.
        }
    }
}
