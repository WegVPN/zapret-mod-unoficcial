@echo off
chcp 65001 >nul
title ZapretMod - General Strategy
cd /d "%~dp0"

echo ============================================
echo   ZapretMod - Discord + YouTube + Telegram
echo ============================================
echo.

bin\winws.exe --wf=l3 --dpi-desync=fake --dpi-desync-autottls=1 --dpi-desync-fake-tls=oob --ports=443,80,8443

pause
