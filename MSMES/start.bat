@echo off
title MSMES
cd /d "%~dp0"
taskkill /IM "MSMES.Web.exe" /F >nul 2>&1

:: Start server from source directory (needs wwwroot + appsettings.json)
start "MSMES-Server" /D "%~dp0src\MSMES.Web" "%~dp0src\MSMES.Web\bin\Release\net8.0\MSMES.Web.exe" --urls "http://localhost:5000"

:: Wait until port 5000 is actually listening, then open browser
:check
timeout /t 1 >nul
powershell -Command "try{(New-Object Net.Sockets.TcpClient('localhost',5000)).Close();exit 0}catch{exit 1}" >nul 2>&1
if errorlevel 1 goto check

start http://localhost:5000
