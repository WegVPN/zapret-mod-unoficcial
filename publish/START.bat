@echo off
chcp 65001 >nul
title ZapretMod Launcher
cd /d "%~dp0"

echo ════════════════════════════════════════════
echo              ZapretMod Launcher
echo ════════════════════════════════════════════
echo.

REM Check if running as admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] Please run as Administrator!
    echo.
    echo Right-click on ZapretMod.exe and select
    echo "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo [+] Running as Administrator...
echo.

REM Check for .NET 8
dotnet --list-runtimes 2>nul | findstr /C:"Microsoft.WindowsDesktop.App" >nul
if %errorLevel% neq 0 (
    echo [!] .NET 8 Desktop Runtime not found!
    echo.
    echo Please install from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    pause
    exit /b 1
)

echo [+] .NET 8 Runtime found
echo.

REM Check for binaries
if not exist "bin\winws.exe" (
    echo [!] Binaries not found!
    echo.
    echo Downloading required files...
    call download-binaries.bat
    echo.
)

echo [+] Starting ZapretMod...
echo.

start ZapretMod.exe
exit
