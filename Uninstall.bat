@echo off
setlocal
cd /d "%~dp0"
title ControlPad Auto-Start Removal

echo =======================================================
echo    ControlPad (GamepadDesktop) - Auto-Start Removal
echo =======================================================
echo.

echo [1/3] Calisan ControlPad sureci durduruluyor...
taskkill /f /im GamepadDesktop.exe >nul 2>&1

echo [2/3] Windows Baslangic kaydi siliniyor...
reg delete "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "GamepadDesktopController" /f >nul 2>&1

echo [3/3] Gorev Zamanlayici kaydi siliniyor...
schtasks /delete /tn "GamepadDesktopController" /f >nul 2>&1

echo.
echo =======================================================
echo  ControlPad baslangic kayitlari temizlendi ve durduruldu.
echo =======================================================
echo.
timeout /t 3
