using NetCDF.Interop;

namespace NetCDF.LowLevel;

public sealed partial class NetCdfApi
{
    /// <summary>
    /// Inquires a type name and size.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="type">The type ID.</param>
    /// <returns>The type metadata.</returns>
    public TypeInfo InquireType(NetCdfHandle handle, NCType type)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_type), ncid, type);

        int status = Native.nc_inq_type(ncid, type, name, out nuint size);
        LogReturned(nameof(Native.nc_inq_type), status);
        Check(status, nameof(Native.nc_inq_type));

        return new TypeInfo(NativeString.DecodeNullTerminatedUtf8(name), size);
    }

    /// <summary>
    /// Inquires whether two netCDF types are equal.
    /// </summary>
    /// <param name="firstHandle">The first open netCDF file handle.</param>
    /// <param name="firstType">The first type ID.</param>
    /// <param name="secondHandle">The second open netCDF file handle.</param>
    /// <param name="secondType">The second type ID.</param>
    /// <returns><see langword="true"/> if the types are equal; otherwise <see langword="false"/>.</returns>
    public bool InquireTypeEqual(NetCdfHandle firstHandle, NCType firstType, NetCdfHandle secondHandle, NCType secondType)
    {
        int firstNcid = firstHandle.Id;
        int secondNcid = secondHandle.Id;
        logger?.LogDebug("{FunctionName}: ncid1={Ncid1}, type1={Type1}, ncid2={Ncid2}, type2={Type2}", nameof(Native.nc_inq_type_equal), firstNcid, firstType, secondNcid, secondType);

        int status = Native.nc_inq_type_equal(firstNcid, firstType, secondNcid, secondType, out int equal);
        LogReturned(nameof(Native.nc_inq_type_equal), status);
        Check(status, nameof(Native.nc_inq_type_equal));

        return equal != 0;
    }

    /// <summary>
    /// Inquires a user-defined type ID by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The type name.</param>
    /// <returns>The matching type ID.</returns>
    public NCType InquireTypeId(NetCdfHandle handle, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}", nameof(Native.nc_inq_typeid), ncid, name);

        int status = Native.nc_inq_typeid(ncid, name, out NCType type);
        LogReturned(nameof(Native.nc_inq_typeid), status);
        Check(status, nameof(Native.nc_inq_typeid));

        return type;
    }

    /// <summary>
    /// Inquires user-defined type metadata.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="type">The user-defined type ID.</param>
    /// <returns>The user-defined type metadata.</returns>
    public UserTypeInfo InquireUserType(NetCdfHandle handle, NCType type)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_user_type), ncid, type);

        int status = Native.nc_inq_user_type(ncid, type, name, out nuint size, out NCType baseType, out nuint fields, out int typeClass);
        LogReturned(nameof(Native.nc_inq_user_type), status);
        Check(status, nameof(Native.nc_inq_user_type));

        return new UserTypeInfo(NativeString.DecodeNullTerminatedUtf8(name), size, baseType, fields, typeClass);
    }

    /// <summary>
    /// Defines a compound type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="size">The compound type size in bytes.</param>
    /// <param name="name">The compound type name.</param>
    /// <returns>The new compound type ID.</returns>
    public NCType DefineCompound(NetCdfHandle handle, nuint size, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, size={Size}, name={Name}", nameof(Native.nc_def_compound), ncid, size, name);

        int status = Native.nc_def_compound(ncid, size, name, out NCType type);
        LogReturned(nameof(Native.nc_def_compound), status);
        Check(status, nameof(Native.nc_def_compound));

        return type;
    }

    /// <summary>
    /// Inserts a scalar field into a compound type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="name">The field name.</param>
    /// <param name="offset">The field byte offset.</param>
    /// <param name="fieldType">The field type ID.</param>
    public void InsertCompound(NetCdfHandle handle, NCType compoundType, string name, nuint offset, NCType fieldType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, name={Name}, offset={Offset}, fieldType={FieldType}", nameof(Native.nc_insert_compound), ncid, compoundType, name, offset, fieldType);

        int status = Native.nc_insert_compound(ncid, compoundType, name, offset, fieldType);
        LogReturned(nameof(Native.nc_insert_compound), status);
        Check(status, nameof(Native.nc_insert_compound));
    }

    /// <summary>
    /// Inserts an array field into a compound type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="name">The field name.</param>
    /// <param name="offset">The field byte offset.</param>
    /// <param name="fieldType">The field type ID.</param>
    /// <param name="dimensionSizes">The array dimension sizes.</param>
    public void InsertArrayCompound(NetCdfHandle handle, NCType compoundType, string name, nuint offset, NCType fieldType, IReadOnlyList<int> dimensionSizes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(dimensionSizes);
        int ncid = handle.Id;
        int[] sizes = dimensionSizes.ToArray();
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, name={Name}, offset={Offset}, fieldType={FieldType}, dimensions=[{Dimensions}]", nameof(Native.nc_insert_array_compound), ncid, compoundType, name, offset, fieldType, string.Join(", ", sizes));

        int status = Native.nc_insert_array_compound(ncid, compoundType, name, offset, fieldType, sizes.Length, sizes);
        LogReturned(nameof(Native.nc_insert_array_compound), status);
        Check(status, nameof(Native.nc_insert_array_compound));
    }

    /// <summary>
    /// Inquires compound type metadata.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <returns>The compound type metadata.</returns>
    public CompoundTypeInfo InquireCompound(NetCdfHandle handle, NCType compoundType)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_compound), ncid, compoundType);

        int status = Native.nc_inq_compound(ncid, compoundType, name, out nuint size, out nuint fields);
        LogReturned(nameof(Native.nc_inq_compound), status);
        Check(status, nameof(Native.nc_inq_compound));

        return new CompoundTypeInfo(NativeString.DecodeNullTerminatedUtf8(name), size, fields);
    }

    /// <summary>
    /// Inquires a compound type name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <returns>The compound type name.</returns>
    public string InquireCompoundName(NetCdfHandle handle, NCType compoundType)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_compound_name), ncid, compoundType);

        int status = Native.nc_inq_compound_name(ncid, compoundType, name);
        LogReturned(nameof(Native.nc_inq_compound_name), status);
        Check(status, nameof(Native.nc_inq_compound_name));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires a compound type size.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <returns>The compound type size in bytes.</returns>
    public nuint InquireCompoundSize(NetCdfHandle handle, NCType compoundType)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_compound_size), ncid, compoundType);

        int status = Native.nc_inq_compound_size(ncid, compoundType, out nuint size);
        LogReturned(nameof(Native.nc_inq_compound_size), status);
        Check(status, nameof(Native.nc_inq_compound_size));

        return size;
    }

    /// <summary>
    /// Inquires the number of fields in a compound type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <returns>The field count.</returns>
    public nuint InquireCompoundFieldCount(NetCdfHandle handle, NCType compoundType)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_compound_nfields), ncid, compoundType);

        int status = Native.nc_inq_compound_nfields(ncid, compoundType, out nuint fields);
        LogReturned(nameof(Native.nc_inq_compound_nfields), status);
        Check(status, nameof(Native.nc_inq_compound_nfields));

        return fields;
    }

    /// <summary>
    /// Inquires compound field metadata.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="fieldId">The field index.</param>
    /// <returns>The compound field metadata.</returns>
    public CompoundFieldInfo InquireCompoundField(NetCdfHandle handle, NCType compoundType, int fieldId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        int[] dimensions = new int[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, fieldId={FieldId}", nameof(Native.nc_inq_compound_field), ncid, compoundType, fieldId);

        int status = Native.nc_inq_compound_field(ncid, compoundType, fieldId, name, out nuint offset, out NCType fieldType, out int ndims, dimensions);
        LogReturned(nameof(Native.nc_inq_compound_field), status);
        Check(status, nameof(Native.nc_inq_compound_field));

        return new CompoundFieldInfo(NativeString.DecodeNullTerminatedUtf8(name), offset, fieldType, dimensions.Take(ndims).ToArray());
    }

    /// <summary>
    /// Inquires a compound field name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="fieldId">The field index.</param>
    /// <returns>The field name.</returns>
    public string InquireCompoundFieldName(NetCdfHandle handle, NCType compoundType, int fieldId)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, fieldId={FieldId}", nameof(Native.nc_inq_compound_fieldname), ncid, compoundType, fieldId);

        int status = Native.nc_inq_compound_fieldname(ncid, compoundType, fieldId, name);
        LogReturned(nameof(Native.nc_inq_compound_fieldname), status);
        Check(status, nameof(Native.nc_inq_compound_fieldname));

        return NativeString.DecodeNullTerminatedUtf8(name);
    }

    /// <summary>
    /// Inquires a compound field index by name.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="name">The field name.</param>
    /// <returns>The matching field index.</returns>
    public int InquireCompoundFieldIndex(NetCdfHandle handle, NCType compoundType, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, name={Name}", nameof(Native.nc_inq_compound_fieldindex), ncid, compoundType, name);

        int status = Native.nc_inq_compound_fieldindex(ncid, compoundType, name, out int fieldId);
        LogReturned(nameof(Native.nc_inq_compound_fieldindex), status);
        Check(status, nameof(Native.nc_inq_compound_fieldindex));

        return fieldId;
    }

    /// <summary>
    /// Inquires a compound field byte offset.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="fieldId">The field index.</param>
    /// <returns>The field byte offset.</returns>
    public nuint InquireCompoundFieldOffset(NetCdfHandle handle, NCType compoundType, int fieldId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, fieldId={FieldId}", nameof(Native.nc_inq_compound_fieldoffset), ncid, compoundType, fieldId);

        int status = Native.nc_inq_compound_fieldoffset(ncid, compoundType, fieldId, out nuint offset);
        LogReturned(nameof(Native.nc_inq_compound_fieldoffset), status);
        Check(status, nameof(Native.nc_inq_compound_fieldoffset));

        return offset;
    }

    /// <summary>
    /// Inquires a compound field type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="fieldId">The field index.</param>
    /// <returns>The field type.</returns>
    public NCType InquireCompoundFieldType(NetCdfHandle handle, NCType compoundType, int fieldId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, fieldId={FieldId}", nameof(Native.nc_inq_compound_fieldtype), ncid, compoundType, fieldId);

        int status = Native.nc_inq_compound_fieldtype(ncid, compoundType, fieldId, out NCType fieldType);
        LogReturned(nameof(Native.nc_inq_compound_fieldtype), status);
        Check(status, nameof(Native.nc_inq_compound_fieldtype));

        return fieldType;
    }

    /// <summary>
    /// Inquires the number of dimensions used by a compound array field.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="fieldId">The field index.</param>
    /// <returns>The number of array dimensions.</returns>
    public int InquireCompoundFieldDimensionCount(NetCdfHandle handle, NCType compoundType, int fieldId)
    {
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, fieldId={FieldId}", nameof(Native.nc_inq_compound_fieldndims), ncid, compoundType, fieldId);

        int status = Native.nc_inq_compound_fieldndims(ncid, compoundType, fieldId, out int count);
        LogReturned(nameof(Native.nc_inq_compound_fieldndims), status);
        Check(status, nameof(Native.nc_inq_compound_fieldndims));

        return count;
    }

    /// <summary>
    /// Inquires dimension sizes for a compound array field.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="compoundType">The compound type ID.</param>
    /// <param name="fieldId">The field index.</param>
    /// <returns>The array dimension sizes.</returns>
    public IReadOnlyList<int> InquireCompoundFieldDimensionSizes(NetCdfHandle handle, NCType compoundType, int fieldId)
    {
        int count = InquireCompoundFieldDimensionCount(handle, compoundType, fieldId);
        int ncid = handle.Id;
        int[] sizes = new int[count];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, fieldId={FieldId}", nameof(Native.nc_inq_compound_fielddim_sizes), ncid, compoundType, fieldId);

        int status = Native.nc_inq_compound_fielddim_sizes(ncid, compoundType, fieldId, sizes);
        LogReturned(nameof(Native.nc_inq_compound_fielddim_sizes), status);
        Check(status, nameof(Native.nc_inq_compound_fielddim_sizes));

        return sizes;
    }

    /// <summary>
    /// Defines an enum type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="baseType">The enum base type.</param>
    /// <param name="name">The enum type name.</param>
    /// <returns>The new enum type ID.</returns>
    public NCType DefineEnum(NetCdfHandle handle, NCType baseType, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, baseType={BaseType}, name={Name}", nameof(Native.nc_def_enum), ncid, baseType, name);

        int status = Native.nc_def_enum(ncid, baseType, name, out NCType type);
        LogReturned(nameof(Native.nc_def_enum), status);
        Check(status, nameof(Native.nc_def_enum));

        return type;
    }

    /// <summary>
    /// Inserts an enum member.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="enumType">The enum type ID.</param>
    /// <param name="name">The member name.</param>
    /// <param name="value">The member value.</param>
    public void InsertEnum(NetCdfHandle handle, NCType enumType, string name, int value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        int nativeValue = value;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, name={Name}, value={Value}", nameof(Native.nc_insert_enum), ncid, enumType, name, value);

        int status = Native.nc_insert_enum(ncid, enumType, name, ref nativeValue);
        LogReturned(nameof(Native.nc_insert_enum), status);
        Check(status, nameof(Native.nc_insert_enum));
    }

    /// <summary>
    /// Inquires enum type metadata.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="enumType">The enum type ID.</param>
    /// <returns>The enum type metadata.</returns>
    public EnumTypeInfo InquireEnum(NetCdfHandle handle, NCType enumType)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_enum), ncid, enumType);

        int status = Native.nc_inq_enum(ncid, enumType, name, out NCType baseType, out nuint baseSize, out nuint members);
        LogReturned(nameof(Native.nc_inq_enum), status);
        Check(status, nameof(Native.nc_inq_enum));

        return new EnumTypeInfo(NativeString.DecodeNullTerminatedUtf8(name), baseType, baseSize, members);
    }

    /// <summary>
    /// Inquires enum member metadata by index.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="enumType">The enum type ID.</param>
    /// <param name="index">The member index.</param>
    /// <returns>The enum member metadata.</returns>
    public EnumMemberInfo InquireEnumMember(NetCdfHandle handle, NCType enumType, int index)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, index={Index}", nameof(Native.nc_inq_enum_member), ncid, enumType, index);

        int status = Native.nc_inq_enum_member(ncid, enumType, index, name, out int value);
        LogReturned(nameof(Native.nc_inq_enum_member), status);
        Check(status, nameof(Native.nc_inq_enum_member));

        return new EnumMemberInfo(NativeString.DecodeNullTerminatedUtf8(name), value);
    }

    /// <summary>
    /// Inquires an enum member identifier by value.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="enumType">The enum type ID.</param>
    /// <param name="value">The enum member value.</param>
    /// <returns>The enum member identifier.</returns>
    public string InquireEnumIdentifier(NetCdfHandle handle, NCType enumType, long value)
    {
        int ncid = handle.Id;
        byte[] identifier = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, value={Value}", nameof(Native.nc_inq_enum_ident), ncid, enumType, value);

        int status = Native.nc_inq_enum_ident(ncid, enumType, value, identifier);
        LogReturned(nameof(Native.nc_inq_enum_ident), status);
        Check(status, nameof(Native.nc_inq_enum_ident));

        return NativeString.DecodeNullTerminatedUtf8(identifier);
    }

    /// <summary>
    /// Defines a variable-length type.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="name">The VLen type name.</param>
    /// <param name="baseType">The VLen base type.</param>
    /// <returns>The new VLen type ID.</returns>
    public NCType DefineVLen(NetCdfHandle handle, string name, NCType baseType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        int ncid = handle.Id;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, name={Name}, baseType={BaseType}", nameof(Native.nc_def_vlen), ncid, name, baseType);

        int status = Native.nc_def_vlen(ncid, name, baseType, out NCType type);
        LogReturned(nameof(Native.nc_def_vlen), status);
        Check(status, nameof(Native.nc_def_vlen));

        return type;
    }

    /// <summary>
    /// Inquires variable-length type metadata.
    /// </summary>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="vlenType">The VLen type ID.</param>
    /// <returns>The VLen type metadata.</returns>
    public VLenTypeInfo InquireVLen(NetCdfHandle handle, NCType vlenType)
    {
        int ncid = handle.Id;
        byte[] name = new byte[NameBufferSize];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}", nameof(Native.nc_inq_vlen), ncid, vlenType);

        int status = Native.nc_inq_vlen(ncid, vlenType, name, out nuint datumSize, out NCType baseType);
        LogReturned(nameof(Native.nc_inq_vlen), status);
        Check(status, nameof(Native.nc_inq_vlen));

        return new VLenTypeInfo(NativeString.DecodeNullTerminatedUtf8(name), datumSize, baseType);
    }

    /// <summary>
    /// Creates a native VLen element from managed values.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="vlenType">The VLen type ID.</param>
    /// <param name="values">The managed values to copy into the native VLen element.</param>
    /// <returns>The native VLen element.</returns>
    public unsafe VLenElement CreateVLenElement<T>(NetCdfHandle handle, NCType vlenType, T[] values)
        where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(values);
        int ncid = handle.Id;
        Native.NcVlen element = default;
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, elementType={ElementType}, elementCount={ElementCount}", nameof(Native.nc_put_vlen_element), ncid, vlenType, typeof(T).Name, values.Length);

        fixed (T* valuePtr = values)
        {
            int status = Native.nc_put_vlen_element(ncid, (int)vlenType, ref element, (nuint)values.Length, (IntPtr)valuePtr);
            LogReturned(nameof(Native.nc_put_vlen_element), status);
            Check(status, nameof(Native.nc_put_vlen_element));
        }

        return new VLenElement(element);
    }

    /// <summary>
    /// Reads a native VLen element into a managed array.
    /// </summary>
    /// <typeparam name="T">The unmanaged element type.</typeparam>
    /// <param name="handle">The open netCDF file handle.</param>
    /// <param name="vlenType">The VLen type ID.</param>
    /// <param name="element">The native VLen element.</param>
    /// <returns>The managed values read from the VLen element.</returns>
    public unsafe T[] ReadVLenElement<T>(NetCdfHandle handle, NCType vlenType, VLenElement element)
        where T : unmanaged
    {
        int ncid = handle.Id;
        Native.NcVlen nativeElement = element.Value;
        T[] values = new T[checked((int)element.Length)];
        logger?.LogDebug("{FunctionName}: ncid={Ncid}, type={Type}, elementType={ElementType}, elementCount={ElementCount}", nameof(Native.nc_get_vlen_element), ncid, vlenType, typeof(T).Name, values.Length);

        fixed (T* valuePtr = values)
        {
            int status = Native.nc_get_vlen_element(ncid, (int)vlenType, ref nativeElement, out nuint length, (IntPtr)valuePtr);
            LogReturned(nameof(Native.nc_get_vlen_element), status);
            Check(status, nameof(Native.nc_get_vlen_element));
            if (length != element.Length)
            {
                throw new InvalidOperationException($"Expected VLen length {element.Length}, but libnetcdf returned {length}.");
            }
        }

        return values;
    }

    /// <summary>
    /// Frees a native VLen element.
    /// </summary>
    /// <param name="element">The VLen element to free.</param>
    public void FreeVLenElement(VLenElement element)
    {
        Native.NcVlen nativeElement = element.Value;
        logger?.LogDebug("{FunctionName}: elementCount={ElementCount}", nameof(Native.nc_free_vlen), element.Length);

        int status = Native.nc_free_vlen(ref nativeElement);
        LogReturned(nameof(Native.nc_free_vlen), status);
        Check(status, nameof(Native.nc_free_vlen));
    }

    /// <summary>
    /// Frees native VLen elements.
    /// </summary>
    /// <param name="elements">The VLen elements to free.</param>
    public void FreeVLenElements(IReadOnlyList<VLenElement> elements)
    {
        ArgumentNullException.ThrowIfNull(elements);
        Native.NcVlen[] nativeElements = elements.Select(static e => e.Value).ToArray();
        logger?.LogDebug("{FunctionName}: elementCount={ElementCount}", nameof(Native.nc_free_vlens), nativeElements.Length);

        int status = Native.nc_free_vlens((nuint)nativeElements.Length, nativeElements);
        LogReturned(nameof(Native.nc_free_vlens), status);
        Check(status, nameof(Native.nc_free_vlens));
    }
}
