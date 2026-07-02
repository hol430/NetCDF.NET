using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using MPI_Comm = System.IntPtr;
using MPI_Info = System.IntPtr;

namespace NetCDF.Interop;

/// <summary>
/// P/Invoke signatures for the native netCDF library.
/// </summary>
/// <remarks>
/// Some functions are omitted:
///  nc_close_memio
///  nc_create_par_fortran
///  nc_def_user_format
///  nc_int_user_format
///  nc_open_mem
///  nc_open_memio
///  nc_open_par_fortran
///  nc_get_var
///  and all deprecated nc_get_varm functions
///
/// User defined, Compound, Enum and VLen functions have not yet been tested and
/// the functions required for VLen are incomplete. e.g. the VLen struct is not
/// defined here There is also a macro defined for VLen, which we do not have :
/// #define NC_COMPOUND_OFFSET(S,M)    (offsetof(S,M))
/// </remarks>
public static partial class Native
{
    private const string library = "netcdf";

    [StructLayout(LayoutKind.Sequential)]
    public struct NcVlen
    {
        public nuint len;
        public IntPtr p;
    }

    /// <summary>Return the library version string</summary>
    /// <remarks>
    /// Methods returning const char * require the custom Marshaller.
    /// </remarks>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint nc_inq_libvers();

    /// <summary>Return the error message</summary>
    /// <param name="ncerr">The netCDF error code.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial nint nc_strerror(int ncerr);

    /// <summary>Provided for completeness - No longer necessary for user to invoke manually.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_abort(int ncid);

    /// <summary>Close an open netCDF file.</summary>
    /// <param name="ncid">The ID of the file to close.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_close(int ncid);

    /// <summary>Create a new netCDF file.</summary>
    /// <param name="path">The path to the file to create.</param>
    /// <param name="mode">The mode to create the file in.</param>
    /// <param name="ncidp">The ID of the newly created file.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_create(string path, CreateMode mode, out int ncidp);

    /// <summary>Create a netCDF file with the contents stored in memory.</summary>
    /// <param name="path">The filesystem path.</param>
    /// <param name="mode">The mode value.</param>
    /// <param name="initialsize">The initial in-memory file size.</param>
    /// <param name="ncidp">Receives the netCDF file or dataset ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_create_mem(string path, CreateMode mode, nuint initialsize, out int ncidp);

    /// <summary>Leave define mode</summary>
    /// <param name="ncidp">The netCDF file or dataset ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_enddef(int ncidp);

    /// <summary>Inquire about a file or group.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="ndims">The number of dimensions.</param>
    /// <param name="nvars">Receives the number of variables.</param>
    /// <param name="ngatts">Receives the number of global attributes.</param>
    /// <param name="unlimdimid">Receives the unlimited dimension ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq(int ncid, out int ndims, out int nvars, out int ngatts, out int unlimdimid);

    /// <summary>Inquire about the binary format of a netCDF file as presented by the API.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="format">The format value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_format(int ncid, out int format);

    /// <summary>Obtain more detailed (vis-a-vis nc_inq_format) format information about an open dataset.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="format">The format value.</param>
    /// <param name="mode">The mode value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_format_extended(int ncid, out int format, out int mode);

    /// <summary>Learn the path used to open/create the file.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="pathlen">Receives the path length.</param>
    /// <param name="path">The filesystem path.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_path(int ncid, out nuint pathlen, [In, Out] byte[]? path);

    /// <summary>Inquire about a type.</summary>
    /// <remarks>
    /// Given an ncid and a typeid, get the information about a type. This
    /// function will work on any type, including atomic and any user defined
    /// type, whether compound, opaque, enumeration, or variable length array.
    /// For even more information about a user defined type nc_inq_user_type().
    /// </remarks>
    /// <param name="ncid">The ncid for the group containing the type (ignored
    /// for atomic types).</param>
    /// <param name="type">The typeid for this type, as returned by
    /// nc_def_compound, nc_def_opaque, nc_def_enum, nc_def_vlen, or nc_inq_var,
    /// or as found in netcdf.h in the list of atomic types (NC_CHAR, NC_INT,
    /// etc.).</param>
    /// <param name="name">If non-NULL, the name of the user defined type will
    /// be copied here. It will be NC_MAX_NAME bytes or less. For atomic types,
    /// the type name from CDL will be given.</param>
    /// <param name="size">If non-NULL, the (in-memory) size of the type in
    /// bytes will be copied here. VLEN type size is the size of nc_vlen_t.
    /// String size is returned as the size of a character pointer. The size may
    /// be used to malloc space for the data, no matter what the type.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_type(int ncid, NCType type, [In, Out] byte[]? name, out nuint size);

    /// <summary>Open an existing netCDF file.</summary>
    /// <param name="path">The filesystem path.</param>
    /// <param name="mode">The mode value.</param>
    /// <param name="ncidp">Receives the netCDF file or dataset ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_open(string path, OpenMode mode, out int ncidp);

    /// <summary>Put open netcdf dataset into define mode</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_redef(int ncid);

    /// <summary>Set the fill mode (classic or 64-bit offset files only).</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="fillmode">The requested fill mode.</param>
    /// <param name="old_modep">Receives the previous fill mode.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_set_fill(int ncid, int fillmode, out int old_modep);

    /// <summary>Synchronize an open netcdf dataset to disk</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_sync(int ncid);

    /// <summary>Open an existing netCDF file for parallel I/O.</summary>
    /// <param name="path">The filesystem path.</param>
    /// <param name="omode">The open mode.</param>
    /// <param name="comm">The MPI communicator.</param>
    /// <param name="info">The MPI info object.</param>
    /// <param name="ncidp">Receives the netCDF file or dataset ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_open_par(string path, OpenMode omode, MPI_Comm comm, MPI_Info info, out int ncidp);

