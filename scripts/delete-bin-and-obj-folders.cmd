@echo off
color 06
cls
echo Deletes the 'BIN' and 'OBJ' folders from each of the repository projects
echo that are located inside the 'SRC', 'TESTS' and 'SAMPLES' folders, resp.
echo.
echo Created by Oscar Fernandez Gonzalez a.k.a. Osc@rNET  (OscarNET-SOFTware)
echo ------------------------------------------------------------------------
echo.

setlocal enabledelayedexpansion
set _ScriptsDir=%~dp0
set _RepoDir=%_ScriptsDir:scripts\=%
set _SamplesDir=%_RepoDir%samples\
set _SourceDir=%_RepoDir%src\
set _TestsDir=%_RepoDir%tests\
set _ThemesDir=%_RepoDir%themes\

:DELETE_BIN_FOLDERS
set _CurrentFolder=BIN
set _NextStep=DELETE_OBJ_FOLDERS
goto DELETE_FOLDERS

:DELETE_OBJ_FOLDERS
set _CurrentFolder=OBJ
set _NextStep=END
goto DELETE_FOLDERS

:DELETE_FOLDERS
echo.
echo DELETING THE FOLLOWING '%_CurrentFolder%' FOLDERS:
echo.
set _FolderExists=false
for %%d in (%_SamplesDir%, %_SourceDir%, %_TestsDir%, %_ThemesDir%) do (
    for /f "tokens=*" %%a in ('dir %%d /s /b /o:n /ad ^| findstr /i "\\%_CurrentFolder%$"') do (
        set _FolderExists=true
        set _=%%a
        echo    [x]   ..\!_:%_RepoDir%=!
        rmdir /s /q %%a
        set _=
    )
)
if "%_FolderExists%" == "false" (
    echo    [-]   There are currently no '%_CurrentFolder%' folders to delete.
)
echo.
goto %_NextStep%

:END
endlocal
echo.
echo Press any key to end . . .
pause > nul
color
cls
