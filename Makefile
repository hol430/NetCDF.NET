SLN=src/NetCDF.NET.slnx

.PHONY: clean build check test coverage

build:
	dotnet build $(SLN)

clean:
	dotnet clean $(SLN)
	rm -rf coverage src/NetCDF.NET.Tests/TestResults publish

check:
	dotnet test --collect:"XPlat Code Coverage" $(SLN)

test: check

# dotnet tool install -g dotnet-reportgenerator-globaltool
coverage: check
	reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
	xdg-open coverage/index.html
