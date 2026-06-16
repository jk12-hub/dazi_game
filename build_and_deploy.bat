@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ============================================
echo   打字逃生游戏 - WebGL 一键构建部署
echo ============================================
echo.

:: 项目路径
set "PROJECT_DIR=%~dp0"
cd /d "%PROJECT_DIR%"

:: 查找 Tuanjie Editor
set "EDITOR_PATH="

:: 1. 检查 Tuanjie Hub 默认安装路径
for /d %%d in ("C:\Program Files\Unity\Hub\Editor\*t11*") do (
    if exist "%%d\Editor\Tuanjie.exe" set "EDITOR_PATH=%%d\Editor\Tuanjie.exe"
)
for /d %%d in ("C:\Program Files\Tuanjie*") do (
    if exist "%%d\Editor\Tuanjie.exe" set "EDITOR_PATH=%%d\Editor\Tuanjie.exe"
)

:: 2. 检查标准 Unity 路径
if "%EDITOR_PATH%"=="" (
    for /d %%d in ("C:\Program Files\Unity\Hub\Editor\2022.3*") do (
        if exist "%%d\Editor\Unity.exe" set "EDITOR_PATH=%%d\Editor\Unity.exe"
    )
)

:: 3. 扫描 D 盘
if "%EDITOR_PATH%"=="" (
    for /r "D:\" %%f in (Tuanjie.exe) do set "EDITOR_PATH=%%f"
)
if "%EDITOR_PATH%"=="" (
    for /r "D:\" %%f in (Unity.exe) do set "EDITOR_PATH=%%f"
)

echo [1/4] 查找编辑器...
if "%EDITOR_PATH%"=="" (
    echo.
    echo ❌ 找不到 Tuanjie 或 Unity 编辑器！
    echo.
    echo 请安装团结引擎后重试：
    echo   1. 打开 Tuanjie Hub
    echo   2. 安装版本 2022.3.61t11 （含 WebGL 模块）
    echo.
    echo   下载地址: https://unity.cn/tuanjie/releases
    echo.
    pause
    exit /b 1
)
echo    ✅ 找到: %EDITOR_PATH%
echo.

:: 清理旧构建
echo [2/4] 清理旧构建...
if exist "%PROJECT_DIR%WebGL" rmdir /s /q "%PROJECT_DIR%WebGL"
mkdir "%PROJECT_DIR%WebGL"
echo    ✅ 完成

:: 执行 WebGL 构建
echo [3/4] 构建 WebGL（约 2-10 分钟）...
echo.

set "BUILD_LOG=%PROJECT_DIR%WebGL\build.log"

"%EDITOR_PATH%" ^
    -quit ^
    -batchmode ^
    -nographics ^
    -projectPath "%PROJECT_DIR%" ^
    -buildTarget WebGL ^
    -executeMethod BuildScript.BuildWebGL ^
    -logFile "%BUILD_LOG%"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 构建失败！查看日志: WebGL\build.log
    type "%BUILD_LOG%" | findstr /i "error exception"
    echo.
    pause
    exit /b 1
)

echo    ✅ WebGL 构建完成！
echo.

:: 部署到 GitHub Pages
echo [4/4] 推送到 GitHub Pages...

git add WebGL/ -f
git commit -m "deploy: WebGL build %date% %time%" 2>nul
git push origin main

echo.
echo ============================================
echo   ✅ 部署完成！
echo   游戏地址: https://jk12-hub.github.io/dazi_game/WebGL/
echo ============================================
echo.
pause
