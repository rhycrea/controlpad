$ErrorActionPreference = "Stop"
$dir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$cs = Join-Path $dir "GamepadDesktop.cs"
$exe = Join-Path $dir "GamepadDesktop.exe"
$configXml = Join-Path $dir "GamepadDesktop.exe.config"
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

Stop-ScheduledTask -TaskName "GamepadDesktopController" -ErrorAction SilentlyContinue
$oldProcs = Get-Process -Name "GamepadDesktop" -ErrorAction SilentlyContinue
if ($oldProcs) {
    $oldProcs | Stop-Process -Force -ErrorAction SilentlyContinue
    $oldProcs | Wait-Process -Timeout 15 -ErrorAction SilentlyContinue
}
Start-Sleep -Milliseconds 300

# Write .NET Corrupted State Exceptions policy to prevent driver SEH crash
$appConfigContent = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <runtime>
    <legacyCorruptedStateExceptionsPolicy enabled="true"/>
  </runtime>
</configuration>
'@
[System.IO.File]::WriteAllText($configXml, $appConfigContent, [System.Text.Encoding]::UTF8)

& $csc /target:winexe /optimize+ /out:$exe /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll $cs
if ($LASTEXITCODE -ne 0) {
    throw "Derleme basarisiz oldu!"
}

Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "GamepadDesktopController" -Value "`"$exe`""

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
$action = New-ScheduledTaskAction -Execute $exe -WorkingDirectory $dir
$trigger = New-ScheduledTaskTrigger -AtLogOn -User $env:USERNAME
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -ExecutionTimeLimit ([TimeSpan]::Zero) -RestartCount 3 -RestartInterval (New-TimeSpan -Minutes 1)

if ($isAdmin) {
    $principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Highest
    Register-ScheduledTask -TaskName "GamepadDesktopController" -Action $action -Trigger $trigger -Settings $settings -Principal $principal -Force | Out-Null
} else {
    Register-ScheduledTask -TaskName "GamepadDesktopController" -Action $action -Trigger $trigger -Settings $settings -Force | Out-Null
}

try {
    Start-ScheduledTask -TaskName "GamepadDesktopController" -ErrorAction SilentlyContinue
} catch {}

Start-Sleep -Seconds 1
$running = Get-Process -Name "GamepadDesktop" -ErrorAction SilentlyContinue
if (-not $running) {
    Start-Process -FilePath $exe -WorkingDirectory $dir
    Start-Sleep -Seconds 1
}

$proc = Get-Process -Name "GamepadDesktop" -ErrorAction SilentlyContinue
if ($proc) {
    Write-Host "GamepadDesktop basariyla baslatildi! PID: $($proc.Id), RAM: $([math]::Round($proc.WorkingSet64 / 1MB, 2)) MB" -ForegroundColor Green
} else {
    Write-Warning "GamepadDesktop baslatilamadi, lutfen kontrol edin."
}