    /// <summary>Create a netCDF file for parallel I/O.</summary>
    /// <param name="path">The filesystem path.</param>
    /// <param name="cmode">The create mode.</param>
    /// <param name="comm">The MPI communicator.</param>
    /// <param name="info">The MPI info object.</param>
    /// <param name="ncidp">Receives the netCDF file or dataset ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_create_par(string path, CreateMode cmode, MPI_Comm comm, MPI_Info info, out int ncidp);

    /// <summary>
    /// Change the parallel access of a variable from independent to collective
    /// and vice versa.
    /// </summary>
    /// <remarks>
    /// This function will change the parallel access of a variable from
    /// independent to collective and vice versa.
    ///
    /// This function is collective, i.e. must be called by all MPI processes
    /// defined in the MPI communicator used in nc_create_par() or
    /// nc_open_par(). In addition, values of arguments of this function must be
    /// the same among all MPI processes.
    ///
    /// To obtain a good I/O performance, users are recommended to use
    /// collective mode. In addition, switching between collective and
    /// independent I/O mode can be expensive.
    ///
    /// In netcdf-c-4.7.4 or later, using hdf5-1.10.2 or later, the zlib, szip,
    /// fletcher32, and other filters may be used when writing data with
    /// parallel I/O. The use of these filters require collective access.
    /// Turning on the zlib (deflate) or fletcher32 filter for a variable will
    /// automatically set its access to collective if the file has been opened
    /// for parallel I/O. Attempts to set access to independent will return
    /// NC_EINVAL.
    ///
    /// Note When the library is built with --enable-pnetcdf, and when file is
    /// opened/created to use PnetCDF library to perform parallel I/O
    /// underneath, argument varid is ignored and the mode changed by this
    /// function applies to all variables. This is because PnetCDF does not
    /// support access mode change for individual variables. In this case, users
    /// may use NC_GLOBAL in varid argument for better program readability.
    /// </remarks>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="par_access">The parallel access mode.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_var_par_access(int ncid, int varid, ParallelAccess par_access);

    /// <summary>Define a new dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The dimension name.</param>
    /// <param name="len">The length value.</param>
    /// <param name="dimidp">Receives the dimension ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_dim(int ncid, string name, nuint len, out int dimidp);

    /// <summary>Find the name and length of a dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="dimid">The dimension ID.</param>
    /// <param name="name">The dimension name.</param>
    /// <param name="len">The length value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_dim(int ncid, int dimid, [Out] byte[]? name, out nuint len);

    /// <summary>Find the ID of a dimension from the name.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The dimension name.</param>
    /// <param name="dimid">The dimension ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_dimid(int ncid, string name, out int dimid);

    /// <summary>Find the length of a dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="dimid">The dimension ID.</param>
    /// <param name="len">The length value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_dimlen(int ncid, int dimid, out nuint len);

    /// <summary>Find out the name of a dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="dimid">The dimension ID.</param>
    /// <param name="name">The dimension name.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_dimname(int ncid, int dimid, [Out] byte[]? name);

    /// <summary>Find the number of dimensions.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="ndims">The number of dimensions.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_ndims(int ncid, out int ndims);

    /// <summary>Find the ID of the unlimited dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="unlimdimid">Receives the unlimited dimension ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_unlimdim(int ncid, out int unlimdimid);

    /// <summary>Find the ID of the unlimited dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="nunlimdimsp">Receives the number of unlimited dimensions.</param>
    /// <param name="unlimdimidsp">Receives the unlimited dimension IDs.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_unlimdims(int ncid, out int nunlimdimsp, [Out] int[]? unlimdimidsp);

    /// <summary>Rename a dimension.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="dimid">The dimension ID.</param>
    /// <param name="name">The dimension name.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_rename_dim(int ncid, int dimid, string name);

    /// <summary>Define a variable</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="ndims">The number of dimensions.</param>
    /// <param name="dimids">The dimension IDs.</param>
    /// <param name="varidp">Receives the variable ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var(int ncid, string name, NCType xtype, int ndims, [In] int[] dimids, out int varidp);

    /// <summary>Define fill value behavior for a variable. This must be done after nc_def_var</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="no_fill">The no-fill flag.</param>
    /// <param name="fill_value">The fill value pointer.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int nc_def_var_fill(int ncid, int varid, int no_fill, void *fill_value);

    /// <summary>Set compression settings for a variable. Lower is faster, higher is better.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="shuffle">The shuffle filter flag.</param>
    /// <param name="deflate">The deflate filter flag.</param>
    /// <param name="deflate_level">The deflate compression level.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var_deflate(int ncid, int varid, int shuffle, int deflate, int deflate_level);

    /// <summary>Set fletcher32 checksum for a var. This must be done after nc_def_var</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="fletcher32">The fletcher32 checksum flag.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var_fletcher32(int ncid, int varid, int fletcher32);

    /// <summary>Define chunking for a variable. This must be done after nc_def_var</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="storage">The chunk storage mode.</param>
    /// <param name="chunksizes">The chunk sizes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var_chunking(int ncid, int varid, int storage, [In] nuint[]? chunksizes);

    /// <summary>Define endianness of a variable.
    /// NC_ENDIAN_NATIVE to select the native endianness of the platform (the default), NC_ENDIAN_LITTLE to use little-endian, NC_ENDIAN_BIG to use big-endian
    /// </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="endian">The endian setting.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var_endian(int ncid, int varid, int endian);

    /// <summary>Define a filter for a variable</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="id">The filter ID.</param>
    /// <param name="nparams">The number of filter parameters.</param>
    /// <param name="parms">The filter parameters.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var_filter(int ncid, int varid, uint id, nuint nparams, [In] uint[] parms);

    /// <summary>Set szip compression settings on a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="options_maskp">The szip options mask.</param>
    /// <param name="pixels_per_blockp">The szip pixels-per-block value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_var_szip(int ncid, int varid, int options_maskp, int pixels_per_blockp);

    /// <summary>Rename a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The variable name.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_rename_var(int ncid, int varid, string name);

