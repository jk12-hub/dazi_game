@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo ============================================
echo   打字逃生游戏 - 一键构建部署
echo ============================================
echo.

echo [1/4] 杀掉旧进程...
taskkill /f /im Tuanjie.exe >nul 2>&1
taskkill /f /im Unity.exe >nul 2>&1
taskkill /f /im bee_backend.exe >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/4] 清理旧缓存...
rmdir /s /q "Library" >nul 2>&1
rmdir /s /q "Temp" >nul 2>&1
mkdir Library >nul 2>&1
mkdir Temp >nul 2>&1

echo [3/4] 构建 WebGL（约3-10分钟）...
set "EDITOR=C:\Program Files\Tuanjie\Hub\Editor\2022.3.61t11\Editor\Tuanjie.exe"
set "LOG=WebGL\build.log"
rmdir /s /q WebGL >nul 2>&1
mkdir WebGL >nul 2>&1

"%EDITOR%" ^
    -quit ^
    -batchmode ^
    -nographics ^
    -projectPath "%~dp0." ^
    -buildTarget WebGL ^
    -executeMethod BuildScript.BuildWebGL ^
    -logFile "%LOG%"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 构建失败，错误信息：
    type "%LOG%" | findstr /c:"Error" /c:"error CS" /c:"Aborting"
    echo.
    echo 完整日志：%LOG%
    pause
    exit /b 1
)

echo    ✅ 构建成功！
echo.
echo [4/4] 推送到 GitHub...
git add WebGL\ -f
git commit -m "deploy: WebGL build" >nul 2>&1
git push origin main

echo.
echo ============================================
echo   ✅ 全部完成！
echo   游戏地址: https://jk12-hub.github.io/dazi_game/WebGL/
echo ============================================
pause
