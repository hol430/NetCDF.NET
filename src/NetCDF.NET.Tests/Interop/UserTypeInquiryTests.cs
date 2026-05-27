using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class UserTypeInquiryTests
{
    [Fact]
    public void UserTypeInquiry_APIs_ReturnExpectedInformation()
    {
        using NcTempFile hnd = new();

        int defStatus = Native.nc_def_compound(hnd.Id, 4, "compound_t", out NCType typeId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(defStatus, "nc_def_compound");
        InteropTestCommon.AssertSuccess(Native.nc_insert_compound(hnd.Id, typeId, "value", 0, NCType.NC_INT), "nc_insert_compound");

        InteropTestCommon.AssertSuccess(Native.nc_inq_typeid(hnd.Id, "compound_t", out NCType lookedUpTypeId), "nc_inq_typeid");
        Assert.Equal((int)typeId, (int)lookedUpTypeId);

        int typeIdsCountStatus = Native.nc_inq_typeids(hnd.Id, out int ntypes, null!);
        InteropTestCommon.AssertSuccess(typeIdsCountStatus, "nc_inq_typeids(count)");
        Assert.True(ntypes >= 1);

        int[] typeIds = new int[ntypes];
        InteropTestCommon.AssertSuccess(Native.nc_inq_typeids(hnd.Id, out int ntypesAgain, typeIds), "nc_inq_typeids(values)");
        Assert.Equal(ntypes, ntypesAgain);
        Assert.Contains((int)typeId, typeIds);

        var inqName = new StringBuilder(256);
        InteropTestCommon.AssertSuccess(Native.nc_inq_type(hnd.Id, typeId, inqName, out nuint inqSize), "nc_inq_type_untested");
        Assert.Equal("compound_t", inqName.ToString());
        Assert.Equal((nuint)4, inqSize);

        InteropTestCommon.AssertSuccess(Native.nc_enddef(hnd.Id), "nc_enddef");

        InteropTestCommon.AssertSuccess(Native.nc_inq_type_equal(hnd.Id, typeId, hnd.Id, lookedUpTypeId, out int equalSelf), "nc_inq_type_equal(self)");
        Assert.NotEqual(0, equalSelf);

        InteropTestCommon.AssertSuccess(Native.nc_inq_type_equal(hnd.Id, typeId, hnd.Id, NCType.NC_INT, out int equalInt), "nc_inq_type_equal(int)");
        Assert.Equal(0, equalInt);

        var userTypeName = new StringBuilder(256);
        InteropTestCommon.AssertSuccess(
            Native.nc_inq_user_type(hnd.Id, typeId, userTypeName, out IntPtr userTypeSize, out NCType baseType, out int nfields, out int classp),
            "nc_inq_user_type");
        Assert.Equal("compound_t", userTypeName.ToString());
        Assert.Equal(4, userTypeSize.ToInt64());
        Assert.Equal(1, nfields);
        Assert.Equal((int)NCType.NC_COMPOUND, classp);
    }
}
