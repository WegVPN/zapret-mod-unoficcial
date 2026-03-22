@echo off
chcp 65001 >nul
title ZapretMod - Download Binaries
cd /d "%~dp0"

echo ════════════════════════════════════════════
echo         ZapretMod - Download Binaries
echo ════════════════════════════════════════════
echo.

set BIN_DIR=bin
if not exist "%BIN_DIR%" mkdir "%BIN_DIR%"

echo Downloading winws.exe...
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://github.com/bol-van/zapret-win-bundle/releases/latest/download/winws.exe' -OutFile '%BIN_DIR%\winws.exe'}"

echo Downloading WinDivert64.sys...
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://github.com/bol-van/zapret-win-bundle/releases/latest/download/WinDivert64.sys' -OutFile '%BIN_DIR%\WinDivert64.sys'}"

echo Downloading WinDivert64.dll...
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://github.com/bol-van/zapret-win-bundle/releases/latest/download/WinDivert64.dll' -OutFile '%BIN_DIR%\WinDivert64.dll'}"

echo.
echo ════════════════════════════════════════════
echo   Download Complete!
echo ════════════════════════════════════════════
echo.
echo Files downloaded to: %CD%\%BIN_DIR%
echo.
echo You can now run ZapretMod.exe
echo.
pause
