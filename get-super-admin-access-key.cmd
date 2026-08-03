@echo off
setlocal

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\set-super-admin-key.ps1" -ReuseExisting
if errorlevel 1 (
    echo Unable to copy the Super Admin access key.
    pause
    exit /b 1
)

echo Paste the clipboard value into Super Admin access.
timeout /t 3 /nobreak >nul
