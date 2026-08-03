@echo off
setlocal

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\set-platform-operations-key.ps1" -ReuseExisting
if errorlevel 1 (
    echo Unable to copy the Platform Operations access key.
    pause
    exit /b 1
)

echo Paste the clipboard value into Platform Operations access.
timeout /t 3 /nobreak >nul
