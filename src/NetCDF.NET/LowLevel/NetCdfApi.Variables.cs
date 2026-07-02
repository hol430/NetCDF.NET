using NetCDF.Interop;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Defines a new variable in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="type">The netCDF external data type.</param>
    /// <param name="dimensions">The dimensions used by the variable.</param>
    /// <returns>The ID of the newly defined variable.</returns>
    public VariableId DefineVariable(NetCdfHandle handle, string name, NCType type, IReadOnlyList<DimensionId> dimensions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(dimensions);

        int ncid = handle.Id;
        int[] dimids = dimensions.Select(static d => d.Value).ToArray();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}, type={Type}, dimensions=[{Dimensions}]", nameof(Native.nc_def_var), ncid, name, type, string.Join(", ", dimids));

        int status = Native.nc_def_var(ncid, name, type, dimids.Length, dimids, out int varid);
        LogReturned(nameof(Native.nc_def_var), status);
        Check(status, nameof(Native.nc_def_var));

        return new VariableId(varid);
    }

    /// <summary>
    /// Disables fill values for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    public unsafe void DefineVariableNoFill(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, noFill={NoFill}", nameof(Native.nc_def_var_fill), ncid, variableId.Value, true);

        int status = Native.nc_def_var_fill(ncid, variableId.Value, 1, null);
        LogReturned(nameof(Native.nc_def_var_fill), status);
        Check(status, nameof(Native.nc_def_var_fill));
    }

    /// <summary>
    /// Defines the fill value for a variable.
    /// </summary>
    /// <typeparam name="T">The unmanaged fill value type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="fillValue">The fill value.</param>
    public unsafe void DefineVariableFill<T>(NetCdfHandle handle, VariableId variableId, T fillValue)
        where T : unmanaged
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, noFill={NoFill}, fillType={FillType}", nameof(Native.nc_def_var_fill), ncid, variableId.Value, false, typeof(T).Name);

        int status = Native.nc_def_var_fill(ncid, variableId.Value, 0, &fillValue);
        LogReturned(nameof(Native.nc_def_var_fill), status);
        Check(status, nameof(Native.nc_def_var_fill));
    }

    /// <summary>
    /// Configures deflate compression for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="shuffle">Whether to enable the shuffle filter.</param>
    /// <param name="deflate">Whether to enable deflate compression.</param>
    /// <param name="deflateLevel">The deflate compression level.</param>
    public void DefineVariableDeflate(NetCdfHandle handle, VariableId variableId, bool shuffle, bool deflate, int deflateLevel)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, shuffle={Shuffle}, deflate={Deflate}, level={Level}", nameof(Native.nc_def_var_deflate), ncid, variableId.Value, shuffle, deflate, deflateLevel);

        int status = Native.nc_def_var_deflate(ncid, variableId.Value, shuffle ? 1 : 0, deflate ? 1 : 0, deflateLevel);
        LogReturned(nameof(Native.nc_def_var_deflate), status);
        Check(status, nameof(Native.nc_def_var_deflate));
    }

    /// <summary>
    /// Configures the fletcher32 checksum filter for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="enabled">Whether to enable fletcher32 checksums.</param>
    public void DefineVariableFletcher32(NetCdfHandle handle, VariableId variableId, bool enabled)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, enabled={Enabled}", nameof(Native.nc_def_var_fletcher32), ncid, variableId.Value, enabled);

        int status = Native.nc_def_var_fletcher32(ncid, variableId.Value, enabled ? 1 : 0);
        LogReturned(nameof(Native.nc_def_var_fletcher32), status);
        Check(status, nameof(Native.nc_def_var_fletcher32));
    }

    /// <summary>
    /// Configures variable chunking.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="storage">The storage layout.</param>
    /// <param name="chunkSizes">The chunk sizes, or <see langword="null"/> for contiguous storage.</param>
    public void DefineVariableChunking(NetCdfHandle handle, VariableId variableId, VariableStorage storage, IReadOnlyList<nuint>? chunkSizes)
    {
        int ncid = handle.Id;
        nuint[]? chunks = chunkSizes?.ToArray();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, storage={Storage}, chunks=[{Chunks}]", nameof(Native.nc_def_var_chunking), ncid, variableId.Value, storage, chunks is null ? string.Empty : string.Join(", ", chunks));

        int status = Native.nc_def_var_chunking(ncid, variableId.Value, (int)storage, chunks);
        LogReturned(nameof(Native.nc_def_var_chunking), status);
        Check(status, nameof(Native.nc_def_var_chunking));
    }

    /// <summary>
    /// Configures endian storage for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="endian">The endian storage setting.</param>
    public void DefineVariableEndian(NetCdfHandle handle, VariableId variableId, VariableEndian endian)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, endian={Endian}", nameof(Native.nc_def_var_endian), ncid, variableId.Value, endian);

        int status = Native.nc_def_var_endian(ncid, variableId.Value, (int)endian);
        LogReturned(nameof(Native.nc_def_var_endian), status);
        Check(status, nameof(Native.nc_def_var_endian));
    }

    /// <summary>
    /// Configures an HDF5 filter for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="filterId">The filter ID.</param>
    /// <param name="parameters">The filter parameters.</param>
    public void DefineVariableFilter(NetCdfHandle handle, VariableId variableId, uint filterId, IReadOnlyList<uint> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        int ncid = handle.Id;
        uint[] parms = parameters.ToArray();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, filterId={FilterId}, parameterCount={ParameterCount}", nameof(Native.nc_def_var_filter), ncid, variableId.Value, filterId, parms.Length);

        int status = Native.nc_def_var_filter(ncid, variableId.Value, filterId, (nuint)parms.Length, parms);
        LogReturned(nameof(Native.nc_def_var_filter), status);
        Check(status, nameof(Native.nc_def_var_filter));
    }

    /// <summary>
    /// Configures SZIP compression for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="optionsMask">The SZIP options mask.</param>
    /// <param name="pixelsPerBlock">The SZIP pixels-per-block value.</param>
    public void DefineVariableSzip(NetCdfHandle handle, VariableId variableId, int optionsMask, int pixelsPerBlock)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, optionsMask={OptionsMask}, pixelsPerBlock={PixelsPerBlock}", nameof(Native.nc_def_var_szip), ncid, variableId.Value, optionsMask, pixelsPerBlock);

        int status = Native.nc_def_var_szip(ncid, variableId.Value, optionsMask, pixelsPerBlock);
        LogReturned(nameof(Native.nc_def_var_szip), status);
        Check(status, nameof(Native.nc_def_var_szip));
    }

    /// <summary>
    /// Changes the parallel access mode for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="access">The requested parallel access mode.</param>
    public void SetVariableParallelAccess(NetCdfHandle handle, VariableId variableId, ParallelAccess access)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, access={Access}", nameof(Native.nc_var_par_access), ncid, variableId.Value, access);

        int status = Native.nc_var_par_access(ncid, variableId.Value, access);
        LogReturned(nameof(Native.nc_var_par_access), status);
        Check(status, nameof(Native.nc_var_par_access));
    }

    /// <summary>
    /// Renames a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="newName">The new variable name.</param>
    public void RenameVariable(NetCdfHandle handle, VariableId variableId, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, name={Name}", nameof(Native.nc_rename_var), ncid, variableId.Value, newName);

        int status = Native.nc_rename_var(ncid, variableId.Value, newName);
        LogReturned(nameof(Native.nc_rename_var), status);
        Check(status, nameof(Native.nc_rename_var));
    }

    /// <summary>
    /// Sets the per-variable chunk cache.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <param name="size">The cache size in bytes.</param>
    /// <param name="elementCount">The number of cache elements.</param>
    /// <param name="preemption">The cache preemption value.</param>
    public void SetVariableChunkCache(NetCdfHandle handle, VariableId variableId, nuint size, nuint elementCount, float preemption)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, size={Size}, elements={Elements}, preemption={Preemption}", nameof(Native.nc_set_var_chunk_cache), ncid, variableId.Value, size, elementCount, preemption);

        int status = Native.nc_set_var_chunk_cache(ncid, variableId.Value, size, elementCount, preemption);
        LogReturned(nameof(Native.nc_set_var_chunk_cache), status);
        Check(status, nameof(Native.nc_set_var_chunk_cache));
    }

    /// <summary>
    /// Inquires the per-variable chunk cache.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The variable chunk cache settings.</returns>
    public VariableChunkCacheInfo GetVariableChunkCache(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_get_var_chunk_cache), ncid, variableId.Value);

        int status = Native.nc_get_var_chunk_cache(ncid, variableId.Value, out nuint size, out nuint elements, out float preemption);
        LogReturned(nameof(Native.nc_get_var_chunk_cache), status);
        Check(status, nameof(Native.nc_get_var_chunk_cache));

        return new VariableChunkCacheInfo(size, elements, preemption);
    }

    /// <summary>
    /// Inquires a variable ID by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The variable name.</param>
    /// <returns>The matching variable ID.</returns>
    public VariableId InquireVariableId(NetCdfHandle handle, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}", nameof(Native.nc_inq_varid), ncid, name);

        int status = Native.nc_inq_varid(ncid, name, out int varid);
        LogReturned(nameof(Native.nc_inq_varid), status);
        Check(status, nameof(Native.nc_inq_varid));

        return new VariableId(varid);
    }

    /// <summary>
    /// Inquires variable metadata.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The variable metadata.</returns>
    public VariableInfo InquireVariable(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        int[] dimids = new int[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var), ncid, variableId.Value);

        int status = Native.nc_inq_var(ncid, variableId.Value, name, out NCType type, out int ndims, dimids, out int natts);
        LogReturned(nameof(Native.nc_inq_var), status);
        Check(status, nameof(Native.nc_inq_var));

        DimensionId[] dimensions = dimids.Take(ndims).Select(static id => new DimensionId(id)).ToArray();
        return new VariableInfo(NativeString.DecodeNullTerminatedUtf8(name), type, dimensions, natts);
    }

    /// <summary>
    /// Inquires a variable name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The variable name.</returns>
    public string InquireVariableName(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_varname), ncid, variableId.Value);

        int status = Native.nc_inq_varname(ncid, variableId.Value, name);
        LogReturned(nameof(Native.nc_inq_varname), status);
        Check(status, nameof(Native.nc_inq_varname));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires a variable data type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The netCDF external data type.</returns>
    public NCType InquireVariableType(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_vartype), ncid, variableId.Value);

        int status = Native.nc_inq_vartype(ncid, variableId.Value, out NCType type);
        LogReturned(nameof(Native.nc_inq_vartype), status);
        Check(status, nameof(Native.nc_inq_vartype));

        return type;
    }

    /// <summary>
    /// Inquires the number of dimensions used by a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The number of variable dimensions.</returns>
    public int InquireVariableDimensionCount(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_varndims), ncid, variableId.Value);

        int status = Native.nc_inq_varndims(ncid, variableId.Value, out int count);
        LogReturned(nameof(Native.nc_inq_varndims), status);
        Check(status, nameof(Native.nc_inq_varndims));

        return count;
    }

    /// <summary>
    /// Inquires the dimension IDs used by a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The dimension IDs used by the variable.</returns>
    public IReadOnlyList<DimensionId> InquireVariableDimensions(NetCdfHandle handle, VariableId variableId)
    {
        int count = InquireVariableDimensionCount(handle, variableId);
        int ncid = handle.Id;
        int[] dimids = new int[count];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_vardimid), ncid, variableId.Value);

        int status = Native.nc_inq_vardimid(ncid, variableId.Value, dimids);
        LogReturned(nameof(Native.nc_inq_vardimid), status);
        Check(status, nameof(Native.nc_inq_vardimid));

        return dimids.Select(static id => new DimensionId(id)).ToArray();
    }

    /// <summary>
    /// Inquires the number of attributes attached to a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The number of variable attributes.</returns>
    public int InquireVariableAttributeCount(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_varnatts), ncid, variableId.Value);

        int status = Native.nc_inq_varnatts(ncid, variableId.Value, out int count);
        LogReturned(nameof(Native.nc_inq_varnatts), status);
        Check(status, nameof(Native.nc_inq_varnatts));

        return count;
    }

    /// <summary>
    /// Inquires deflate settings for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The variable deflate settings.</returns>
    public VariableDeflateInfo InquireVariableDeflate(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var_deflate), ncid, variableId.Value);

        int status = Native.nc_inq_var_deflate(ncid, variableId.Value, out int shuffle, out int deflate, out int level);
        LogReturned(nameof(Native.nc_inq_var_deflate), status);
        Check(status, nameof(Native.nc_inq_var_deflate));

        return new VariableDeflateInfo(shuffle != 0, deflate != 0, level);
    }

    /// <summary>
    /// Inquires variable chunking settings.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The storage layout and chunk sizes.</returns>
    public unsafe (VariableStorage Storage, IReadOnlyList<nuint> ChunkSizes) InquireVariableChunking(NetCdfHandle handle, VariableId variableId)
    {
        int count = InquireVariableDimensionCount(handle, variableId);
        int ncid = handle.Id;
        nuint[] chunks = new nuint[count];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var_chunking), ncid, variableId.Value);

        fixed (nuint* chunkPtr = chunks)
        {
            int status = Native.nc_inq_var_chunking(ncid, variableId.Value, out int storage, chunkPtr);
            LogReturned(nameof(Native.nc_inq_var_chunking), status);
            Check(status, nameof(Native.nc_inq_var_chunking));

            return ((VariableStorage)storage, chunks);
        }
    }

    /// <summary>
    /// Inquires whether fill is disabled for a variable and reads the variable fill value.
    /// </summary>
    /// <typeparam name="T">The unmanaged fill value type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>Whether fill is disabled and the fill value reported by libnetcdf.</returns>
    public unsafe (bool NoFill, T FillValue) InquireVariableFill<T>(NetCdfHandle handle, VariableId variableId)
        where T : unmanaged
    {
        int ncid = handle.Id;
        T fillValue = default;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}, fillType={FillType}", nameof(Native.nc_inq_var_fill), ncid, variableId.Value, typeof(T).Name);

        int status = Native.nc_inq_var_fill(ncid, variableId.Value, out int noFill, &fillValue);
        LogReturned(nameof(Native.nc_inq_var_fill), status);
        Check(status, nameof(Native.nc_inq_var_fill));

        return (noFill != 0, fillValue);
    }

    /// <summary>
    /// Inquires whether fletcher32 checksums are enabled for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns><see langword="true"/> when fletcher32 is enabled; otherwise <see langword="false"/>.</returns>
    public bool InquireVariableFletcher32(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var_fletcher32), ncid, variableId.Value);

        int status = Native.nc_inq_var_fletcher32(ncid, variableId.Value, out int enabled);
        LogReturned(nameof(Native.nc_inq_var_fletcher32), status);
        Check(status, nameof(Native.nc_inq_var_fletcher32));

        return enabled != 0;
    }

    /// <summary>
    /// Inquires endian storage for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The endian storage setting.</returns>
    public VariableEndian InquireVariableEndian(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var_endian), ncid, variableId.Value);

        int status = Native.nc_inq_var_endian(ncid, variableId.Value, out int endian);
        LogReturned(nameof(Native.nc_inq_var_endian), status);
        Check(status, nameof(Native.nc_inq_var_endian));

        return (VariableEndian)endian;
    }

    /// <summary>
    /// Inquires SZIP settings for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The options mask and pixels-per-block values.</returns>
    public (int OptionsMask, int PixelsPerBlock) InquireVariableSzip(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var_szip), ncid, variableId.Value);

        int status = Native.nc_inq_var_szip(ncid, variableId.Value, out int options, out int pixels);
        LogReturned(nameof(Native.nc_inq_var_szip), status);
        Check(status, nameof(Native.nc_inq_var_szip));

        return (options, pixels);
    }

    /// <summary>
    /// Inquires the number of variables in an open netCDF file.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <returns>The number of variables.</returns>
    public int InquireVariableCount(NetCdfHandle handle)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}", nameof(Native.nc_inq_nvars), ncid);

        int status = Native.nc_inq_nvars(ncid, out int count);
        LogReturned(nameof(Native.nc_inq_nvars), status);
        Check(status, nameof(Native.nc_inq_nvars));

        return count;
    }

    /// <summary>
    /// Inquires HDF5 filter settings for a variable.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="variableId">The variable ID.</param>
    /// <returns>The filter ID and parameters.</returns>
    public (uint FilterId, IReadOnlyList<uint> Parameters) InquireVariableFilter(NetCdfHandle handle, VariableId variableId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, varid={VariableId}", nameof(Native.nc_inq_var_filter), ncid, variableId.Value);

        int status = Native.nc_inq_var_filter(ncid, variableId.Value, out uint id, out nuint nparams, null);
        LogReturned(nameof(Native.nc_inq_var_filter), status);
        Check(status, nameof(Native.nc_inq_var_filter));

        uint[] parameters = new uint[nparams];
        status = Native.nc_inq_var_filter(ncid, variableId.Value, out id, out _, parameters);
        LogReturned(nameof(Native.nc_inq_var_filter), status);
        Check(status, nameof(Native.nc_inq_var_filter));

        return (id, parameters);
    }

    /// <summary>
    /// Copies a variable definition and data to another open netCDF file.
    /// </summary>
    /// <param name="source">The source open netCDF file handle.</param>
    /// <param name="variableId">The source variable ID.</param>
    /// <param name="destination">The destination open netCDF file handle.</param>
    public void CopyVariable(NetCdfHandle source, VariableId variableId, NetCdfHandle destination)
    {
        int sourceNcid = source.Id;
        int destinationNcid = destination.Id;
        logger?.LogDebug("{FunctionName}: sourceNcid={SourceNcid}, varid={VariableId}, destinationNcid={DestinationNcid}", nameof(Native.nc_copy_var), sourceNcid, variableId.Value, destinationNcid);

        int status = Native.nc_copy_var(sourceNcid, variableId.Value, destinationNcid);
        LogReturned(nameof(Native.nc_copy_var), status);
        Check(status, nameof(Native.nc_copy_var));
    }
}
