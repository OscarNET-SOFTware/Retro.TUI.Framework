@echo off
setlocal EnableDelayedExpansion

:: ============================================================================
:: publish-nuget.cmd
:: Retro.TUI.Framework - NuGet publish script
::
:: Builds all publishable projects in Release configuration, packs them as
:: NuGet packages and pushes them to nuget.org.
::
:: Usage:
::   publish-nuget.cmd                  (prompts for API key interactively)
::   publish-nuget.cmd --api-key <KEY>  (non-interactive / CI)
::
:: Requirements:
::   - .NET 10 SDK installed and on PATH
::   - icon.png present at the repository root (128x128 px)
::   - All tests passing before running this script
::
:: Output:
::   .nupkg files are written to artifacts\nuget\ under the repo root.
::   That folder is created automatically if it does not exist.
::   Add artifacts\ to .gitignore to keep it out of source control.
:: ============================================================================

:: ----------------------------------------------------------------------------
:: Locate repository root (one level above scripts\)
:: ----------------------------------------------------------------------------
set "SCRIPT_DIR=%~dp0"
set "REPO_ROOT=%SCRIPT_DIR%.."
pushd "%REPO_ROOT%"
set "REPO_ROOT=%CD%"
popd

echo.
echo ========================================================================
echo Retro.TUI.Framework -- NuGet Publish
echo Created by Oscar Fernandez Gonzalez a.k.a. Osc@rNET  (OscarNET-SOFTware)
echo ========================================================================
echo.
echo  Repository : %REPO_ROOT%
echo.

:: ----------------------------------------------------------------------------
:: Parse arguments
:: ----------------------------------------------------------------------------
set "NUGET_API_KEY="

:parse_args
if "%~1"=="" goto args_done
if /i "%~1"=="--api-key" (
    set "NUGET_API_KEY=%~2"
    shift
    shift
    goto parse_args
)
shift
goto parse_args
:args_done

:: ----------------------------------------------------------------------------
:: Prompt for API key if not supplied
:: ----------------------------------------------------------------------------
if "!NUGET_API_KEY!"=="" (
    echo Enter your NuGet.org API key ^(input is visible - run from a private terminal^):
    echo.
    set /p "NUGET_API_KEY=API Key: "
    echo.
)

if "!NUGET_API_KEY!"=="" (
    echo ERROR: No API key provided. Aborting.
    exit /b 1
)

:: ----------------------------------------------------------------------------
:: Verify prerequisites
:: ----------------------------------------------------------------------------
if not exist "%REPO_ROOT%\icon.png" (
    echo ERROR: icon.png not found at repository root.
    echo        NuGet packaging requires a 128x128 px icon.
    echo        Place icon.png at: %REPO_ROOT%\icon.png
    exit /b 1
)

if not exist "%REPO_ROOT%\Retro.TUI.Framework.slnx" (
    echo ERROR: Solution file not found at: %REPO_ROOT%\Retro.TUI.Framework.slnx
    exit /b 1
)

:: ----------------------------------------------------------------------------
:: Read version from Directory.Build.props
:: ----------------------------------------------------------------------------
set "VERSION_PREFIX="
set "VERSION_SUFFIX="

for /f "tokens=3 delims=<>" %%A in ('findstr "VersionPrefix" "%REPO_ROOT%\Directory.Build.props"') do (
    set "VERSION_PREFIX=%%A"
)
for /f "tokens=3 delims=<>" %%A in ('findstr "VersionSuffix" "%REPO_ROOT%\Directory.Build.props"') do (
    set "VERSION_SUFFIX=%%A"
)

if "!VERSION_SUFFIX!"=="" (
    set "PACKAGE_VERSION=!VERSION_PREFIX!"
) else (
    set "PACKAGE_VERSION=!VERSION_PREFIX!-!VERSION_SUFFIX!"
)

echo  Version    : !PACKAGE_VERSION!
echo  Target     : https://api.nuget.org/v3/index.json
echo.

:: ----------------------------------------------------------------------------
:: Prepare output directory — always start clean
:: ----------------------------------------------------------------------------
set "NUPKG_DIR=%REPO_ROOT%\artifacts\nuget"

