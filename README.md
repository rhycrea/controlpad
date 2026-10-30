# 🎮 ControlPad (`GamepadDesktop`)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](./LICENSE)
[![Platform: Windows 10/11](https://img.shields.io/badge/Platform-Windows%2010%2F11%20(x64)-0078D6.svg)](https://controlpad.lunapard.com)
[![Zero Dependencies](https://img.shields.io/badge/Dependencies-Zero%20(Native%20Win32%20%2B%20XInput)-10b981.svg)](https://controlpad.lunapard.com)
[![Live Showcase](https://img.shields.io/badge/Web-controlpad.lunapard.com-8b5cf6.svg)](https://controlpad.lunapard.com)

**ControlPad** is an ultra-lightweight (~40 KB), zero-dependency C# / Win32 system tray controller that transforms any Windows 10/11 PC into a seamless couch & console experience using any **Xbox / XInput** (`xinput1_4.dll`, `xinput9_1_0.dll`, `xinput1_3.dll`) or **DirectInput / WinMM** (`winmm.dll`) gamepad, with first-class **Playnite** integration and a non-focus-stealing HUD overlay.

🌐 **Live Product Page & Interactive Controller Map:** [https://controlpad.lunapard.com](https://controlpad.lunapard.com)  
🏢 **Part of the Lunapard Ecosystem:** [https://lunapard.com](https://lunapard.com) • Built by [Aybars Yüksel](https://aybars.lunapard.com)

---

## ✨ Why ControlPad?

- **Zero Game Interference (Hardware-Grade Safety Barrier):** Hold `Start + Select` for `1.0s` to toggle **Desktop Mode ON/OFF**. When **OFF (Game Mode)**, ControlPad intercepts **zero** inputs so active games, Steam Input, and Playnite Fullscreen work natively without double-input conflicts.
- **Wait-for-Release Barrier:** Mode transitions wait until all held buttons are physically released before emitting desktop actions, preventing accidental `Enter` or `Alt+Tab` leaks.
- **Sub-Pixel Power-Curve Cursor Physics (`125 Hz`):** Radial deadzone filtering (`14%` left stick, `18%` right stick) paired with a `2.15` power response curve and sub-pixel remainder accumulation—delivering both pixel-perfect precision (`LT` Sniper `0.28x`) and rapid multi-monitor traversal (`RT` Turbo `2.40x`).
- **Non-Focus-Stealing Visual HUD (`LB + RB`):** Press `LB + RB` at any time to summon a sleek, translucent controller cheat-sheet overlay (`WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW`) without stealing focus from your active window or video stream.
- **Zero External Dependencies:** Single-file C# source (`GamepadDesktop.cs`) that compiles in `< 1 second` using the built-in Windows `.NET Framework 64-bit csc.exe` compiler. No NuGet packages, no third-party runtimes, no background telemetry.

---

## 🎮 Controller Button & Shortcut Matrix

| Input / Combo | Desktop Action | Notes |
| :--- | :--- | :--- |
| **`Start + Select` (Hold 450ms)**<br>*(or **Guide / Xbox** hold)*<br>*(or **`LB + RB + Start`**)*<br>*(or **`L3 + R3`** hold)* | **Toggle Desktop Mode ON / OFF** | Non-focus-stealing Toast + hardware audio cue. Works reliably **even inside full-screen games** without conflicting with in-game controls. |
| **`LB + RB`** | **Toggle Visual HUD Cheat-Sheet** | Interactive dark-glass overlay |
| **Left Stick** | **Mouse Cursor Movement** | `2.15` response curve + sub-pixel precision |
| **Right Stick** | **Vertical & Horizontal Smooth Scroll** | Proportional wheel delta |
| **`A` (Tap / Hold)** | **Left Click / Native Click-and-Drag** | Hold `A` and move Left Stick to drag windows or select text |
| **`LB + A`** | **Toggle Drag Lock** | Latches Left Mouse Button without holding `A` |
| **`B`** | **Right Click** | Context menu |
| **`X`** | **Middle Click** | Open links in new browser tab / close tab |
| **`Y`** | **Toggle On-Screen Keyboard (`osk.exe`)** | Instant couch typing |
| **`LT` (Held)** | **Precision Sniper Mode (`0.28x`)** | Fine UI target selection |
| **`RT` (Held)** | **Turbo Fast Mode (`2.40x`)** | Fast cross-screen travel |
| **`Select` (Tap / Hold)** | **Quick `Alt + Tab` / Hold Switcher** | Hold `Select` + use `LB`/`RB`/D-Pad to pick window |
| **`Start` (Tap)** | **`Enter` Key** | Confirm dialog / submit |
| **`L3` / `R3` (Thumb Clicks)** | **`Escape` (`Esc`) / Task View (`Win + Tab`)** | Quick dismiss or virtual desktop overview |
| **`LB` / `RB` (Single Tap)** | **Previous / Next Browser Tab** | `Ctrl + Shift + Tab` / `Ctrl + Tab` |
| **`LB + D-Pad Up / Down`** | **System Master Volume Up / Down** | Media volume control |
| **`RB + Start`** | **Focus / Restore Playnite** | Brings Playnite to foreground from tray or minimized state |
| **`RB + Y`** | **Switch Playnite Fullscreen ↔ Desktop** | Seamlessly toggles `Playnite.FullscreenApp` & `Playnite.DesktopApp` |
| **`RB + B`** | **Minimize Active Window** | Clean desktop view |

---

## 🚀 Quick Start (Portable Download or 1-Click Build)

### 📦 Option 1: Download Portable Release (Recommended)
1. Download **`ControlPad-v1.0.0-Portable.zip`** from [GitHub Releases](https://github.com/rhycrea/controlpad/releases/latest).
2. Extract the archive anywhere on your PC (e.g., `C:\Tools\ControlPad`).
3. Double-click **`Install-Autostart.bat`** to enable automatic background startup and run it immediately!
   *(Or simply run `GamepadDesktop.exe` for a one-time portable session)*

---

### 🛠️ Option 2: Compile & Auto-Start via PowerShell (1 Second)
Every Windows 10/11 machine includes the native `.NET Framework64` C# compiler (`csc.exe`). Clone the repository and run:

```powershell
git clone https://github.com/rhycrea/controlpad.git
cd controlpad
powershell -ExecutionPolicy Bypass -File .\install_or_rebuild.ps1
```

### Option 3: Direct Manual Compile
```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /optimize+ /out:GamepadDesktop.exe /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll GamepadDesktop.cs
```

---

## ⚙️ Live Configuration (`config.ini`)

Edit `config.ini` and click **"Reload Settings"** from the system tray icon (no restart required):

```ini
BaseCursorSpeed=14
SlowMultiplier=0.28
TurboMultiplier=2.4
ScrollSpeed=32
DeadzoneLeft=0.14
DeadzoneRight=0.18
ResponseCurve=2.15
ToggleHoldMs=1000
PollIntervalMs=8
StartEnabled=True
EnableSoundFeedback=True
```

---

## 📄 License

Released under the [MIT License](./LICENSE) by **Aybars Yüksel** ([aybars.lunapard.com](https://aybars.lunapard.com) • [lunapard.com](https://lunapard.com)).
