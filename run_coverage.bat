dotnet test -- --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --coverage-settings coverage.xml
dotnet ReportGenerator -reports:TheDialgaTeam.Pokemon3D.Server.Test\bin\Debug\net10.0\TestResults\coverage.cobertura.xml -targetdir:CoverageReport
start CoverageReport\index.html