@echo off
setlocal enabledelayedexpansion

echo ============================================
echo   ZapretGUI Build Script
echo ============================================
echo.

REM Check for .NET SDK
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERROR] .NET SDK not found. Please install .NET 8 SDK.
    echo Download: https://dotnet.microsoft.com/download/dotnet/8.0
    exit /b 1
)

echo [INFO] .NET SDK found
echo.

REM Clean previous build
echo [1/4] Cleaning previous build...
dotnet clean ZapretGUI/ZapretGUI.csproj -c Release
if errorlevel 1 (
    echo [WARN] Clean failed, continuing anyway...
)

REM Restore packages
echo [2/4] Restoring NuGet packages...
dotnet restore ZapretGUI/ZapretGUI.csproj
if errorlevel 1 (
    echo [ERROR] Package restore failed
    exit /b 1
)

REM Build
echo [3/4] Building Release configuration...
dotnet build ZapretGUI/ZapretGUI.csproj -c Release --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed
    exit /b 1
)

REM Publish
echo [4/4] Publishing application...
dotnet publish ZapretGUI/ZapretGUI.csproj -c Release -r win-x64 --self-contained false -o ZapretGUI\bin\Release\net8.0-windows\publish
if errorlevel 1 (
    echo [ERROR] Publish failed
    exit /b 1
)

echo.
echo ============================================
echo   Build completed successfully!
echo ============================================
echo.
echo Output directory: ZapretGUI\bin\Release\net8.0-windows\publish
echo.
echo To create installer:
echo   1. Install Inno Setup 6 from https://jrsoftware.org/isdl.php
echo   2. Open ZapretGUI\Installer.iss in Inno Setup Compiler
echo   3. Build the installer
echo.

endlocal
