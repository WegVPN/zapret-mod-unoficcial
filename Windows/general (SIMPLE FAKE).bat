@echo off
chcp 65001 >nul
title ZapretMod - SIMPLE FAKE
cd /d "%~dp0"

echo ============================================
echo   ZapretMod - SIMPLE FAKE Strategy
echo ============================================
echo.

bin\winws.exe --wf=l3 --dpi-desync=simple-fake --ports=443,80

pause
