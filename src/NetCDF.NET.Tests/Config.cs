// Not ideal, but netcdf doesn't seem to be thread safe. This lets us run our
// tests without race conditions. Should revisit this at some point.
[assembly: CollectionBehavior(MaxParallelThreads = 1)]
