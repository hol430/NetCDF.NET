using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class CompoundTypeTests
{
    [Fact]
    public void CompoundType_InquiryAPIs_ReturnExpectedContracts()
    {
        using NcTempFile hnd = new();

        int defStatus = Native.nc_def_compound(hnd.Id, 16, "sample_compound", out NCType typeId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(defStatus, "nc_def_compound");

        InteropTestCommon.AssertSuccess(Native.nc_insert_compound(hnd.Id, typeId, "scalar", 0, NCType.NC_INT), "nc_insert_compound");
        InteropTestCommon.AssertSuccess(Native.nc_insert_array_compound(hnd.Id, typeId, "vector", 4, NCType.NC_INT, 1, [3]), "nc_insert_array_compound");

        var inqName = new StringBuilder(256);
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound(hnd.Id, typeId, inqName, out IntPtr size, out int nfields), "nc_inq_compound");
        Assert.Equal("sample_compound", inqName.ToString());
        Assert.Equal(16, size.ToInt64());
        Assert.Equal(2, nfields);

        var nameOnly = new StringBuilder(256);
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_name(hnd.Id, typeId, nameOnly), "nc_inq_compound_name");
        Assert.Equal("sample_compound", nameOnly.ToString());

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_size(hnd.Id, typeId, out IntPtr sizeOnly), "nc_inq_compound_size");
        Assert.Equal(16, sizeOnly.ToInt64());

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_nfields(hnd.Id, typeId, out int nfieldsOnly), "nc_inq_compound_nfields");
        Assert.Equal(2, nfieldsOnly);

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldindex(hnd.Id, typeId, "scalar", out int scalarIndex), "nc_inq_compound_fieldindex(scalar)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldindex(hnd.Id, typeId, "vector", out int vectorIndex), "nc_inq_compound_fieldindex(vector)");
        Assert.NotEqual(scalarIndex, vectorIndex);

        AssertField(hnd.Id, typeId, scalarIndex, "scalar", 0, NCType.NC_INT, 0, null);
        AssertField(hnd.Id, typeId, vectorIndex, "vector", 4, NCType.NC_INT, 1, [3]);
    }

    private static void AssertField(int ncid, NCType typeId, int fieldIndex, string expectedName, int expectedOffset, NCType expectedType, int expectedNdims, int[]? expectedDims)
    {
        var fullName = new StringBuilder(256);
        int[]? dimSizes = expectedNdims > 0 ? new int[expectedNdims] : null;
        InteropTestCommon.AssertSuccess(
            Native.nc_inq_compound_field(ncid, typeId, fieldIndex, fullName, out int fullOffset, out NCType fullType, out int fullNdims, dimSizes),
            "nc_inq_compound_field");
        Assert.Equal(expectedName, fullName.ToString());
        Assert.Equal(expectedOffset, fullOffset);
        Assert.Equal(expectedType, fullType);
        Assert.Equal(expectedNdims, fullNdims);
        if (expectedDims is not null)
        {
            Assert.Equal(expectedDims, dimSizes);
        }

        var fieldName = new StringBuilder(256);
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldname(ncid, typeId, fieldIndex, fieldName), "nc_inq_compound_fieldname");
        Assert.Equal(expectedName, fieldName.ToString());

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldoffset(ncid, typeId, fieldIndex, out int offset), "nc_inq_compound_fieldoffset");
        Assert.Equal(expectedOffset, offset);

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldtype(ncid, typeId, fieldIndex, out NCType fieldType), "nc_inq_compound_fieldtype");
        Assert.Equal(expectedType, fieldType);

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldndims(ncid, typeId, fieldIndex, out int ndims), "nc_inq_compound_fieldndims");
        Assert.Equal(expectedNdims, ndims);

        if (expectedNdims > 0)
        {
            int[] dims = new int[expectedNdims];
            InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fielddim_sizes(ncid, typeId, fieldIndex, dims), "nc_inq_compound_fielddim_sizes");
            Assert.Equal(expectedDims, dims);
        }
    }
}
