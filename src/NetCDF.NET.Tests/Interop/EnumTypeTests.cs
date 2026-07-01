using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class EnumTypeTests
{
    [Fact]
    public void EnumType_InquiryAPIs_ReturnExpectedContracts()
    {
        using NcTempFile hnd = new();

        int defStatus = Native.nc_def_enum(hnd.Id, NCType.NC_INT, "color_t", out NCType enumTypeId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(defStatus, "nc_def_enum");

        int red = 1, green = 2, blue = 3;
        InteropTestCommon.AssertSuccess(Native.nc_insert_enum(hnd.Id, enumTypeId, "RED", ref red), "nc_insert_enum(RED)");
        InteropTestCommon.AssertSuccess(Native.nc_insert_enum(hnd.Id, enumTypeId, "GREEN", ref green), "nc_insert_enum(GREEN)");
        InteropTestCommon.AssertSuccess(Native.nc_insert_enum(hnd.Id, enumTypeId, "BLUE", ref blue), "nc_insert_enum(BLUE)");

        byte[] enumName = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_enum(hnd.Id, enumTypeId, enumName, out NCType baseType, out IntPtr baseSize, out int memberCount), "nc_inq_enum");
        Assert.Equal("color_t", DecodeCString(enumName));
        Assert.Equal(NCType.NC_INT, baseType);
        Assert.Equal(sizeof(int), baseSize.ToInt64());
        Assert.Equal(3, memberCount);

        var members = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < memberCount; i++)
        {
            byte[] nameBuffer = new byte[256];
            InteropTestCommon.AssertSuccess(Native.nc_inq_enum_member(hnd.Id, enumTypeId, i, nameBuffer, out int value), "nc_inq_enum_member");
            members[DecodeCString(nameBuffer)] = value;
        }

        Assert.Equal(3, members.Count);
        Assert.Equal(1, members["RED"]);
        Assert.Equal(2, members["GREEN"]);
        Assert.Equal(3, members["BLUE"]);

        byte[] ident = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_enum_ident(hnd.Id, enumTypeId, 2, ident), "nc_inq_enum_ident");
        Assert.Equal("GREEN", DecodeCString(ident));
    }

    private static string DecodeCString(byte[] bytes)
    {
        int nul = Array.IndexOf(bytes, (byte)0);
        int len = nul >= 0 ? nul : bytes.Length;
        return Encoding.ASCII.GetString(bytes, 0, len);
    }
}