    /// <summary>Use this function to free resources associated with NC_STRING data.</summary>
    /// <param name="len">The length value.</param>
    /// <param name="data">The data buffer or pointer.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_free_string(nuint len, [In] IntPtr[] data);

    /// <summary>Set the per-variable cache size, nelems, and preemption policy. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="size">The size value.</param>
    /// <param name="nelems">The number of cache elements.</param>
    /// <param name="preemption">The cache preemption policy.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_set_var_chunk_cache(int ncid, int varid, nuint size, nuint nelems, float preemption);

    /// <summary>Get the per-variable cache size, nelems, and preemption policy.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="size">The size value.</param>
    /// <param name="nelemsp">Receives the number of cache elements.</param>
    /// <param name="preemptionp">Receives the cache preemption policy.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_chunk_cache(int ncid, int varid, out nuint size, out nuint nelemsp, out float preemptionp);

    /// <summary>Read all values from a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_text(int ncid, int varid, [Out] byte[] ip);

    /// <summary>Read all values from a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_schar(int ncid, int varid, [Out] sbyte[] ip);

    /// <summary>Read all values from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_uchar(int ncid, int varid, [Out] byte[] ip);

    /// <summary>Read all values from a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_short(int ncid, int varid, [Out] short[] ip);

    /// <summary>Read all values from a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_int(int ncid, int varid, [Out] int[] ip);

    /// <summary>Read all values from a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_float(int ncid, int varid, [Out] float[] ip);

    /// <summary>Read all values from a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_double(int ncid, int varid, [Out] double[] ip);

    /// <summary>Read all values from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_ubyte(int ncid, int varid, [Out] byte[] ip);

    /// <summary>Read all values from a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_ushort(int ncid, int varid, [Out] ushort[] ip);

    /// <summary>Read all values from a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_uint(int ncid, int varid, [Out] uint[] ip);

    /// <summary>Read all values from a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_longlong(int ncid, int varid, [Out] long[] ip);

    /// <summary>Read all values from a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_ulonglong(int ncid, int varid, [Out] ulong[] ip);

    /// <summary>Read all values from a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var_string(int ncid, int varid, [Out] IntPtr[] ip);

    /// <summary>Read one value from a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_text(int ncid, int varid, [In] nuint[] index, out byte ip);

    /// <summary>Read one value from a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_schar(int ncid, int varid, [In] nuint[] index, out sbyte ip);

    /// <summary>Read one value from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_uchar(int ncid, int varid, [In] nuint[] index, out byte ip);

    /// <summary>Read one value from a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_short(int ncid, int varid, [In] nuint[] index, out short ip);

    /// <summary>Read one value from a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_int(int ncid, int varid, [In] nuint[] index, out int ip);

    /// <summary>Read one value from a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_float(int ncid, int varid, [In] nuint[] index, out float ip);

    /// <summary>Read one value from a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_double(int ncid, int varid, [In] nuint[] index, out double ip);

    /// <summary>Read one value from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_ubyte(int ncid, int varid, [In] nuint[] index, out byte ip);

    /// <summary>Read one value from a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_ushort(int ncid, int varid, [In] nuint[] index, out ushort ip);

    /// <summary>Read one value from a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_uint(int ncid, int varid, [In] nuint[] index, out uint ip);

    /// <summary>Read one value from a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_longlong(int ncid, int varid, [In] nuint[] index, out long ip);

    /// <summary>Read one value from a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_ulonglong(int ncid, int varid, [In] nuint[] index, out ulong ip);

    /// <summary>Read one value from a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_var1_string(int ncid, int varid, [In] nuint[] index, [Out] IntPtr[] ip);

    /// <summary>Read an array section from a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_text(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] byte[] ip);

    /// <summary>Read an array section from a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_schar(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] sbyte[] ip);

    /// <summary>Read an array section from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_uchar(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] byte[] ip);

    /// <summary>Read an array section from a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_short(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] short[] ip);

    /// <summary>Read an array section from a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_int(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] int[] ip);

    /// <summary>Read an array section from a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_float(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] float[] ip);

    /// <summary>Read an array section from a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_double(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] double[] ip);

    /// <summary>Read an array section from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_ubyte(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] byte[] ip);

    /// <summary>Read an array section from a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_ushort(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] ushort[] ip);

    /// <summary>Read an array section from a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_uint(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] uint[] ip);

    /// <summary>Read an array section from a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_longlong(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] long[] ip);

    /// <summary>Read an array section from a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_ulonglong(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] ulong[] ip);

    /// <summary>Read an array section from a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vara_string(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] IntPtr[] ip);

    /// <summary>Read a strided array section from a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_text(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] byte[] ip);

    /// <summary>Read a strided array section from a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_uchar(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] byte[] ip);

    /// <summary>Read a strided array section from a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_schar(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] sbyte[] ip);

    /// <summary>Read a strided array section from a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_short(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] short[] ip);

    /// <summary>Read a strided array section from a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_int(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] int[] ip);

    /// <summary>Read a strided array section from a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_float(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] float[] ip);

    /// <summary>Read a strided array section from a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_double(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] double[] ip);

    /// <summary>Read a strided array section from a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_ushort(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] ushort[] ip);

    /// <summary>Read a strided array section from a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_uint(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] uint[] ip);

    /// <summary>Read a strided array section from a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_longlong(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] long[] ip);

    /// <summary>Read a strided array section from a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_ulonglong(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] ulong[] ip);

    /// <summary>Read a strided array section from a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vars_string(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] IntPtr[] ip);

    /// <summary>Find the ID of a variable from its name.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="varidp">Receives the variable ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_varid(int ncid, string name, out int varidp);

    /// <summary>Inquire about a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="ndims">The number of dimensions.</param>
    /// <param name="dimids">The dimension IDs.</param>
    /// <param name="natts">Receives the number of attributes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_var(int ncid, int varid, [Out] byte[]? name, out NCType type, out int ndims, [Out] int[] dimids, out int natts);

    /// <summary>Find the name of a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The variable name.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_varname(int ncid, int varid, [Out] byte[]? name);

