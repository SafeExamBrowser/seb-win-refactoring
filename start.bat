@echo off
setlocal

set "ROOT_DIR=%~dp0"
set "APP_PATH=%ROOT_DIR%SafeExamBrowser.Runtime\bin\x64\Debug\SafeExamBrowser.exe"

if not exist "%APP_PATH%" (
    echo SafeExamBrowser.exe was not found at:
    echo   %APP_PATH%
    echo.
    echo Build the Debug x64 solution first, then run start.bat again.
    exit /b 1
)

start "" "%APP_PATH%" %*
