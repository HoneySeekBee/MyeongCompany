@echo off
title MSMES 서버 시작 중...
cd /d "%~dp0"

echo MSMES 서버를 시작합니다...
echo 잠시 후 브라우저가 자동으로 열립니다.
echo 서버를 종료하려면 이 창을 닫으세요.
echo.

:: 기존 프로세스 종료
taskkill /IM "MSMES.Web.exe" /F >nul 2>&1

:: 3초 후 브라우저 열기 (서버 기동 대기)
start "" cmd /c "timeout /t 3 >nul && start http://localhost:5000"

:: 서버 실행
dotnet run --project src\MSMES.Web\MSMES.Web.csproj --urls "http://localhost:5000"

pause
