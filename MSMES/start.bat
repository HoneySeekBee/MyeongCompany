@echo off
title MSMES
cd /d "%~dp0"
taskkill /IM "MSMES.Web.exe" /F >nul 2>&1

:: Start server in background
start "MSMES-Server" src\MSMES.Web\bin\Release\net8.0\MSMES.Web.exe --urls "http://localhost:5000"

:: Wait until port 5000 is actually listening
:check
timeout /t 1 >nul
powershell -Command "try{(New-Object Net.Sockets.TcpClient('localhost',5000)).Close();exit 0}catch{exit 1}" >nul 2>&1
if errorlevel 1 goto check

:: Port is open, launch browser
start http://localhost:5000
