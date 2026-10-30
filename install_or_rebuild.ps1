$ErrorActionPreference = "Stop"
$dir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$cs = Join-Path $dir "GamepadDesktop.cs"
$exe = Join-Path $dir "GamepadDesktop.exe"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

Stop-ScheduledTask -TaskName "GamepadDesktopController" -ErrorAction SilentlyContinue
$oldProcs = Get-Process -Name "GamepadDesktop" -ErrorAction SilentlyContinue
if ($oldProcs) {
    $oldProcs | Stop-Process -Force -ErrorAction SilentlyContinue
    $oldProcs | Wait-Process -Timeout 15 -ErrorAction SilentlyContinue
}
Start-Sleep -Milliseconds 300

& $csc /target:winexe /optimize+ /out:$exe /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll $cs
if ($LASTEXITCODE -ne 0) {
    throw "Derleme basarisiz oldu!"
}

Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "GamepadDesktopController" -Value "`"$exe`""

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
$action = New-ScheduledTaskAction -Execute $exe -WorkingDirectory $dir
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit ([TimeSpan]::Zero) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)

if ($isAdmin) {
    $principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Highest
    Register-ScheduledTask -TaskName "GamepadDesktopController" -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Force | Out-Null
} else {
    Register-ScheduledTask -TaskName "GamepadDesktopController" -Action $action -Trigger $trigger -Settings $settings -Force | Out-Null
}

Start-ScheduledTask -TaskName "GamepadDesktopController"
Start-Sleep -Seconds 1
Write-Host "GamepadDesktop (C:\ws\agents\console) basariyla derlendi ve interaktif masaustu oturumunda baslatildi!" -ForegroundColor Green
