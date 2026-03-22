@echo off
chcp 65001 >nul
title ZapretMod Service Manager
cd /d "%~dp0"

:menu
cls
echo ============================================
echo   ZapretMod Service Manager
echo ============================================
echo.
echo 1. Install Service (Auto-start)
echo 2. Remove Service
echo 3. Start Service
echo 4. Stop Service
echo 5. Check Status
echo 6. Exit
echo.
set /p choice="Select option: "

if "%choice%"=="1" goto install
if "%choice%"=="2" goto remove
if "%choice%"=="3" goto start
if "%choice%"=="4" goto stop
if "%choice%"=="5" goto status
if "%choice%"=="6" goto exit
goto menu

:install
echo Installing service...
sc create "ZapretMod" binPath= "%cd%\ZapretMod.exe" start= auto DisplayName= "ZapretMod DPI Bypass Service"
sc description "ZapretMod" "Automatically starts zapret DPI bypass on system startup"
echo Service installed. Press any key to continue...
pause >nul
goto menu

:remove
echo Stopping and removing service...
sc stop "ZapretMod" 2>nul
sc delete "ZapretMod"
echo Service removed. Press any key to continue...
pause >nul
goto menu

:start
echo Starting service...
sc start "ZapretMod"
echo Service started. Press any key to continue...
pause >nul
goto menu

:stop
echo Stopping service...
sc stop "ZapretMod"
echo Service stopped. Press any key to continue...
pause >nul
goto menu

:status
echo Service status:
sc query "ZapretMod"
echo.
echo Press any key to continue...
pause >nul
goto menu

:exit
exit
