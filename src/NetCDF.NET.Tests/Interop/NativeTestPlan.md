# Native Binding Test Plan (Round-Trip V1)

**Important**: All created files should be managed via a `TempFile` helper with
`IDisposable` pattern to ensure cleanup.

## 1. Scope and Phasing

Do not require “every native function” in the first pass. Start with a curated,
core supported surface and expand.

### P0 (done)

- File lifecycle: `nc_create`, `nc_open`, `nc_close`, `nc_sync`, `nc_enddef`
- Basic metadata inquiry: `nc_inq`, `nc_inq_ndims`, `nc_inq_nvars`
- Dimensions: define/inquire/id/length
- Variables: define, `nc_inq_varid`, primitive type inquiry where stable
- Attributes (core scalar/short-vector cases)
- Data round-trip for core primitive types (`int`, `float`, `double`, etc.)
- Core error behavior (missing file, invalid ids/names)

### P1 (done)

- Groups (`nc_def_grp`, `nc_inq_grps`, parent/child lookups)
- Unlimited-dimension multi-dim behavior
- Chunking/cache/filter inquiry paths (where library support exists)
- More attribute/data type combinations

### P2 (done)

- User-defined types (`compound`, `enum`, `vlen`)
- Any API currently marked untested/incomplete in `Native.cs`
- Highly version/backend-specific paths not stable across installs

## 2. Round-Trip Principle (No Golden Files in V1) (done)

For this phase, validate with round-trip correctness only.

Pattern:

```csharp
// write path
nc_create(...)
nc_def_dim(...)
nc_def_var(...)
nc_put_var*(...)
nc_close(...)

// read/query path
nc_open(...)
nc_inq*(...)
nc_inq_dim*(...)
nc_inq_var*(...)
nc_get_var*(...)
```

This is acceptable and expected for interop validation.

## 3. Verify Semantic Facts, Not Bytes (done)

Do not compare NetCDF files byte-for-byte. Verify semantic facts:

- number of dimensions
- dimension names and lengths
- variable names, types, and shapes
- attribute names, types, and values
- data values round-trip correctly

## 4. Required Pattern for Count+Array APIs (done)

For APIs that take `(out count, arrayPtr)` (for example `nc_inq_unlimdims`,
`nc_inq_grps`, and similar patterns):

1. First call with array argument `null` to get count.
2. Allocate managed array with exact count.
3. Second call to fill the array.
4. Assert count and contents.

This pattern is mandatory in tests because it catches pointer/array marshaling
mistakes.

## 5. Negative Tests (done)

Negative tests are high-value for binding correctness.

Examples:

- `nc_open("missing.nc", ...)` returns non-zero expected failure.
- invalid `ncid` to inquiry/close paths returns non-zero.
- invalid dimension/variable names return expected lookup failures.

Guidance:

- Assert specific error codes where behavior is stable.
- Where code may vary by backend/version, assert `status != NC_NOERR` and
  include `nc_strerror(status)` in assertion messages.

## 6. Environment/Feature Gating (done)

Tests must distinguish:

- binding failure vs
- feature unavailable in installed NetCDF library.

Rules:

- Skip feature-specific tests with clear reason when unsupported.
- Keep P0 tests runnable on a typical netCDF install.
- Gate netCDF-4/group/filter-specific tests if the runtime lacks support.

## 7. Test Structure

Keep interop tests low-level and mechanically close to the native surface.

Suggested files:

- `Interop/CreateOpenCloseTests.cs`
- `Interop/DimensionTests.cs`
- `Interop/VariableTests.cs`
- `Interop/AttributeTests.cs`
- `Interop/DataRoundTripTests.cs`
- `Interop/ErrorBehaviorTests.cs`
- `Interop/GroupTests.cs` (P1)

## 8. Determinism and Isolation

- One temp file per test.
- No shared mutable files between tests.
- No test ordering assumptions.
- Always cleanup in `Dispose`/`finally`.

## 9. Best Practical V1 Goal

For now, use:

1. Round-trip tests through bindings.
2. Strong negative tests.
3. Feature-gated coverage expansion from P0 to P1.

Golden files are intentionally out of scope for this phase.