    /// <summary>Find the type of a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="xtypep">Receives the netCDF type ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_vartype(int ncid, int varid, out NCType xtypep);

    /// <summary>Find the number of dimensions for a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ndims">The number of dimensions.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_varndims(int ncid, int varid, out int ndims);

    /// <summary>Find the dimension IDs for a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="dimids">The dimension IDs.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_vardimid(int ncid, int varid, [Out] int[] dimids);

    /// <summary>Find the number of attributes for a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="nattsp">Receives the number of attributes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_varnatts(int ncid, int varid, out int nattsp);

    /// <summary>Find out compression settings of a var.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="shufflep">Receives the shuffle filter flag.</param>
    /// <param name="deflatep">Receives the deflate filter flag.</param>
    /// <param name="deflate_levelp">Receives the deflate compression level.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_var_deflate(int ncid, int varid, out int shufflep, out int deflatep, out int deflate_levelp);


    /// <summary>Inquire about fletcher32 checksum for a var.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="fletcher32p">Receives the fletcher32 checksum flag.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_var_fletcher32(int ncid, int varid, out int fletcher32p);

    /// <summary>Inq chunking stuff for a var.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="storagep">Receives the chunk storage mode.</param>
    /// <param name="chunksizesp">Receives the chunk sizes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int nc_inq_var_chunking(int ncid, int varid, out int storagep, nuint* chunksizesp);

    /// <summary>Inq fill value setting for a var.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="no_fill">The no-fill flag.</param>
    /// <param name="fill_valuep">Receives the fill value pointer.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial int nc_inq_var_fill(int ncid, int varid, out int no_fill, void *fill_valuep);

    /// <summary>Learn about the endianness of a variable.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="endianp">Receives the endian setting.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_var_endian(int ncid, int varid, out int endianp);

    /// <summary>Find out szip settings of a var.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="options_maskp">The szip options mask.</param>
    /// <param name="pixels_per_blockp">The szip pixels-per-block value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_var_szip(int ncid, int varid, out int options_maskp, out int pixels_per_blockp);

    /// <summary>Find the number of variables.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="nvars">Receives the number of variables.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_nvars(int ncid, out int nvars);

    /// <summary>Learn about the filter on a variable</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="idp">Receives the filter or attribute ID.</param>
    /// <param name="nparams">The number of filter parameters.</param>
    /// <param name="parms">The filter parameters.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_var_filter(int ncid, int varid, out uint idp, out nuint nparams, [Out] uint[]? parms);

    /// <summary>Write all values to a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_text(int ncid, int varid, [In] byte[] op);

    /// <summary>Write all values to a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_schar(int ncid, int varid, [In] sbyte[] op);

    /// <summary>Write all values to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_uchar(int ncid, int varid, [In] byte[] op);

    /// <summary>Write all values to a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_short(int ncid, int varid, [In] short[] op);

    /// <summary>Write all values to a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_int(int ncid, int varid, [In] int[] op);

    /// <summary>Write all values to a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_float(int ncid, int varid, [In] float[] op);

    /// <summary>Write all values to a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_double(int ncid, int varid, [In] double[] op);

    /// <summary>Write all values to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_ubyte(int ncid, int varid, [In] byte[] op);

    /// <summary>Write all values to a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_ushort(int ncid, int varid, [In] ushort[] op);

    /// <summary>Write all values to a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_uint(int ncid, int varid, [In] uint[] op);

    /// <summary>Write all values to a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_longlong(int ncid, int varid, [In] long[] op);

    /// <summary>Write all values to a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_ulonglong(int ncid, int varid, [In] ulong[] op);

    /// <summary>Write all values to a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var_string(int ncid, int varid, [In] string[] op);

    /// <summary>Write one value to a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_text(int ncid, int varid, [In] nuint[] index, ref byte op);

    /// <summary>Write one value to a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_schar(int ncid, int varid, [In] nuint[] index, ref sbyte op);

    /// <summary>Write one value to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_uchar(int ncid, int varid, [In] nuint[] index, ref byte op);

    /// <summary>Write one value to a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_short(int ncid, int varid, [In] nuint[] index, ref short op);

    /// <summary>Write one value to a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_int(int ncid, int varid, [In] nuint[] index, ref int op);

    /// <summary>Write one value to a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_float(int ncid, int varid, [In] nuint[] index, ref float op);

    /// <summary>Write one value to a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_double(int ncid, int varid, [In] nuint[] index, ref double op);

    /// <summary>Write one value to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_ubyte(int ncid, int varid, [In] nuint[] index, ref byte op);

    /// <summary>Write one value to a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_ushort(int ncid, int varid, [In] nuint[] index, ref ushort op);

    /// <summary>Write one value to a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_uint(int ncid, int varid, [In] nuint[] index, ref uint op);

    /// <summary>Write one value to a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_longlong(int ncid, int varid, [In] nuint[] index, ref long op);

    /// <summary>Write one value to a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_ulonglong(int ncid, int varid, [In] nuint[] index, ref ulong op);

    /// <summary>Write one value to a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="index">The element index.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_var1_string(int ncid, int varid, [In] nuint[] index, [In] string[] op);

    /// <summary>Write an array section to a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_text(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] byte[] op);

    /// <summary>Write an array section to a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_schar(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] sbyte[] op);

    /// <summary>Write an array section to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_uchar(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] byte[] op);

    /// <summary>Write an array section to a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_short(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] short[] op);

    /// <summary>Write an array section to a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_int(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] int[] op);

    /// <summary>Write an array section to a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_float(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] float[] op);

    /// <summary>Write an array section to a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_double(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] double[] op);

    /// <summary>Write an array section to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_ubyte(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] byte[] op);

    /// <summary>Write an array section to a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_ushort(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] ushort[] op);

    /// <summary>Write an array section to a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_uint(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] uint[] op);

    /// <summary>Write an array section to a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_longlong(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] long[] op);

    /// <summary>Write an array section to a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_ulonglong(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] ulong[] op);

