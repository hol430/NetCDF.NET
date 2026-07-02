using NetCDF.Interop;
using NetCDF.Tests.Helpers;

namespace NetCDF.Tests.Interop;

public sealed class CompoundTypeTests
{
    [Fact]
    public void CompoundType_InquiryAPIs_ReturnExpectedContracts()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        int defStatus = Native.nc_def_compound(hnd.Id, 16, "sample_compound", out NCType typeId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(defStatus, "nc_def_compound");

        InteropTestCommon.AssertSuccess(Native.nc_insert_compound(hnd.Id, typeId, "scalar", 0, NCType.NC_INT), "nc_insert_compound");
        InteropTestCommon.AssertSuccess(Native.nc_insert_array_compound(hnd.Id, typeId, "vector", 4, NCType.NC_INT, 1, [3]), "nc_insert_array_compound");

        byte[] inqName = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound(hnd.Id, typeId, inqName, out nuint size, out nuint nfields), "nc_inq_compound");
        Assert.Equal("sample_compound", ReadCString(inqName));
        Assert.Equal((nuint)16, size);
        Assert.Equal((nuint)2, nfields);

        byte[] nameOnly = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_name(hnd.Id, typeId, nameOnly), "nc_inq_compound_name");
        Assert.Equal("sample_compound", ReadCString(nameOnly));

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_size(hnd.Id, typeId, out nuint sizeOnly), "nc_inq_compound_size");
        Assert.Equal((nuint)16, sizeOnly);

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_nfields(hnd.Id, typeId, out nuint nfieldsOnly), "nc_inq_compound_nfields");
        Assert.Equal((nuint)2, nfieldsOnly);

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldindex(hnd.Id, typeId, "scalar", out int scalarIndex), "nc_inq_compound_fieldindex(scalar)");
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldindex(hnd.Id, typeId, "vector", out int vectorIndex), "nc_inq_compound_fieldindex(vector)");
        Assert.NotEqual(scalarIndex, vectorIndex);

        AssertField(hnd.Id, typeId, scalarIndex, "scalar", 0, NCType.NC_INT, 0, null);
        AssertField(hnd.Id, typeId, vectorIndex, "vector", 4, NCType.NC_INT, 1, [3]);
    }

    private static void AssertField(int ncid, NCType typeId, int fieldIndex, string expectedName, int expectedOffset, NCType expectedType, int expectedNdims, int[]? expectedDims)
    {
        byte[] fullName = new byte[256];
        int[]? dimSizes = expectedNdims > 0 ? new int[expectedNdims] : null;
        InteropTestCommon.AssertSuccess(
            Native.nc_inq_compound_field(ncid, typeId, fieldIndex, fullName, out nuint fullOffset, out NCType fullType, out int fullNdims, dimSizes),
            "nc_inq_compound_field");
        Assert.Equal(expectedName, ReadCString(fullName));
        Assert.Equal((nuint)expectedOffset, fullOffset);
        Assert.Equal(expectedType, fullType);
        Assert.Equal(expectedNdims, fullNdims);
        if (expectedDims is not null)
        {
            Assert.Equal(expectedDims, dimSizes);
        }

        byte[] fieldName = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldname(ncid, typeId, fieldIndex, fieldName), "nc_inq_compound_fieldname");
        Assert.Equal(expectedName, ReadCString(fieldName));

        InteropTestCommon.AssertSuccess(Native.nc_inq_compound_fieldoffset(ncid, typeId, fieldIndex, out nuint offset), "nc_inq_compound_fieldoffset");
        Assert.Equal((nuint)expectedOffset, offset);

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

    private static string ReadCString(byte[] bytes)
    {
        int nulIndex = Array.IndexOf(bytes, (byte)0);
        int length = nulIndex >= 0 ? nulIndex : bytes.Length;
        return System.Text.Encoding.ASCII.GetString(bytes, 0, length);
    }
}
