@echo off
chcp 65001 >nul
title ZapretMod v3.0
cd /d "%~dp0"

echo ════════════════════════════════════════
echo         ZapretMod v3.0 Launcher
echo ════════════════════════════════════════
echo.

REM Check admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] Запустите от имени администратора!
    echo.
    pause
    exit /b 1
)

echo [+] Запуск ZapretMod...
echo.

start ZapretMod.exe
exit