    /// <summary>Write an array section to a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="start">The starting indices.</param>
    /// <param name="count">The element counts.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vara_string(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] string[] op);

    /// <summary>Write a strided array section to a variable as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_text(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] byte[] op);

    /// <summary>Write a strided array section to a variable as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_uchar(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] byte[] op);

    /// <summary>Write a strided array section to a variable as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_schar(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] sbyte[] op);

    /// <summary>Write a strided array section to a variable as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_short(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] short[] op);

    /// <summary>Write a strided array section to a variable as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_int(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] int[] op);

    /// <summary>Write a strided array section to a variable as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_float(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] float[] op);

    /// <summary>Write a strided array section to a variable as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_double(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] double[] op);

    /// <summary>Write a strided array section to a variable as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_ushort(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] ushort[] op);

    /// <summary>Write a strided array section to a variable as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_uint(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] uint[] op);

    /// <summary>Write a strided array section to a variable as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_longlong(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] long[] op);

    /// <summary>Write a strided array section to a variable as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_ulonglong(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] ulong[] op);

    /// <summary>Write a strided array section to a variable as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="startp">The starting indices.</param>
    /// <param name="countp">The element counts.</param>
    /// <param name="stridep">The strides.</param>
    /// <param name="op">The source buffer or value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vars_string(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] string[] op);

    /// <summary>Copy a variable from one open dataset to another.</summary>
    /// <param name="ncid_in">The source netCDF file or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="ncid_out">The destination netCDF file or dataset ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_copy_var(int ncid_in, int varid, int ncid_out);

    /// <summary>Inquire about an attribute.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="xtypep">Receives the netCDF type ID.</param>
    /// <param name="lenp">Receives the length value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_att(int ncid, int varid, string name, out NCType xtypep, out nuint lenp);

    /// <summary>Find the ID of an attribute from its name.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="idp">Receives the filter or attribute ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_attid(int ncid, int varid, string name, out int idp);

    /// <summary>Find the name of an attribute from its number.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="attnum">The attribute number.</param>
    /// <param name="name">The attribute name.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_attname(int ncid, int varid, int attnum, [Out] byte[]? name);

    /// <summary>Find the number of global attributes.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="ngatts">Receives the number of global attributes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_natts(int ncid, out int ngatts);

    /// <summary>Find the type of an attribute.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="xtypep">Receives the netCDF type ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_atttype(int ncid, int varid, string name, out NCType xtypep);

    /// <summary>Find the length of an attribute.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="lenp">Receives the length value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_attlen(int ncid, int varid, string name, out nuint lenp);

    /// <summary>Read an attribute as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="data">The data buffer or pointer.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_text(int ncid, int varid, string name, [Out] byte[] data);

    /// <summary>Read an attribute as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_schar(int ncid, int varid, string name, [Out] sbyte[] value);

    /// <summary>Read an attribute as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_uchar(int ncid, int varid, string name, [Out] byte[] value);

    /// <summary>Read an attribute as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_short(int ncid, int varid, string name, [Out] short[] value);

    /// <summary>Read an attribute as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_int(int ncid, int varid, string name, [Out] int[] value);

    /// <summary>Read an attribute as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_float(int ncid, int varid, string name, [Out] float[] value);

    /// <summary>Read an attribute as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_double(int ncid, int varid, string name, [Out] double[] value);

    /// <summary>Read an attribute as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_ubyte(int ncid, int varid, string name, [Out] byte[] value);

    /// <summary>Read an attribute as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_ushort(int ncid, int varid, string name, [Out] ushort[] value);

    /// <summary>Read an attribute as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_uint(int ncid, int varid, string name, [Out] uint[] value);

    /// <summary>Read an attribute as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_longlong(int ncid, int varid, string name, [Out] long[] value);

    /// <summary>Read an attribute as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_ulonglong(int ncid, int varid, string name, [Out] ulong[] value);

    /// <summary>Write an attribute as text data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_text(int ncid, int varid, string name, nuint len, string value);

    /// <summary>Write an attribute as signed byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_schar(int ncid, int varid, string name, NCType type, nuint len, [In] sbyte[] value);

    /// <summary>Write an attribute as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_uchar(int ncid, int varid, string name, NCType type, nuint len, [In] byte[] value);

    /// <summary>Write an attribute as short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_short(int ncid, int varid, string name, NCType type, nuint len, [In] short[] value);

    /// <summary>Write an attribute as int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_int(int ncid, int varid, string name, NCType type, nuint len, [In] int[] value);

    /// <summary>Write an attribute as float data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_float(int ncid, int varid, string name, NCType type, nuint len, [In] float[] value);

    /// <summary>Write an attribute as double data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_double(int ncid, int varid, string name, NCType type, nuint len, [In] double[] value);

    /// <summary>Write an attribute as unsigned byte data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_ubyte(int ncid, int varid, string name, NCType type, nuint len, [In] byte[] value);

    /// <summary>Write an attribute as unsigned short data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_ushort(int ncid, int varid, string name, NCType type, nuint len, [In] ushort[] value);

    /// <summary>Write an attribute as unsigned int data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_uint(int ncid, int varid, string name, NCType type, nuint len, [In] uint[] value);

    /// <summary>Write an attribute as long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_longlong(int ncid, int varid, string name, NCType type, nuint len, [In] long[] value);

    /// <summary>Write an attribute as unsigned long long data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="type">The netCDF type ID.</param>
    /// <param name="len">The length value.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_ulonglong(int ncid, int varid, string name, NCType type, nuint len, [In] ulong[] value);

    /// <summary>Write an attribute as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="len">The length value.</param>
    /// <param name="tp">The source string values.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_att_string(int ncid, int varid, string name, nuint len, [In] string[] tp);

    /// <summary>Copy an attribute from one variable or dataset to another.</summary>
    /// <param name="ncid_in">The source netCDF file or dataset ID.</param>
    /// <param name="varid_in">The source variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="ncid_out">The destination netCDF file or dataset ID.</param>
    /// <param name="varid_out">The destination variable ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_copy_att(int ncid_in, int varid_in, string name, int ncid_out, int varid_out);

