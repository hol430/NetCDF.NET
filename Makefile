SLN=src/NetCDF.NET.slnx

.PHONY: clean build check mpi-test test coverage

build:
	dotnet build $(SLN)

clean:
	dotnet clean $(SLN)
	rm -rf coverage src/NetCDF.NET.Tests/TestResults publish

check:
	dotnet test --collect:"XPlat Code Coverage" $(SLN)

mpi-test:
	@if command -v mpiexec >/dev/null 2>&1; then \
		echo "Running MPI interop tests with mpiexec"; \
		NETCDF_MPI_TEST_FILE="/tmp/netcdf-dotnet-mpi-$$PPID-$$RANDOM.nc" mpiexec -n 2 dotnet test src/NetCDF.NET.Tests/NetCDF.NET.Tests.csproj --filter Category=MPI; \
	elif command -v mpirun >/dev/null 2>&1; then \
		echo "Running MPI interop tests with mpirun"; \
		NETCDF_MPI_TEST_FILE="/tmp/netcdf-dotnet-mpi-$$PPID-$$RANDOM.nc" mpirun -n 2 dotnet test src/NetCDF.NET.Tests/NetCDF.NET.Tests.csproj --filter Category=MPI; \
	else \
		echo "Skipping MPI tests (mpiexec/mpirun not found)."; \
	fi

test: check mpi-test

# dotnet tool install -g dotnet-reportgenerator-globaltool
coverage: check
	reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
	xdg-open coverage/index.html
