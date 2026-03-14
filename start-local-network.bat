@echo off
echo Starting Cricket Live Score Application...
echo.

echo Getting your local IP address...
for /f "tokens=2 delims=:" %%a in ('ipconfig ^| findstr /c:"IPv4 Address"') do (
    for /f "tokens=1" %%b in ("%%a") do (
        set LOCAL_IP=%%b
        goto :found
    )
)
:found

echo Your local IP: %LOCAL_IP%
echo.

echo Starting Backend API on %LOCAL_IP%:8080...
start "Cricket API" cmd /k "cd backend\CricketAPI && dotnet run --urls http://0.0.0.0:8080"

timeout /t 5 /nobreak > nul

echo Starting Frontend on %LOCAL_IP%:3001...
start "Cricket Frontend" cmd /k "cd frontend\cricket-app && set REACT_APP_API_URL=http://localhost:8080&& set PORT=3001&& npm start"

echo.
echo ========================================
echo Cricket Live Score App is starting...
echo.
echo Backend API: http://%LOCAL_IP%:8080
echo Frontend:    http://%LOCAL_IP%:3001
echo.
echo Access from any device on your network!
echo ========================================
pause