    /// <summary>Delete an attribute.</summary>
    /// <param name="ncid_in">The source netCDF file or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_del_att(int ncid_in, int varid, string name);

    /// <summary>Rename an attribute.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="newname">The new name.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_rename_att(int ncid, int varid, string name, string newname);

    /// <summary>Define a new group.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The group name.</param>
    /// <param name="grp_ncid">Receives the group ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_grp(int ncid, string name, out int grp_ncid);

    /// <summary>Retrieve a list of dimension ids associated with a group</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="ndims">The number of dimensions.</param>
    /// <param name="dimids">The dimension IDs.</param>
    /// <param name="include_parents">Whether to include parent dimensions.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_dimids(int ncid, out int ndims, [Out] int[] dimids, int include_parents);

    /// <summary>Given a full name and ncid, find group ncid.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="full_name">The full group name.</param>
    /// <param name="grp_ncid">Receives the group ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grp_full_ncid(int ncid, string full_name, out int grp_ncid);

    /// <summary>Given a name and parent ncid, find group ncid.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="grp_name">The group name.</param>
    /// <param name="grp_ncid">Receives the group ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grp_ncid(int ncid, string grp_name, out int grp_ncid);

    /// <summary>Given an ncid, find the ncid of its parent group.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="parent_ncid">Receives the parent group ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grp_parent(int ncid, out int parent_ncid);

    /// <summary>Given locid, find name of group. (Root group is named "/".) </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The group name.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grpname(int ncid, [Out] byte[] name);

    /// <summary>
    /// Given ncid, find full name and len of full name. (Root group is named "/", with length 1.) 
    /// But use the C# friendlier nc_inq_grpname_full(ncid) instead
    /// </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="lenp">Receives the length value.</param>
    /// <param name="full_name">The full group name buffer.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grpname_full(int ncid, out nuint lenp, [Out] byte[] full_name);

    /// <summary>Given ncid, find len of full name. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="lenp">Receives the length value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grpname_len(int ncid, out nuint lenp);

    /// <summary>Given a location id, return the number of groups it contains, and an array of their locids.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="numgrps">Receives the number of groups.</param>
    /// <param name="ncids">Receives the group IDs.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_grps(int ncid, out int numgrps, [Out] int[]? ncids);

    /// <summary>Given an ncid and group name (NULL gets root group), return locid. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The group name.</param>
    /// <param name="grp_ncid">Receives the group ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_ncid(int ncid, string name, out int grp_ncid);

    /// <summary>Retrieve a list of types associated with a group.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="ntypes">Receives the number of types.</param>
    /// <param name="typeids">Receives the type IDs.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_typeids(int ncid, out int ntypes, [Out] int[] typeids);

    /// <summary>Get a list of varids associated with a group given a group ID.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="nvars">Receives the number of variables.</param>
    /// <param name="varids">Receives the varids value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_varids(int ncid, out int nvars, [Out] int[] varids);

    /// <summary>Rename a group.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The group name.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_rename_grp(int ncid, string name);

    /// <summary>Print the metadata for a file.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_show_metadata(int ncid);

    /// <summary> Are two types equal? </summary>
    /// <param name="ncid1">The ncid1 value.</param>
    /// <param name="typeid1">The netCDF type ID.</param>
    /// <param name="ncid2">The ncid2 value.</param>
    /// <param name="typeid2">The second netCDF type ID.</param>
    /// <param name="equal">Receives whether the two types are equal.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_type_equal(int ncid1, NCType typeid1, int ncid2, NCType typeid2, out int equal);

    /// <summary> Get the id of a type from the name. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The type name.</param>
    /// <param name="typeidp">Receives the netCDF type ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_typeid(int ncid, string name, out NCType typeidp);

    /// <summary> Find out about a user defined type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The type name.</param>
    /// <param name="size">The size value.</param>
    /// <param name="base_NCTypep">Receives the base type ID.</param>
    /// <param name="nfieldsp">Receives the number of fields.</param>
    /// <param name="classp">Receives the user-defined type class.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_user_type(int ncid, NCType xtype, [Out] byte[] name, out nuint size, out NCType base_NCTypep, out nuint nfieldsp, out int classp);

    /// <summary> Here are functions for dealing with compound types.  Create a compound type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="size">The size value.</param>
    /// <param name="name">The compound type or field name.</param>
    /// <param name="typeidp">Receives the netCDF type ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_compound(int ncid, nuint size, string name, out NCType typeidp);

    /// <summary> Insert a named field into a compound type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The compound type or field name.</param>
    /// <param name="offset">The field offset.</param>
    /// <param name="field_typeid">The type ID of the compound field.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_insert_compound(int ncid, NCType xtype, string name, nuint offset, NCType field_typeid);

    /// <summary>Insert a named array into a compound type (array dims form).</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The compound type or field name.</param>
    /// <param name="offset">The field offset.</param>
    /// <param name="field_typeid">The type ID of the compound field.</param>
    /// <param name="ndims">The number of dimensions.</param>
    /// <param name="dim_sizes">The dimension sizes.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_insert_array_compound(int ncid, NCType xtype, string name, nuint offset, NCType field_typeid, int ndims, int[] dim_sizes);

    /// <summary> Get the name, size, and number of fields in a compound type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The compound type or field name.</param>
    /// <param name="sizep">Receives the size value.</param>
    /// <param name="nfieldsp">Receives the number of fields.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound(int ncid, NCType xtype, [Out] byte[] name, out nuint sizep, out nuint nfieldsp);

    /// <summary> Get the name of a compound type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The compound type or field name.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_name(int ncid, NCType xtype, [Out] byte[] name);

    /// <summary> Get the size of a compound type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="sizep">Receives the size value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_size(int ncid, NCType xtype, out nuint sizep);