if exist "!NUPKG_DIR!" (
    echo   Cleaning previous artifacts...
    rmdir /s /q "!NUPKG_DIR!"
)
mkdir "!NUPKG_DIR!"

:: ----------------------------------------------------------------------------
:: Step 1 - Build (Release)
:: ----------------------------------------------------------------------------
echo ============================================================
echo  Step 1/3 -- Build (Release)
echo ============================================================
echo.

dotnet build "%REPO_ROOT%\Retro.TUI.Framework.slnx" ^
    --configuration Release ^
    --verbosity minimal

if errorlevel 1 (
    echo.
    echo ERROR: Build failed. Fix all errors before publishing.
    exit /b 1
)

:: ----------------------------------------------------------------------------
:: Projects to publish - in strict dependency order so NuGet resolves refs
:: ----------------------------------------------------------------------------
set PROJECTS=^
    src\Retro.TUI.Events\Retro.TUI.Events.csproj ^
    src\Retro.TUI.Theming\Retro.TUI.Theming.csproj ^
    src\Retro.TUI.Hosting\Retro.TUI.Hosting.csproj ^
    src\Retro.TUI.Rendering\Retro.TUI.Rendering.csproj ^
    src\Retro.TUI.Views\Retro.TUI.Views.csproj ^
    src\Retro.TUI.Core\Retro.TUI.Core.csproj ^
    src\Retro.TUI.Windows\Retro.TUI.Windows.csproj ^
    themes\Retro.TUI.Theme.PcTools9\Retro.TUI.Theme.PcTools9.csproj

:: ----------------------------------------------------------------------------
:: Step 2 - Pack
:: ----------------------------------------------------------------------------
echo.
echo ============================================================
echo  Step 2/3 -- Pack
echo ============================================================
echo.

for %%P in (%PROJECTS%) do (
    echo   Packing: %%P
    dotnet pack "%REPO_ROOT%\%%P" ^
        --configuration Release ^
        --no-build ^
        --output "!NUPKG_DIR!" ^
        -p:ContinuousIntegrationBuild=true ^
        -p:EmbedUntrackedSources=true

    if errorlevel 1 (
        echo.
        echo ERROR: Pack failed for %%P
        exit /b 1
    )
    echo   OK
    echo.
)

:: ----------------------------------------------------------------------------
:: List packages and ask for confirmation
:: ----------------------------------------------------------------------------
echo.
echo ============================================================
echo  Packages ready to publish:
echo ============================================================
echo.
dir /b "!NUPKG_DIR!\*.nupkg"
echo.
echo  Version : !PACKAGE_VERSION!
echo  Target  : https://api.nuget.org/v3/index.json
echo.
set /p "CONFIRM=Push all packages to nuget.org? [Y/N]: "
if /i "!CONFIRM!" neq "Y" (
    echo.
    echo Aborted by user.
    echo Packages are available at: !NUPKG_DIR!
    exit /b 0
)

:: ----------------------------------------------------------------------------
:: Step 3 - Push to nuget.org
:: ----------------------------------------------------------------------------
echo.
echo ============================================================
echo  Step 3/3 -- Push to nuget.org
echo ============================================================
echo.

for %%F in ("!NUPKG_DIR!\*.nupkg") do (
    echo   Pushing: %%~nxF
    dotnet nuget push "%%F" ^
        --api-key "!NUGET_API_KEY!" ^
        --source "https://api.nuget.org/v3/index.json" ^
        --skip-duplicate

    if errorlevel 1 (
        echo.
        echo ERROR: Push failed for %%~nxF
        echo        Check the API key and your NuGet.org account permissions.
        exit /b 1
    )
    echo   OK
    echo.
)

:: ----------------------------------------------------------------------------
:: Done
:: ----------------------------------------------------------------------------
echo.
echo ============================================================
echo  All packages published successfully.
echo  Version !PACKAGE_VERSION! is now live on nuget.org.
echo  https://www.nuget.org/profiles/OscarNET-SOFTware
echo ============================================================
echo.

endlocal
exit /b 0