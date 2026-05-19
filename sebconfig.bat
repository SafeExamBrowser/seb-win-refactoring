@echo off
setlocal

set "ROOT_DIR=%~dp0"
set "APP_PATH=%ROOT_DIR%SebWindowsConfig\bin\x64\Debug\SEBConfigTool.exe"

if not exist "%APP_PATH%" (
    echo SEBConfigTool.exe was not found at:
    echo   %APP_PATH%
    echo.
    echo Build the SebWindowsConfig Debug x64 project first, then run sebconfig.bat again.
    exit /b 1
)

start "" "%APP_PATH%" %*