    /// <summary> Get the number of fields in this compound type. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="nfieldsp">Receives the number of fields.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_nfields(int ncid, NCType xtype, out nuint nfieldsp);

    /// <summary> Given the xtype and the fieldid, get all info about it. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="fieldid">The compound field ID.</param>
    /// <param name="name">The compound type or field name.</param>
    /// <param name="offsetp">Receives the field offset.</param>
    /// <param name="field_typeidp">Receives the type ID of the compound field.</param>
    /// <param name="ndimsp">Receives the number of dimensions.</param>
    /// <param name="dim_sizesp">Receives the dimension sizes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_field(int ncid, NCType xtype, int fieldid, [Out] byte[] name, out nuint offsetp, out NCType field_typeidp, out int ndimsp, [Out] int[]? dim_sizesp);

    /// <summary> Given the typeid and the fieldid, get the name. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="fieldid">The compound field ID.</param>
    /// <param name="name">The compound type or field name.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_fieldname(int ncid, NCType xtype, int fieldid, [Out] byte[] name);

    /// <summary> Given the xtype and the name, get the fieldid. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The compound type or field name.</param>
    /// <param name="fieldidp">Receives the compound field ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_fieldindex(int ncid, NCType xtype, string name, out int fieldidp);

    /// <summary> Given the xtype and fieldid, get the offset. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="fieldid">The compound field ID.</param>
    /// <param name="offsetp">Receives the field offset.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_fieldoffset(int ncid, NCType xtype, int fieldid, out nuint offsetp);

    /// <summary> Given the xtype and the fieldid, get the type of that field. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="fieldid">The compound field ID.</param>
    /// <param name="field_typeidp">Receives the type ID of the compound field.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_fieldtype(int ncid, NCType xtype, int fieldid, out NCType field_typeidp);

    /// <summary> Given the xtype and the fieldid, get the number of dimensions for that field (scalars are 0). </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="fieldid">The compound field ID.</param>
    /// <param name="ndimsp">Receives the number of dimensions.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_fieldndims(int ncid, NCType xtype, int fieldid, out int ndimsp);

    /// <summary> Given the xtype and the fieldid, get the sizes of dimensions for that field. User must have allocated storage for the dim_sizes. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="fieldid">The compound field ID.</param>
    /// <param name="dim_sizes">The dimension sizes.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_compound_fielddim_sizes(int ncid, NCType xtype, int fieldid, [Out] int[] dim_sizes);

    /// <summary>
    /// Create an enum type. Provide a base type and a name. At the moment
    /// only ints are accepted as base types. 
    /// </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="base_typeid">The base type ID.</param>
    /// <param name="name">The enum type or member name.</param>
    /// <param name="typeidp">Receives the netCDF type ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_enum(int ncid, NCType base_typeid, string name, out NCType typeidp);

    /// <summary>Insert a named value into an enum type.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The enum type or member name.</param>
    /// <param name="value">The value to write.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_insert_enum(int ncid, NCType xtype, string name, ref int value);

    /// <summary>Get information about an enum type: its name, base type and the number of members defined. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The enum type or member name.</param>
    /// <param name="base_NCTypep">Receives the base type ID.</param>
    /// <param name="base_sizep">Receives the base type size.</param>
    /// <param name="num_membersp">Receives the number of enum members.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_enum(int ncid, NCType xtype, [Out] byte[] name, out NCType base_NCTypep, out nuint base_sizep, out nuint num_membersp);

    /// <summary>Get information about an enum member</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="idx">The enum member index.</param>
    /// <param name="name">The enum type or member name.</param>
    /// <param name="value">Receives the value.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_enum_member(int ncid, NCType xtype, int idx, [Out] byte[]? name, out int value);

    /// <summary>Get enum name from enum value. Name size will be <= NC_MAX_NAME.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="value">Receives the value.</param>
    /// <param name="identifier">Receives the enum identifier.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_enum_ident(int ncid, NCType xtype, long value, [Out] byte[] identifier);

    /// <summary>Create a variable length type.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="name">The type name.</param>
    /// <param name="base_typeid">The base type ID.</param>
    /// <param name="xtypep">Receives the netCDF type ID.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_def_vlen(int ncid, string name, NCType base_typeid, out NCType xtypep);

    /// <summary> Find out about a vlen. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="xtype">The netCDF type ID.</param>
    /// <param name="name">The type name.</param>
    /// <param name="datum_sizep">Receives the VLEN datum size.</param>
    /// <param name="base_NCTypep">Receives the base type ID.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_inq_vlen(int ncid, NCType xtype, [Out] byte[] name, out nuint datum_sizep, out NCType base_NCTypep);

    /// <summary> When you read VLEN type the library will actually allocate the storage space for the data. This storage space must be freed, so pass the pointer back to this function, when you're done with the data, and it will free the vlen memory. </summary>
    /// <param name="vl">The VLEN value to free.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_free_vlen(ref NcVlen vl);

    /// <summary>Free resources associated with an array of VLEN values.</summary>
    /// <param name="len">The length value.</param>
    /// <param name="vlens">The VLEN values to free.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_free_vlens(nuint len, [In, Out] NcVlen[] vlens);

    /// <summary> Put or get one element in a vlen array. </summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="typeid1">The netCDF type ID.</param>
    /// <param name="vlen_element">The VLEN element.</param>
    /// <param name="len">The length value.</param>
    /// <param name="data">The data buffer or pointer.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_put_vlen_element(int ncid, int typeid1, ref NcVlen vlen_element, nuint len, IntPtr data);

    /// <summary>Get the data and length from one VLEN element.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="typeid1">The netCDF type ID.</param>
    /// <param name="vlen_element">The VLEN element.</param>
    /// <param name="len">The length value.</param>
    /// <param name="data">The data buffer or pointer.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_vlen_element(int ncid, int typeid1, ref NcVlen vlen_element, out nuint len, IntPtr data);

