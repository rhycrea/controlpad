# Console & Gamepad Desktop Controller — Agent Instructions (`C:\ws\agents\console\AGENTS.md`)

> **Scope**: Project instructions for `C:\ws\agents\console` (`GamepadDesktop`), Aybars's custom zero-dependency C# / Win32 console-like PC desktop & Playnite controller.
> **Skill Binding**: Bound to the global **`pc-master`** skill (`C:\Users\aybar\.agents\skills\pc-master\SKILL.md`).

---

## 1. Project Overview & Architecture

`GamepadDesktop` turns Aybars's Windows 11 PC into a seamless couch/console experience using any **XInput** (`xinput1_4.dll` / `xinput9_1_0.dll` / `xinput1_3.dll`) or **DirectInput / WinMM** (`winmm.dll` `joyGetPosEx`, e.g., Logitech G29 wheel or generic gamepads) controller, with native **Playnite** window and mode management.

| File | Purpose |
| :--- | :--- |
| `GamepadDesktop.cs` | Single-file C# WinForms + P/Invoke source (`NativeMethods`, `ToastOverlayForm`, `HelpOverlayForm`, `GamepadDesktopContext`). |
| `GamepadDesktop.exe` | Compiled native 64-bit Windows executable (`winexe`, zero external NuGet/DLL dependencies). |
| `config.ini` | Live-reloadable user configuration (cursor speeds, deadzones, response curve, poll interval, startup mode). |
| `install_or_rebuild.ps1` | Stops the running instance, compiles `GamepadDesktop.cs` via `.NET Framework64 csc.exe`, updates `HKCU\...\Run` and the `GamepadDesktopController` Scheduled Task, and restarts the service. |

---

## 2. Controller Button & Combo Reference

### 2.1 Mode & Safety Barrier (Zero Game Interference)
- **`Start + Select` (Hold 1 sec)**: Toggle **Desktop Mode ON / OFF** (with non-focus-stealing Toast notification and audio feedback).
- **When OFF (Game Mode)**: **Zero inputs are intercepted or sent** so active games or Playnite Fullscreen are never disrupted.
- **Wait-for-Release Barrier**: Toggling mode ON or OFF waits until all held buttons are released before processing actions, preventing stray `Enter` or `Alt+Tab` leaks.
- **`LB + RB` (Press together)**: Toggle the non-focus-stealing **Visual Button Cheatsheet HUD Overlay (`HelpOverlayForm`)**.

### 2.2 Mouse & Click-and-Drag
- **Left Stick / Wheel**: Mouse cursor movement (power curve `2.15`, sub-pixel accumulation).
- **Right Stick**: Vertical & Horizontal smooth scrolling.
- **`A` (Hold / Tap)**: Left Click & native **Click-and-Drag** while held.
- **`LB + A`**: Toggle **Drag Lock** (hold left mouse button without keeping `A` pressed).
- **`B` / `X`**: Right Click / Middle Click (open link in new tab).
- **`LT` (Held)**: Precision / Sniper slow cursor mode (`0.28x`).
- **`RT` (Held)**: Turbo fast cursor mode (`2.40x`).

### 2.3 Playnite & Window Management
- **`RB + Start`**: Bring **Playnite** to front / restore from minimized or tray.
- **`RB + Y`**: Switch Playnite between **Fullscreen Mode (`Playnite.FullscreenApp.exe`)** and **Desktop Mode (`Playnite.DesktopApp.exe`)**.
- **`RB + B`**: Minimize active window / Playnite.
- **`Select` (Hold)**: Open `Alt + Tab` switcher (navigate with `LB`/`RB`/D-Pad, release `Select` to focus window).
- **`Select` (Single Tap)**: Quick `Alt + Tab` to previous window.

### 2.4 Keyboard, Tabs & Media
- **`LB` / `RB` (Single Tap)**: Previous Tab (`Ctrl+Shift+Tab`) / Next Tab (`Ctrl+Tab`).
- **`Y`**: Toggle Windows On-Screen Keyboard (`osk.exe`).
- **`Start` (Single Tap)**: `Enter` key.
- **D-Pad**: Up / Down / Left / Right arrow keys (with auto-repeat).
- **`LB + D-Pad Up/Down`**: System Volume Up / Down.
- **`L3` (Left Thumb Click)**: `Escape` (`Esc`).
- **`R3` (Right Thumb Click)**: Windows Task View (`Win + Tab`).

---

## 3. Build, Deploy & Verification Workflow

Whenever modifying `GamepadDesktop.cs` or `config.ini`, ALWAYS compile and restart using:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\ws\agents\console\install_or_rebuild.ps1"
```

Verify that the process is running from `C:\ws\agents\console\GamepadDesktop.exe`:

```powershell
Get-Process -Name "GamepadDesktop" | Select-Object Id, ProcessName, Path
```
