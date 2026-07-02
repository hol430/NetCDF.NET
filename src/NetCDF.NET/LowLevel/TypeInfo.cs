using NetCDF.Interop;

namespace NetCDF.LowLevel;

/// <summary>
/// Describes a netCDF type.
/// </summary>
/// <param name="Name">The type name.</param>
/// <param name="Size">The in-memory type size in bytes.</param>
public readonly record struct TypeInfo(string Name, nuint Size);

/// <summary>
/// Describes a netCDF user-defined type.
/// </summary>
/// <param name="Name">The type name.</param>
/// <param name="Size">The in-memory type size in bytes.</param>
/// <param name="BaseType">The base netCDF type.</param>
/// <param name="FieldCount">The number of fields or members.</param>
/// <param name="Class">The native user type class value.</param>
public readonly record struct UserTypeInfo(string Name, nuint Size, NCType BaseType, nuint FieldCount, int Class);

/// <summary>
/// Describes a netCDF compound type.
/// </summary>
/// <param name="Name">The compound type name.</param>
/// <param name="Size">The compound type size in bytes.</param>
/// <param name="FieldCount">The number of fields.</param>
public readonly record struct CompoundTypeInfo(string Name, nuint Size, nuint FieldCount);

/// <summary>
/// Describes a netCDF compound field.
/// </summary>
/// <param name="Name">The field name.</param>
/// <param name="Offset">The field byte offset.</param>
/// <param name="Type">The field type.</param>
/// <param name="DimensionSizes">The array dimension sizes for array fields.</param>
public readonly record struct CompoundFieldInfo(string Name, nuint Offset, NCType Type, IReadOnlyList<int> DimensionSizes);

/// <summary>
/// Describes a netCDF enum type.
/// </summary>
/// <param name="Name">The enum type name.</param>
/// <param name="BaseType">The enum base type.</param>
/// <param name="BaseSize">The enum base type size in bytes.</param>
/// <param name="MemberCount">The number of enum members.</param>
public readonly record struct EnumTypeInfo(string Name, NCType BaseType, nuint BaseSize, nuint MemberCount);

/// <summary>
/// Describes a netCDF enum member.
/// </summary>
/// <param name="Name">The member name.</param>
/// <param name="Value">The member value.</param>
public readonly record struct EnumMemberInfo(string Name, int Value);

/// <summary>
/// Describes a netCDF variable-length type.
/// </summary>
/// <param name="Name">The VLen type name.</param>
/// <param name="DatumSize">The datum size in bytes.</param>
/// <param name="BaseType">The VLen base type.</param>
public readonly record struct VLenTypeInfo(string Name, nuint DatumSize, NCType BaseType);
