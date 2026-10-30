@echo off
setlocal
cd /d "%~dp0"
title ControlPad Auto-Start Setup

echo =======================================================
echo    ControlPad (GamepadDesktop) - Auto-Start Setup
echo =======================================================
echo.

set "EXE_PATH=%~dp0GamepadDesktop.exe"
if not exist "%EXE_PATH%" (
    echo [ERROR] GamepadDesktop.exe bulunamadi!
    echo Lutfen zip dosyasini cikardiginizdan emin olun.
    pause
    exit /b 1
)

echo [1/3] Calisan eski surecler durduruluyor...
taskkill /f /im GamepadDesktop.exe >nul 2>&1
schtasks /end /tn "GamepadDesktopController" >nul 2>&1

echo [2/3] Windows Baslangic kaydi (HKCU Run) ekleniyor...
reg add "HKCU\Software\Microsoft\Windows\CurrentVersion\Run" /v "GamepadDesktopController" /t REG_SZ /d "\"%EXE_PATH%\"" /f >nul
if %errorlevel% equ 0 (
    echo   - Kayit defteri basariyla guncellendi.
) else (
    echo   - [UYARI] Kayit defteri guncellenemedi.
)

echo [3/3] Gorev Zamanlayici ile baslangic gorevi tanimlaniyor...
schtasks /create /tn "GamepadDesktopController" /tr "\"%EXE_PATH%\"" /sc onlogon /rl highest /f >nul 2>&1
if %errorlevel% equ 0 (
    echo   - Gorev Zamanlayici (Yuksek Yetki / Oyun Uyumlulugu) basariyla ayarlandi.
) else (
    schtasks /create /tn "GamepadDesktopController" /tr "\"%EXE_PATH%\"" /sc onlogon /rl limited /f >nul 2>&1
    if %errorlevel% equ 0 (
        echo   - Gorev Zamanlayici (Standart Kullanici) basariyla ayarlandi.
    )
)

echo.
echo ControlPad baslatiliyor...
start "" "%EXE_PATH%"

echo.
echo =======================================================
echo  [BASARILI] ControlPad kuruldu ve arka planda baslatildi!
echo.
echo  - Windows her acildiginda otomatik olarak baslayacaktir.
echo  - Masaustu Modu: Start + Select (1 sn basili tutun)
echo  - Yardim / Rehber HUD: LB + RB
echo =======================================================
echo.
timeout /t 5
