@echo off
title MSMES
cd /d "%~dp0"
taskkill /IM "MSMES.Web.exe" /F >nul 2>&1
start "" cmd /c "timeout /t 4 >nul && start http://localhost:5000"
src\MSMES.Web\bin\Release\net8.0\MSMES.Web.exe --urls "http://localhost:5000"
pause
