using System.Runtime.InteropServices;
using System.Text;
using NetCDF.Interop;
using NetCDF.Tests.Helpers;
using static NetCDF.LowLevel.Constants;

namespace NetCDF.Tests.Interop;

public sealed class VlenAndGlobalConfigTests
{
    [Fact]
    public void VlenType_DefineInquireAndElementRoundTrip()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        int defStatus = Native.nc_def_vlen(hnd.Id, "int_list_t", NCType.NC_INT, out NCType vlenTypeId);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(defStatus, "nc_def_vlen");

        byte[] name = new byte[256];
        InteropTestCommon.AssertSuccess(Native.nc_inq_vlen(hnd.Id, vlenTypeId, name, out nuint datumSize, out NCType baseType), "nc_inq_vlen");
        Assert.Equal("int_list_t", DecodeCString(name));
        Assert.Equal(NCType.NC_INT, baseType);
        Assert.Equal((IntPtr.Size * 2), (int)datumSize);

        int[] values = [10, 20, 30, 40];
        GCHandle handle = GCHandle.Alloc(values, GCHandleType.Pinned);
        try
        {
            Native.NcVlen vlen = default;
            InteropTestCommon.AssertSuccess(
                Native.nc_put_vlen_element(hnd.Id, (int)vlenTypeId, ref vlen, (nuint)values.Length, handle.AddrOfPinnedObject()),
                "nc_put_vlen_element");

            int[] output = new int[values.Length];
            GCHandle outHandle = GCHandle.Alloc(output, GCHandleType.Pinned);
            try
            {
                InteropTestCommon.AssertSuccess(
                    Native.nc_get_vlen_element(hnd.Id, (int)vlenTypeId, ref vlen, out nuint outLen, outHandle.AddrOfPinnedObject()),
                    "nc_get_vlen_element");
                Assert.Equal((nuint)values.Length, outLen);
                Assert.Equal(values, output);
            }
            finally
            {
                outHandle.Free();
            }

        }
        finally
        {
            handle.Free();
        }
    }

    [Fact]
    public void FreeVlens_ReleasesArrayOfVlenElements()
    {
        Native.NcVlen[] vlens = new Native.NcVlen[2]
        {
            new() { len = 3, p = Marshal.AllocHGlobal(3 * sizeof(int)) },
            new() { len = 2, p = Marshal.AllocHGlobal(2 * sizeof(int)) }
        };

        InteropTestCommon.AssertSuccess(Native.nc_free_vlens((nuint)vlens.Length, vlens), "nc_free_vlens");
    }

    [Fact]
    public void FreeVlen_ReleasesSingleVlenElement()
    {
        Native.NcVlen value = new()
        {
            len = 4,
            p = Marshal.AllocHGlobal(4 * sizeof(int))
        };

        InteropTestCommon.AssertSuccess(Native.nc_free_vlen(ref value), "nc_free_vlen");
    }

    [Fact]
    public void SetDefaultFormat_CanRoundTripOldValue()
    {
        const int ncFormatNetcdf4 = 3;
        int status = Native.nc_set_default_format(ncFormatNetcdf4, out int original);
        InteropTestCommon.AssertSuccessOrSkipIfFeatureUnavailable(status, "nc_set_default_format(set)");

        try
        {
            int verifyStatus = Native.nc_set_default_format(ncFormatNetcdf4, out int previous);
            InteropTestCommon.AssertSuccess(verifyStatus, "nc_set_default_format(verify)");
            Assert.Equal(ncFormatNetcdf4, previous);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_set_default_format(original, out _), "nc_set_default_format(restore)");
        }
    }

    [Fact]
    public void SetAndGetChunkCache_RoundTrip()
    {
        InteropTestCommon.AssertSuccess(Native.nc_get_chunk_cache(out nuint originalSize, out nuint originalElems, out float originalPreemption), "nc_get_chunk_cache(original)");

        nuint requestedSize = (nuint)2_097_152;
        nuint requestedElems = (nuint)4093;
        float requestedPreemption = 0.33f;

        InteropTestCommon.AssertSuccess(Native.nc_set_chunk_cache(requestedSize, requestedElems, requestedPreemption), "nc_set_chunk_cache");
        InteropTestCommon.AssertSuccess(Native.nc_get_chunk_cache(out nuint size, out nuint elems, out float preemption), "nc_get_chunk_cache");

        Assert.True(size >= requestedSize);
        Assert.True(elems >= requestedElems);
        Assert.Equal(requestedPreemption, preemption, 3);

        InteropTestCommon.AssertSuccess(Native.nc_set_chunk_cache(originalSize, originalElems, originalPreemption), "nc_set_chunk_cache(restore)");
    }

    [Fact]
    public void GetAttString_ReadsStringAttributeValues()
    {
        using NcTempFile hnd = new(NetcdfTestFormats.Netcdf4);

        string[] expected = ["north", "south", "east"];
        InteropTestCommon.AssertSuccess(
            Native.nc_put_att_string(hnd.Id, NcGlobal, "labels", (nuint)expected.Length, expected),
            "nc_put_att_string");

        IntPtr[] ptrs = new IntPtr[expected.Length];
        try
        {
            InteropTestCommon.AssertSuccess(Native.nc_get_att_string(hnd.Id, NcGlobal, "labels", ptrs), "nc_get_att_string");
            string[] actual = ptrs.Select(p => Marshal.PtrToStringAnsi(p) ?? string.Empty).ToArray();
            Assert.Equal(expected, actual);
        }
        finally
        {
            InteropTestCommon.AssertSuccess(Native.nc_free_string((nuint)ptrs.Length, ptrs), "nc_free_string");
        }
    }

    private static string DecodeCString(byte[] bytes)
    {
        int nul = Array.IndexOf(bytes, (byte)0);
        int len = nul >= 0 ? nul : bytes.Length;
        return Encoding.ASCII.GetString(bytes, 0, len);
    }
}
