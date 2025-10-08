@echo off
echo Building Galactic Commander...
dotnet clean GalacticCommander.csproj
dotnet restore GalacticCommander.csproj
dotnet build GalacticCommander.csproj --configuration Release --verbosity quiet
if %ERRORLEVEL% EQU 0 (
    echo Build successful! Starting game...
    dotnet run --project GalacticCommander.csproj --configuration Release
) else (
    echo Build failed with errors.
    pause
)