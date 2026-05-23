@echo off
color 02
cls
echo Runs test projects with code coverage.
echo.
echo Created by Oscar Fernandez Gonzalez a.k.a. Osc@rNET  (OscarNET-SOFTware)
echo ------------------------------------------------------------------------
echo.

setlocal enabledelayedexpansion
set _ScriptsDir=%~dp0
set _RepoDir=%_ScriptsDir:scripts\=%
set _SourceDir=%_RepoDir%src\
set _TestsDir=%_RepoDir%tests\
set _TestsResultsDir=%_RepoDir%TestResults\
set _TestsResultsCoverageDir=%_TestsResultsDir%coverage\
set _TestsResultsHistoryDir=%_TestsResultsDir%history\
set _TestsResultsOutputsDir=%_TestsResultsDir%outputs\
set _DotNetConfiguration=Debug
set _DotNetRuntime=win-x64
set _SolutionFile=%_RepoDir%Retro.TUI.Framework.slnx

:: ReportGenerator is consumed as a NuGet package (coverlet.collector + ReportGenerator).
:: The version and TFM are resolved dynamically from the local NuGet cache so that
:: this script does not need to be updated when Directory.Packages.props changes.
for /f "tokens=*" %%v in ('dotnet list %_SolutionFile% package --include-transitive 2^>nul ^| findstr /i "ReportGenerator"') do (
    for %%t in (net10.0 net9.0 net8.0) do (
        if not defined _ReportGeneratorPath (
            for /f "tokens=3" %%p in ("%%v") do (
                set _CandidatePath=%UserProfile%\.nuget\packages\reportgenerator\%%p\tools\%%t\ReportGenerator.dll
                if exist !_CandidatePath! set _ReportGeneratorPath=!_CandidatePath!
            )
        )
    )
)
:: Fallback: if dynamic resolution fails, use the version pinned in Directory.Packages.props.
if not defined _ReportGeneratorPath (
    echo    [!]   Dynamic ReportGenerator resolution failed. Using pinned fallback.
    for %%t in (net10.0 net9.0 net8.0) do (
        if not defined _ReportGeneratorPath (
            set _CandidatePath=%UserProfile%\.nuget\packages\reportgenerator\5.5.10\tools\%%t\ReportGenerator.dll
            if exist !_CandidatePath! set _ReportGeneratorPath=!_CandidatePath!
        )
    )
)

:CREATE_TEST_RESULTS_FOLDERS_IF_APPLICABLE
if not exist %_TestsResultsDir% mkdir %_TestsResultsDir%
if not exist %_TestsResultsHistoryDir% mkdir %_TestsResultsHistoryDir%

:DELETE_OLD_TEST_RESULTS
echo.
echo DELETING OLD RESULTS . . .
echo.
if exist %_TestsResultsCoverageDir% rmdir /s /q %_TestsResultsCoverageDir%
if exist %_TestsResultsOutputsDir% rmdir /s /q %_TestsResultsOutputsDir%

:RUN_TESTS
echo.
echo TESTING THE FOLLOWING PROJECTS:
echo.
for /f "tokens=*" %%a in ('dir %_TestsDir%\*.tests.csproj /s /b /o:n /a-d') do (
    echo    [x]   %%~nxa
)
echo.
dotnet test %_SolutionFile% ^
    --configuration %_DotNetConfiguration% ^
    --collect "XPlat Code Coverage" ^
    --results-directory %_TestsResultsOutputsDir% ^
    --runtime %_DotNetRuntime% ^
    --verbosity quiet ^
    -- RunConfiguration.MaxCpuCount=0 ^
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.DeterministicReport=true ^
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByAttribute=CompilerGeneratedAttribute,ExcludeFromCodeCoverageAttribute,GeneratedCodeAttribute,Obsolete ^
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

:GENERATE_CODE_COVERAGE_REPORT
echo.
echo GENERATING THE CODE COVERAGE REPORT . . .
echo.
if not defined _ReportGeneratorPath (
    echo    [!]   ReportGenerator.dll could not be located. Skipping report generation.
    echo    [!]   Restore NuGet packages first: dotnet restore
    goto OPEN_CODE_COVERAGE_REPORT_IF_APPLICABLE
)
dotnet %_ReportGeneratorPath% ^
    -reports:%_TestsResultsOutputsDir%**\coverage.cobertura.xml ^
    -sourcedirs:%_SourceDir% ^
    -targetdir:%_TestsResultsCoverageDir% ^
    -historydir:%_TestsResultsHistoryDir% ^
    -reporttypes:HTML;HTMLSummary;Cobertura ^
    -title:Retro.TUI.Framework ^
    -verbosity:Error

:OPEN_CODE_COVERAGE_REPORT_IF_APPLICABLE
echo.
if exist %_TestsResultsCoverageDir%index.html (
    echo OPENING THE CODE COVERAGE REPORT USING DEFAULT WEB BROWSER . . .
    start %_TestsResultsCoverageDir%index.html
) else (
    echo The code coverage report could not be generated.
)
echo.

:END
endlocal
echo.
echo Press any key to end . . .
pause > nul
color
cls
