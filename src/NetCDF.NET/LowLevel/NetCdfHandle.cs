namespace NetCDF.LowLevel;

/// <summary>
/// Owned netCDF file or group handle.
/// </summary>
public sealed class NetCdfHandle : IDisposable
{
    private readonly NetCdfApi api;
    private bool disposed;
    private int id;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetCdfHandle"/> class.
    /// </summary>
    /// <param name="id">The NetCDF file ID.</param>
    /// <param name="api">The NetCDF API instance.</param>
    internal NetCdfHandle(int id, NetCdfApi api)
    {
        this.id = id;
        this.api = api;
    }

    /// <summary>
    /// Gets the NetCDF file ID.
    /// </summary>
    internal int Id
    {
        get
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            return id;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the handle has been closed.
    /// </summary>
    public bool IsClosed => disposed;

    /// <summary>
    /// Closes the NetCDF handle and releases any associated resources.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
            return;

        api.Close(this);
        MarkClosed();
    }

    internal void MarkClosed()
    {
        id = -1;
        disposed = true;
    }
}
