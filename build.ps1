# Build script for Tuanjie WebGL
$ErrorActionPreference = "Stop"

$ProjectPath = "D:\game\unityGame\first game\My project"
$BuildLog = "$ProjectPath\WebGL\build.log"
$EditorPath = "C:\Program Files\Tuanjie\Hub\Editor\2022.3.61t11\Editor\Tuanjie.exe"

# Kill any running Unity/Tuanjie processes
Get-Process -Name "Tuanjie","Unity","bee_backend" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 3

# Clean Library and Temp
Remove-Item -Path "$ProjectPath\Library" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$ProjectPath\Temp" -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path "$ProjectPath\Library" -ItemType Directory -Force | Out-Null
New-Item -Path "$ProjectPath\Temp" -ItemType Directory -Force | Out-Null

Write-Host "Starting build..."
& $EditorPath -quit -batchmode -nographics `
    -projectPath $ProjectPath `
    -buildTarget WebGL `
    -executeMethod BuildScript.BuildWebGL `
    -logFile $BuildLog

if ($LASTEXITCODE -eq 0) {
    Write-Host "BUILD SUCCESS"
} else {
    Write-Host "BUILD FAILED with code $LASTEXITCODE"
    Get-Content $BuildLog | Select-Object -Last 30
}