    /// <summary>
    /// Set the default nc_create format to NC_FORMAT_CLASSIC, NC_FORMAT_64BIT,
    /// NC_FORMAT_CDF5, NC_FORMAT_NETCDF4, or NC_FORMAT_NETCDF4_CLASSIC 
    /// </summary>
    /// <param name="format">The format value.</param>
    /// <param name="old_formatp">Receives the previous default format.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_set_default_format(int format, out int old_formatp);

    /// <summary>Set the cache size, nelems, and preemption policy.</summary>
    /// <param name="size">The size value.</param>
    /// <param name="nelems">The number of cache elements.</param>
    /// <param name="preemption">The cache preemption policy.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_set_chunk_cache(nuint size, nuint nelems, float preemption);

    /// <summary>Get the cache size, nelems, and preemption policy.</summary>
    /// <param name="sizep">Receives the size value.</param>
    /// <param name="nelemsp">Receives the number of cache elements.</param>
    /// <param name="preemptionp">Receives the cache preemption policy.</param>
    [LibraryImport(library)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_chunk_cache(out nuint sizep, out nuint nelemsp, out float preemptionp);

    /// <summary>Read an attribute as string data.</summary>
    /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
    /// <param name="varid">The variable ID.</param>
    /// <param name="name">The attribute name.</param>
    /// <param name="ip">The destination buffer or value.</param>
    [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial int nc_get_att_string(int ncid, int varid, string name, [Out] IntPtr[] ip);

    /// <summary>
    /// Unix bindings for methods with signatures that differ between windows
    /// and unix.
    /// </summary>
    /// <remarks>
    /// long is 32-bit on windows, but 64-bit elsewhere. Therefore we need
    /// different signatures for these platforms for all functions that use
    /// long.
    /// </remarks>
    public static partial class Unix
    {
        /// <summary>Read all values from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="values">The value buffer.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_var_long(int ncid, int varid, [Out] long[] values);

        /// <summary>Read one value from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="index">The element index.</param>
        /// <param name="ip">The destination buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_var1_long(int ncid, int varid, [In] nuint[] index, out long ip);

        /// <summary>Read an array section from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="start">The starting indices.</param>
        /// <param name="count">The element counts.</param>
        /// <param name="ip">The destination buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_vara_long(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] long[] ip);

        /// <summary>Read a strided array section from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="startp">The starting indices.</param>
        /// <param name="countp">The element counts.</param>
        /// <param name="stridep">The strides.</param>
        /// <param name="ip">The destination buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_vars_long(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] long[] ip);

        /// <summary>Write all values to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_var_long(int ncid, int varid, [In] long[] op);

        /// <summary>Write one value to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="index">The element index.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_var1_long(int ncid, int varid, [In] nuint[] index, ref long op);

        /// <summary>Write an array section to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="start">The starting indices.</param>
        /// <param name="count">The element counts.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_vara_long(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] long[] op);

        /// <summary>Write a strided array section to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="startp">The starting indices.</param>
        /// <param name="countp">The element counts.</param>
        /// <param name="stridep">The strides.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_vars_long(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] long[] op);

        /// <summary>Read an attribute as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">Receives the value.</param>
        [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_att_long(int ncid, int varid, string name, [Out] long[] value);

        /// <summary>Write an attribute as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="type">The netCDF type ID.</param>
        /// <param name="len">The length value.</param>
        /// <param name="value">The value to write.</param>
        [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_att_long(int ncid, int varid, string name, NCType type, nuint len, [In] long[] value);
    }

    /// <summary>
    /// Windows bindings for methods with signatures that differ between windows
    /// and unix.
    /// </summary>
    /// <remarks>
    /// long is 32-bit on windows, but 64-bit elsewhere. Therefore we need
    /// different signatures for these platforms for all functions that use
    /// long.
    /// </remarks>
    public static partial class Windows
    {
        /// <summary>Read all values from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="values">The value buffer.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_var_long(int ncid, int varid, [Out] int[] values);

        /// <summary>Read one value from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="index">The element index.</param>
        /// <param name="ip">The destination buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_var1_long(int ncid, int varid, [In] nuint[] index, out int ip);

        /// <summary>Read an array section from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="start">The starting indices.</param>
        /// <param name="count">The element counts.</param>
        /// <param name="ip">The destination buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_vara_long(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [Out] int[] ip);

        /// <summary>Read a strided array section from a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="startp">The starting indices.</param>
        /// <param name="countp">The element counts.</param>
        /// <param name="stridep">The strides.</param>
        /// <param name="ip">The destination buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_vars_long(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [Out] int[] ip);

        /// <summary>Write all values to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_var_long(int ncid, int varid, [In] int[] op);

        /// <summary>Write one value to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="index">The element index.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_var1_long(int ncid, int varid, [In] nuint[] index, ref int op);

        /// <summary>Write an array section to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="start">The starting indices.</param>
        /// <param name="count">The element counts.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_vara_long(int ncid, int varid, [In] nuint[] start, [In] nuint[] count, [In] int[] op);

        /// <summary>Write a strided array section to a variable as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="startp">The starting indices.</param>
        /// <param name="countp">The element counts.</param>
        /// <param name="stridep">The strides.</param>
        /// <param name="op">The source buffer or value.</param>
        [LibraryImport(library)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_vars_long(int ncid, int varid, [In] nuint[] startp, [In] nuint[] countp, [In] nint[] stridep, [In] int[] op);

        /// <summary>Read an attribute as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="value">Receives the value.</param>
        [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_get_att_long(int ncid, int varid, string name, [Out] int[] value);

        /// <summary>Write an attribute as long data.</summary>
        /// <param name="ncid">The netCDF file, group, or dataset ID.</param>
        /// <param name="varid">The variable ID.</param>
        /// <param name="name">The attribute name.</param>
        /// <param name="type">The netCDF type ID.</param>
        /// <param name="len">The length value.</param>
        /// <param name="value">The value to write.</param>
        [LibraryImport(library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int nc_put_att_long(int ncid, int varid, string name, NCType type, nuint len, [In] int[] value);
    }
}
