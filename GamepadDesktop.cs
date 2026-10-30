using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace GamepadDesktop
{
    public class AppConfig
    {
        public double BaseCursorSpeed = 14.0;      // Pixels per tick (at 125Hz) at full deflection
        public double SlowMultiplier = 0.28;       // When LT is held (Precision mode)
        public double TurboMultiplier = 2.40;      // When RT is held (Fast mode)
        public double ScrollSpeed = 32.0;          // Wheel delta per tick at full deflection
        public double DeadzoneLeft = 0.14;         // 14% radial deadzone for left stick
        public double DeadzoneRight = 0.18;        // 18% radial deadzone for right stick
        public double ResponseCurve = 2.15;        // Power curve for smooth precision + fast edge
        public int ToggleHoldMs = 1000;            // Hold Start + Select for 1000ms to toggle ON/OFF
        public int PollIntervalMs = 8;             // ~125Hz polling
        public bool StartEnabled = false;          // Default OFF on startup so games/Playnite are never interrupted until user holds Start+Select
        public bool EnableSoundFeedback = true;

        public static string GetConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
        }

        public static AppConfig Load()
        {
            AppConfig cfg = new AppConfig();
            string path = GetConfigPath();
            if (!File.Exists(path))
            {
                cfg.Save();
                return cfg;
            }
            try
            {
                foreach (string rawLine in File.ReadAllLines(path, Encoding.UTF8))
                {
                    string line = rawLine.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith(";"))
                        continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string val = line.Substring(eq + 1).Trim();
                    double dVal;
                    int iVal;
                    bool bVal;
                    if (key.Equals("BaseCursorSpeed", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.BaseCursorSpeed = dVal;
                    else if (key.Equals("SlowMultiplier", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.SlowMultiplier = dVal;
                    else if (key.Equals("TurboMultiplier", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.TurboMultiplier = dVal;
                    else if (key.Equals("ScrollSpeed", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.ScrollSpeed = dVal;
                    else if (key.Equals("DeadzoneLeft", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.DeadzoneLeft = dVal;
                    else if (key.Equals("DeadzoneRight", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.DeadzoneRight = dVal;
                    else if (key.Equals("ResponseCurve", StringComparison.OrdinalIgnoreCase) && double.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out dVal))
                        cfg.ResponseCurve = dVal;
                    else if (key.Equals("ToggleHoldMs", StringComparison.OrdinalIgnoreCase) && int.TryParse(val, out iVal))
                        cfg.ToggleHoldMs = iVal;
                    else if (key.Equals("PollIntervalMs", StringComparison.OrdinalIgnoreCase) && int.TryParse(val, out iVal))
                        cfg.PollIntervalMs = Math.Max(4, Math.Min(50, iVal));
                    else if (key.Equals("StartEnabled", StringComparison.OrdinalIgnoreCase) && bool.TryParse(val, out bVal))
                        cfg.StartEnabled = bVal;
                    else if (key.Equals("EnableSoundFeedback", StringComparison.OrdinalIgnoreCase) && bool.TryParse(val, out bVal))
                        cfg.EnableSoundFeedback = bVal;
                }
            }
            catch { }
            return cfg;
        }

        public void Save()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("# GamepadDesktop Configuration");
                sb.AppendLine("# Changes take effect when you click 'Ayarları Yeniden Yükle' from the tray icon or restart.");
                sb.AppendLine("BaseCursorSpeed=" + BaseCursorSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("SlowMultiplier=" + SlowMultiplier.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("TurboMultiplier=" + TurboMultiplier.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("ScrollSpeed=" + ScrollSpeed.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("DeadzoneLeft=" + DeadzoneLeft.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("DeadzoneRight=" + DeadzoneRight.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("ResponseCurve=" + ResponseCurve.ToString(System.Globalization.CultureInfo.InvariantCulture));
                sb.AppendLine("ToggleHoldMs=" + ToggleHoldMs);
                sb.AppendLine("PollIntervalMs=" + PollIntervalMs);
                sb.AppendLine("StartEnabled=" + StartEnabled);
                sb.AppendLine("EnableSoundFeedback=" + EnableSoundFeedback);
                File.WriteAllText(GetConfigPath(), sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }
    }

    [Flags]
    public enum PadButtons : uint
    {
        None      = 0,
        DPadUp    = 1 << 0,
        DPadDown  = 1 << 1,
        DPadLeft  = 1 << 2,
        DPadRight = 1 << 3,
        Start     = 1 << 4,
        Back      = 1 << 5,
        LThumb    = 1 << 6,
        RThumb    = 1 << 7,
        LB        = 1 << 8,
        RB        = 1 << 9,
        A         = 1 << 10,
        B         = 1 << 11,
        X         = 1 << 12,
        Y         = 1 << 13
    }

    public struct UnifiedPadState
    {
        public bool Connected;
        public string SourceName;
        public PadButtons Buttons;
        public float LX; // -1.0 to 1.0 (right is +)
        public float LY; // -1.0 to 1.0 (up is +)
        public float RX; // -1.0 to 1.0 (right is +)
        public float RY; // -1.0 to 1.0 (up is +)
        public float LT; // 0.0 to 1.0
        public float RT; // 0.0 to 1.0
    }

    public static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_GAMEPAD
        {
            public ushort wButtons;
            public byte bLeftTrigger;
            public byte bRightTrigger;
            public short sThumbLX;
            public short sThumbLY;
            public short sThumbRX;
            public short sThumbRY;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XINPUT_STATE
        {
            public uint dwPacketNumber;
            public XINPUT_GAMEPAD Gamepad;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint XInputGetStateDelegate(uint dwUserIndex, ref XINPUT_STATE pState);

        private static XInputGetStateDelegate _xinputGetState;
        private static bool _xinputInitialized;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        public static uint XInputGetStateSafe(uint dwUserIndex, ref XINPUT_STATE pState)
        {
            if (!_xinputInitialized)
            {
                _xinputInitialized = true;
                string[] dlls = new string[] { "xinput1_4.dll", "xinput9_1_0.dll", "xinput1_3.dll" };
                foreach (string dll in dlls)
                {
                    IntPtr hMod = LoadLibrary(dll);
                    if (hMod != IntPtr.Zero)
                    {
                        IntPtr proc = GetProcAddress(hMod, "XInputGetState");
                        if (proc != IntPtr.Zero)
                        {
                            _xinputGetState = (XInputGetStateDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(XInputGetStateDelegate));
                            break;
                        }
                    }
                }
            }
            if (_xinputGetState == null) return 1167;
            try
            {
                return _xinputGetState(dwUserIndex, ref pState);
            }
            catch
            {
                return 1167;
            }
        }

        [DllImport("winmm.dll")]
        public static extern uint joyGetNumDevs();

        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        public static extern uint joyGetDevCapsW(UIntPtr uJoyID, ref JOYCAPSW pjc, uint cbjc);

        [DllImport("winmm.dll")]
        public static extern uint joyGetPosEx(uint uJoyID, ref JOYINFOEX pji);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct JOYCAPSW
        {
            public ushort wMid;
            public ushort wPid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public uint wXmin, wXmax, wYmin, wYmax, wZmin, wZmax;
            public uint wNumButtons, wPeriodMin, wPeriodMax;
            public uint wRmin, wRmax, wUmin, wUmax, wVmin, wVmax;
            public uint wCaps, wMaxAxes, wNumAxes, wMaxButtons;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szRegKey;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szOEMVxD;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JOYINFOEX
        {
            public uint dwSize;
            public uint dwFlags;
            public uint dwXpos;
            public uint dwYpos;
            public uint dwZpos;
            public uint dwRpos;
            public uint dwUpos;
            public uint dwVpos;
            public uint dwButtons;
            public uint dwButtonNumber;
            public uint dwPOV;
            public uint dwReserved1;
            public uint dwReserved2;
        }

        public const uint JOY_RETURNALL = 0x000000FF;

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public const uint INPUT_MOUSE = 0;
        public const uint INPUT_KEYBOARD = 1;

        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint MOUSEEVENTF_HWHEEL = 0x01000;

        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyIcon(IntPtr hIcon);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public const int SW_MINIMIZE = 6;
        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;

        public static void MoveMouse(int dx, int dy)
        {
            if (dx == 0 && dy == 0) return;

            Point pt;
            if (GetCursorPos(out pt))
            {
                SetCursorPos(pt.X + dx, pt.Y + dy);
            }

            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].U.mi.dx = dx;
            inputs[0].U.mi.dy = dy;
            inputs[0].U.mi.dwFlags = MOUSEEVENTF_MOVE;
            uint sent = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (sent == 0)
            {
                mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, UIntPtr.Zero);
            }
        }

        public static void MouseButton(uint flag)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].U.mi.dwFlags = flag;
            uint sent = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (sent == 0)
            {
                mouse_event(flag, 0, 0, 0, UIntPtr.Zero);
            }
        }

        public static void MouseScroll(int verticalDelta, int horizontalDelta)
        {
            if (verticalDelta != 0)
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = INPUT_MOUSE;
                inputs[0].U.mi.dwFlags = MOUSEEVENTF_WHEEL;
                inputs[0].U.mi.mouseData = verticalDelta;
                SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            }
            if (horizontalDelta != 0)
            {
                INPUT[] inputs = new INPUT[1];
                inputs[0].type = INPUT_MOUSE;
                inputs[0].U.mi.dwFlags = MOUSEEVENTF_HWHEEL;
                inputs[0].U.mi.mouseData = horizontalDelta;
                SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
            }
        }

        public static void KeyDown(ushort vk, bool extended = false)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = vk;
            inputs[0].U.ki.dwFlags = extended ? KEYEVENTF_EXTENDEDKEY : 0;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyUp(ushort vk, bool extended = false)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = vk;
            inputs[0].U.ki.dwFlags = KEYEVENTF_KEYUP | (extended ? KEYEVENTF_EXTENDEDKEY : 0);
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        public static void KeyTap(ushort vk, bool extended = false)
        {
            KeyDown(vk, extended);
            KeyUp(vk, extended);
        }

        public static void KeyChord(ushort[] modifiers, ushort mainKey)
        {
            foreach (ushort m in modifiers) KeyDown(m);
            KeyTap(mainKey);
            for (int i = modifiers.Length - 1; i >= 0; i--) KeyUp(modifiers[i]);
        }
    }

    // Modern Non-Focus-Stealing On-Screen Toast Banner
    public class ToastOverlayForm : Form
    {
        private System.Windows.Forms.Timer _hideTimer;
        private System.Windows.Forms.Timer _fadeTimer;
        private string _title = "";
        private string _subtitle = "";
        private Color _accentColor = Color.FromArgb(46, 204, 113);

        public ToastOverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(410, 82);
            BackColor = Color.FromArgb(18, 22, 29);
            Opacity = 0;
            DoubleBuffered = true;

            _hideTimer = new System.Windows.Forms.Timer();
            _hideTimer.Interval = 2400;
            _hideTimer.Tick += (s, e) =>
            {
                _hideTimer.Stop();
                _fadeTimer.Start();
            };

            _fadeTimer = new System.Windows.Forms.Timer();
            _fadeTimer.Interval = 25;
            _fadeTimer.Tick += (s, e) =>
            {
                if (Opacity > 0.08)
                    Opacity -= 0.08;
                else
                {
                    _fadeTimer.Stop();
                    Opacity = 0;
                    Hide();
                }
            };
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_TOPMOST (0x08) | WS_EX_TOOLWINDOW (0x80) | WS_EX_NOACTIVATE (0x08000000) | WS_EX_TRANSPARENT (0x20)
                cp.ExStyle |= 0x00000008 | 0x00000080 | 0x08000000 | 0x00000020;
                return cp;
            }
        }

        public event Action DeviceChanged;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            // WM_DEVICECHANGE = 0x0219, DBT_DEVNODES_CHANGED = 0x0007
            if (m.Msg == 0x0219)
            {
                int wParam = m.WParam.ToInt32();
                if (wParam == 0x0007 || wParam == 0x8000 || wParam == 0x8004)
                {
                    Action handler = DeviceChanged;
                    if (handler != null) handler();
                }
            }
        }

        public void ShowToast(string title, string subtitle, Color accent)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowToast(title, subtitle, accent)));
                return;
            }
            _title = title;
            _subtitle = subtitle;
            _accentColor = accent;

            Rectangle workArea = Screen.PrimaryScreen.WorkingArea;
            Location = new Point(workArea.Right - Width - 24, workArea.Bottom - Height - 24);

            _fadeTimer.Stop();
            _hideTimer.Stop();
            Opacity = 0.96;
            if (!Visible) Show();
            Invalidate();
            _hideTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush bg = new SolidBrush(Color.FromArgb(20, 25, 34)))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            using (SolidBrush bar = new SolidBrush(_accentColor))
            {
                g.FillRectangle(bar, 0, 0, 6, Height);
            }

            using (Pen border = new Pen(Color.FromArgb(55, 65, 81), 1))
            {
                g.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            }

            using (SolidBrush dot = new SolidBrush(_accentColor))
            {
                g.FillEllipse(dot, 18, 21, 12, 12);
            }

            using (Font fTitle = new Font("Segoe UI", 10.5f, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 8.8f, FontStyle.Regular))
            using (SolidBrush bTitle = new SolidBrush(Color.White))
            using (SolidBrush bSub = new SolidBrush(Color.FromArgb(175, 185, 200)))
            {
                g.DrawString(_title, fTitle, bTitle, 38, 15);
                g.DrawString(_subtitle, fSub, bSub, 38, 42);
            }
        }
    }

    // Sleek, Non-Focus-Stealing Full Cheatsheet HUD Overlay (Toggled via LB + RB)
    public class HelpOverlayForm : Form
    {
        private bool _isActive = false;
        private string _controllerName = "";

        public bool IsHelpVisible
        {
            get { return _isActive; }
        }

        public HelpOverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(820, 560);
            BackColor = Color.FromArgb(15, 20, 28);
            Opacity = 0.96;
            DoubleBuffered = true;
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_TOPMOST (0x08) | WS_EX_TOOLWINDOW (0x80) | WS_EX_NOACTIVATE (0x08000000)
                cp.ExStyle |= 0x00000008 | 0x00000080 | 0x08000000;
                return cp;
            }
        }

        public void ToggleHelp(string controllerName)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ToggleHelp(controllerName)));
                return;
            }
            _controllerName = controllerName;
            if (_isActive)
            {
                _isActive = false;
                Hide();
            }
            else
            {
                _isActive = true;
                Rectangle screen = Screen.PrimaryScreen.WorkingArea;
                Location = new Point(
                    screen.Left + (screen.Width - Width) / 2,
                    screen.Top + (screen.Height - Height) / 2
                );
                Show();
                Invalidate();
            }
        }

        public void HideHelp()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideHelp));
                return;
            }
            if (_isActive)
            {
                _isActive = false;
                Hide();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Background
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(16, 21, 30)))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            // Top header bar
            using (SolidBrush headerBg = new SolidBrush(Color.FromArgb(24, 32, 46)))
            {
                g.FillRectangle(headerBg, 0, 0, Width, 68);
            }
            using (SolidBrush accentLine = new SolidBrush(Color.FromArgb(47, 129, 247)))
            {
                g.FillRectangle(accentLine, 0, 0, Width, 4);
            }

            // Header Text
            using (Font fHeader = new Font("Segoe UI", 14f, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 9.5f, FontStyle.Regular))
            using (SolidBrush bWhite = new SolidBrush(Color.White))
            using (SolidBrush bMuted = new SolidBrush(Color.FromArgb(150, 165, 185)))
            using (SolidBrush bGreen = new SolidBrush(Color.FromArgb(46, 204, 113)))
            {
                g.DrawString("GAMEPAD MASAÜSTÜ KONTROL REHBERİ", fHeader, bWhite, 26, 14);
                g.DrawString("Aktif Cihaz: " + _controllerName, fSub, bGreen, 28, 41);
                g.DrawString("Kapatmak için: LB + RB  veya  A / B / Esc", fSub, bMuted, Width - 290, 26);
            }

            // Two-column layout
            int col1X = 26;
            int col2X = 422;
            int colWidth = 372;

            DrawSection(g, col1X, 86, colWidth, "TEMEL MOD & GÜVENLİK (OYUN KORUMASI)", Color.FromArgb(46, 204, 113), new string[,] {
                { "Start + Select (1 sn)", "Masaüstü Modunu AÇ / KAPAT" },
                { "Kapalıyken (Oyun Modu)", "Sıfır müdahale! Hiçbir tuş masaüstüne atmaz" },
                { "LB + RB (Birlikte Bas)", "Bu Yardım Ekranını Aç / Kapat" }
            });

            DrawSection(g, col1X, 232, colWidth, "FARE & SÜRÜKLE-BIRAK (CLICK & DRAG)", Color.FromArgb(47, 129, 247), new string[,] {
                { "Sol Analog / Direksiyon", "Fare İmleci Hareketi" },
                { "Sağ Analog", "Sayfa Kaydırma (Dikey & Yatay Scroll)" },
                { "A (Basılı Tutarak)", "Sol Tık & Sürükle-Bırak (Click & Drag)" },
                { "LB + A", "Sürükleme Kilidi (Drag Lock Aç / Bırak)" },
                { "B  /  X", "Sağ Tık  /  Orta Tık (Yeni Sekme)" },
                { "LT (Sol Tetik Basılı)", "Hassas (Sniper / Yavaş) İmleç Modu" },
                { "RT (Sağ Tetik Basılı)", "Turbo (Hızlı) İmleç Modu" }
            });

            DrawSection(g, col2X, 86, colWidth, "PLAYNITE & PENCERE YÖNETİMİ", Color.FromArgb(155, 89, 182), new string[,] {
                { "RB + Start", "Playnite'ı Öne Getir / Minimize'dan Çıkar" },
                { "RB + Y", "Playnite Fullscreen <-> Normal Mod Geçişi" },
                { "RB + B", "Aktif Pencereyi / Playnite'ı Minimize Et" },
                { "Select (Basılı Tut)", "Alt + Tab Menüsü (LB/RB/Ok ile Seç, Bırak)" },
                { "Select (Tek Bas-Bırak)", "Son Pencereye Hızlı Geçiş (Alt + Tab)" }
            });

            DrawSection(g, col2X, 294, colWidth, "KLAVYE, SEKME & MEDYA KONTROLLERİ", Color.FromArgb(241, 196, 15), new string[,] {
                { "LB  /  RB (Tek Bas)", "Önceki Sekme  /  Sonraki Sekme (Ctrl+Tab)" },
                { "Y", "Ekran Klavyesini (osk.exe) Aç / Kapat" },
                { "Start (Tek Bas)", "Enter Tuşu" },
                { "D-Pad (Yön Tuşları)", "Yukarı / Aşağı / Sol / Sağ Ok Tuşları" },
                { "LB + D-Pad Yukarı/Aşağı", "Sistem Sesini Aç / Kıs" },
                { "L3 (Sol Analog Tık)", "Escape (Esc)" },
                { "R3 (Sağ Analog Tık)", "Windows Görev Görünümü (Win + Tab)" }
            });

            // Footer border
            using (Pen border = new Pen(Color.FromArgb(55, 68, 90), 2))
            {
                g.DrawRectangle(border, 1, 1, Width - 2, Height - 2);
            }
        }

        private void DrawSection(Graphics g, int x, int y, int width, string sectionTitle, Color badgeColor, string[,] rows)
        {
            int rowCount = rows.GetLength(0);
            int boxHeight = 36 + rowCount * 28;

            using (SolidBrush cardBg = new SolidBrush(Color.FromArgb(22, 29, 41)))
            using (Pen cardBorder = new Pen(Color.FromArgb(40, 50, 68), 1))
            {
                g.FillRectangle(cardBg, x, y, width, boxHeight);
                g.DrawRectangle(cardBorder, x, y, width, boxHeight);
            }

            using (SolidBrush accent = new SolidBrush(badgeColor))
            using (Font fSec = new Font("Segoe UI", 9.2f, FontStyle.Bold))
            {
                g.FillRectangle(accent, x, y, 4, boxHeight);
                g.DrawString(sectionTitle, fSec, accent, x + 12, y + 8);
            }

            using (Font fKey = new Font("Segoe UI Semibold", 8.8f, FontStyle.Bold))
            using (Font fDesc = new Font("Segoe UI", 8.8f, FontStyle.Regular))
            using (SolidBrush bKeyBg = new SolidBrush(Color.FromArgb(34, 44, 62)))
            using (SolidBrush bKeyText = new SolidBrush(Color.FromArgb(235, 242, 255)))
            using (SolidBrush bDescText = new SolidBrush(Color.FromArgb(185, 195, 212)))
            {
                for (int i = 0; i < rowCount; i++)
                {
                    int ry = y + 32 + i * 28;
                    string keyText = rows[i, 0];
                    string descText = rows[i, 1];

                    g.FillRectangle(bKeyBg, x + 12, ry, 148, 22);
                    g.DrawString(keyText, fKey, bKeyText, x + 17, ry + 2);
                    g.DrawString(descText, fDesc, bDescText, x + 166, ry + 2);
                }
            }
        }
    }

    public class GamepadDesktopContext : ApplicationContext
    {
        private NotifyIcon _trayIcon;
        private ToastOverlayForm _toast;
        private HelpOverlayForm _helpOverlay;
        private AppConfig _config;
        private System.Threading.Timer _pollTimer;
        private int _isPolling = 0;

        // STRICT MODE STATE:
        // Only toggled when the user holds Start + Select for 1 second (or clicks tray menu).
        // When _desktopModeEnabled is false, ZERO shortcuts/inputs are intercepted or sent!
        private bool _desktopModeEnabled = false;

        // Wait-for-Release Barrier:
        // Whenever mode is toggled ON or OFF, we ignore all buttons until the user releases all currently held buttons.
        // This guarantees toggling ON/OFF never leaks Enter, Tab, Alt+Tab, or clicks into the active game or desktop!
        private bool _waitForAllButtonsReleased = false;

        private string _activeControllerName = "Yok";

        // Sub-pixel accumulators
        private double _remMoveX = 0;
        private double _remMoveY = 0;
        private double _remScrollV = 0;
        private double _remScrollH = 0;

        // Click & Drag state
        private bool _leftButtonDown = false;
        private bool _dragLocked = false;
        private bool _suppressNextARelease = false;

        // Previous button state & timing
        private PadButtons _prevButtons = PadButtons.None;
        private DateTime _comboStartTime = DateTime.MinValue;
        private bool _comboTriggered = false;

        // Non-conflicting modifier tracking for LB, RB, Back, Start
        private bool _lbUsedAsModifier = false;
        private bool _rbUsedAsModifier = false;
        private bool _startUsedForCombo = false;
        private bool _backUsedForCombo = false;

        // Delayed Alt+Tab switcher so Back+Start doesn't prematurely fire Alt+Tab
        private DateTime _backPressedTime = DateTime.MinValue;
        private bool _altTabActive = false;

        // D-Pad repeat tracking
        private Dictionary<PadButtons, DateTime> _dpadNextRepeat = new Dictionary<PadButtons, DateTime>();

        // WinMM rest calibration for pedals / triggers on DirectInput devices (e.g., G29)
        private Dictionary<uint, uint[]> _winmmInitialAxes = new Dictionary<uint, uint[]>();
        private volatile bool _winmmRescanNeeded = true;
        private List<uint> _validWinmmIds = new List<uint>();
        private Dictionary<uint, NativeMethods.JOYCAPSW> _winmmCaps = new Dictionary<uint, NativeMethods.JOYCAPSW>();

        private ToolStripMenuItem _menuToggleItem;
        private ToolStripMenuItem _menuStatusItem;

        public GamepadDesktopContext()
        {
            _config = AppConfig.Load();
            _desktopModeEnabled = _config.StartEnabled;

            _toast = new ToastOverlayForm();
            IntPtr h1 = _toast.Handle;
            _toast.DeviceChanged += () => { _winmmRescanNeeded = true; };

            _helpOverlay = new HelpOverlayForm();
            IntPtr h2 = _helpOverlay.Handle;

            BuildTrayIcon();
            UpdateTrayVisuals();

            _pollTimer = new System.Threading.Timer(PollTick, null, 100, _config.PollIntervalMs);

            _toast.ShowToast(
                _desktopModeEnabled ? "Gamepad Masaüstü Modu: AÇIK" : "Gamepad Masaüstü: HAZIR (Oyun Modu)",
                "Aç/Kapat: Start + Select (1 sn)  |  Yardım: LB + RB",
                _desktopModeEnabled ? Color.FromArgb(46, 204, 113) : Color.FromArgb(47, 129, 247)
            );
        }

        private Icon CreateGamepadIcon(bool enabled)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                Color mainColor = enabled ? Color.FromArgb(47, 129, 247) : Color.FromArgb(95, 105, 120);
                Color dotColor = enabled ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60);

                using (SolidBrush b = new SolidBrush(mainColor))
                {
                    g.FillEllipse(b, 2, 6, 28, 20);
                }
                using (Pen p = new Pen(Color.White, 2.2f))
                {
                    g.DrawLine(p, 8, 16, 14, 16);
                    g.DrawLine(p, 11, 13, 11, 19);
                }
                using (SolidBrush bWhite = new SolidBrush(Color.White))
                {
                    g.FillEllipse(bWhite, 19, 12, 3, 3);
                    g.FillEllipse(bWhite, 23, 16, 3, 3);
                }
                using (SolidBrush bDot = new SolidBrush(dotColor))
                {
                    g.FillEllipse(bDot, 21, 21, 9, 9);
                }
            }
            IntPtr hIcon = bmp.GetHicon();
            Icon cloned = (Icon)Icon.FromHandle(hIcon).Clone();
            NativeMethods.DestroyIcon(hIcon);
            bmp.Dispose();
            return cloned;
        }

        private void BuildTrayIcon()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            _menuStatusItem = new ToolStripMenuItem("Kontrolcü: Aranıyor...") { Enabled = false };
            menu.Items.Add(_menuStatusItem);
            menu.Items.Add(new ToolStripSeparator());

            _menuToggleItem = new ToolStripMenuItem("Masaüstü Modu (Start+Select 1sn)", null, (s, e) => ToggleDesktopModeManual());
            menu.Items.Add(_menuToggleItem);

            menu.Items.Add(new ToolStripMenuItem("Görsel Tuş Rehberini Aç / Kapat (LB + RB)", null, (s, e) =>
            {
                _helpOverlay.ToggleHelp(_activeControllerName);
            }));

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Playnite'ı Öne Getir / Minimize'dan Çıkar (RB + Start)", null, (s, e) => PlayniteBringToFrontOrRestore()));
            menu.Items.Add(new ToolStripMenuItem("Playnite Fullscreen <-> Desktop Mod Değiştir (RB + Y)", null, (s, e) => PlayniteToggleFullscreenDesktopMode()));
            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add(new ToolStripMenuItem("Ayarlar Dosyasını Aç (config.ini)", null, (s, e) =>
            {
                try { Process.Start("notepad.exe", AppConfig.GetConfigPath()); } catch { }
            }));
            menu.Items.Add(new ToolStripMenuItem("Ayarları Yeniden Yükle", null, (s, e) =>
            {
                _config = AppConfig.Load();
                _pollTimer.Change(0, _config.PollIntervalMs);
                _toast.ShowToast("Ayarlar Yenilendi", "config.ini başarıyla yüklendi", Color.FromArgb(47, 129, 247));
                UpdateTrayVisuals();
            }));

            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("Çıkış", null, (s, e) => ExitThread()));

            _trayIcon = new NotifyIcon
            {
                Icon = CreateGamepadIcon(_desktopModeEnabled),
                Text = "Gamepad Masaüstü Kontrolcüsü",
                ContextMenuStrip = menu,
                Visible = true
            };
            _trayIcon.DoubleClick += (s, e) => _helpOverlay.ToggleHelp(_activeControllerName);
        }

        private void UpdateTrayVisuals()
        {
            if (_trayIcon == null) return;
            try
            {
                Icon old = _trayIcon.Icon;
                _trayIcon.Icon = CreateGamepadIcon(_desktopModeEnabled);
                if (old != null) old.Dispose();

                string stateStr = _desktopModeEnabled ? "AÇIK (Masaüstü Kontrolü)" : "KAPALI (Oyun Modu - Sıfır Müdahale)";
                _menuToggleItem.Text = "Masaüstü Modu: " + stateStr;
                _menuStatusItem.Text = "Kontrolcü: " + _activeControllerName;

                string tip = "Gamepad: " + (_desktopModeEnabled ? "AÇIK" : "KAPALI (Oyun Modu)") + "\nCihaz: " + _activeControllerName;
                if (tip.Length >= 63) tip = tip.Substring(0, 62);
                _trayIcon.Text = tip;
            }
            catch { }
        }

        private void ReleaseHeldInputsCleanly()
        {
            if (_altTabActive)
            {
                NativeMethods.KeyUp(0x12);
                _altTabActive = false;
            }
            if (_leftButtonDown || _dragLocked)
            {
                NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_LEFTUP);
                _leftButtonDown = false;
                _dragLocked = false;
            }
            _dpadNextRepeat.Clear();
            _backPressedTime = DateTime.MinValue;
            _lbUsedAsModifier = false;
            _rbUsedAsModifier = false;
            _startUsedForCombo = false;
            _backUsedForCombo = false;
        }

        private void SetDesktopModeState(bool enable, string customSubtitle = null)
        {
            ReleaseHeldInputsCleanly();
            _desktopModeEnabled = enable;
            // Require all currently held buttons to be released before processing any new actions
            _waitForAllButtonsReleased = true;

            if (!enable && _helpOverlay != null && _helpOverlay.IsHelpVisible)
            {
                _helpOverlay.HideHelp();
            }

            if (_config.EnableSoundFeedback)
            {
                try
                {
                    if (_desktopModeEnabled)
                    {
                        Console.Beep(660, 75);
                        Console.Beep(880, 95);
                    }
                    else
                    {
                        Console.Beep(880, 75);
                        Console.Beep(440, 110);
                    }
                }
                catch { }
            }

            string sub = customSubtitle ?? (_desktopModeEnabled
                ? "Fare, Sürükle-Bırak ve Kısayollar Aktif (Yardım: LB+RB)"
                : "Oyun Modu — Hiçbir tuş masaüstüne müdahale etmez");

            _toast.ShowToast(
                _desktopModeEnabled ? "Gamepad Masaüstü Modu: AÇIK" : "Gamepad Masaüstü Modu: KAPALI",
                sub,
                _desktopModeEnabled ? Color.FromArgb(46, 204, 113) : Color.FromArgb(231, 76, 60)
            );

            if (_toast.InvokeRequired)
                _toast.BeginInvoke(new Action(UpdateTrayVisuals));
            else
                UpdateTrayVisuals();
        }

        private void ToggleDesktopModeManual()
        {
            SetDesktopModeState(!_desktopModeEnabled);
        }

        // Playnite & Window Management Helpers (Only active when Desktop Mode is ON!)
        private string GetPlayniteDir()
        {
            string local = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playnite");
            if (Directory.Exists(local)) return local;
            return @"C:\Users\aybar\AppData\Local\Playnite";
        }

        private IntPtr FindProcessMainWindow(string procNamePrefix)
        {
            IntPtr found = IntPtr.Zero;
            Process[] procs = Process.GetProcesses();
            HashSet<uint> targetPids = new HashSet<uint>();
            foreach (Process p in procs)
            {
                try
                {
                    if (p.ProcessName.StartsWith(procNamePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        targetPids.Add((uint)p.Id);
                        if (p.MainWindowHandle != IntPtr.Zero && found == IntPtr.Zero)
                        {
                            found = p.MainWindowHandle;
                        }
                    }
                }
                catch { }
            }
            if (found != IntPtr.Zero) return found;
            if (targetPids.Count == 0) return IntPtr.Zero;

            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                uint pid;
                NativeMethods.GetWindowThreadProcessId(hWnd, out pid);
                if (targetPids.Contains(pid))
                {
                    StringBuilder sbTitle = new StringBuilder(256);
                    StringBuilder sbClass = new StringBuilder(256);
                    NativeMethods.GetWindowText(hWnd, sbTitle, 256);
                    NativeMethods.GetClassName(hWnd, sbClass, 256);
                    string title = sbTitle.ToString();
                    string cls = sbClass.ToString();
                    if (title.IndexOf("Playnite", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        cls.StartsWith("HwndWrapper[Playnite", StringComparison.OrdinalIgnoreCase))
                    {
                        found = hWnd;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);

            return found;
        }

        private void PlayniteBringToFrontOrRestore()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string playniteDir = GetPlayniteDir();
                    string desktopExe = Path.Combine(playniteDir, "Playnite.DesktopApp.exe");
                    string fullscreenExe = Path.Combine(playniteDir, "Playnite.FullscreenApp.exe");

                    bool isFullscreenRunning = Process.GetProcessesByName("Playnite.FullscreenApp").Length > 0;
                    bool isDesktopRunning = Process.GetProcessesByName("Playnite.DesktopApp").Length > 0;

                    IntPtr hWnd = FindProcessMainWindow(isFullscreenRunning ? "Playnite.FullscreenApp" : "Playnite.DesktopApp");
                    if (hWnd != IntPtr.Zero)
                    {
                        NativeMethods.ShowWindowAsync(hWnd, NativeMethods.SW_RESTORE);
                        NativeMethods.ShowWindowAsync(hWnd, NativeMethods.SW_SHOW);
                        NativeMethods.SetForegroundWindow(hWnd);
                    }

                    if (isFullscreenRunning && File.Exists(fullscreenExe))
                    {
                        Process.Start(new ProcessStartInfo(fullscreenExe, "--startfullscreen") { UseShellExecute = false });
                        // Automatically turn OFF desktop mode when entering Playnite Fullscreen so controller works natively in Playnite/Games!
                        SetDesktopModeState(false, "Playnite Fullscreen açıldı — Oyun Moduna geçildi");
                    }
                    else if (File.Exists(desktopExe))
                    {
                        Process.Start(new ProcessStartInfo(desktopExe, "--startdesktop") { UseShellExecute = false });
                        _toast.ShowToast("Playnite Masaüstü Öne Getirildi", isDesktopRunning ? "Minimize durumundan çıkarıldı" : "Playnite başlatıldı", Color.FromArgb(47, 129, 247));
                    }
                }
                catch { }
            });
        }

        private void PlayniteToggleFullscreenDesktopMode()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string playniteDir = GetPlayniteDir();
                    string desktopExe = Path.Combine(playniteDir, "Playnite.DesktopApp.exe");
                    string fullscreenExe = Path.Combine(playniteDir, "Playnite.FullscreenApp.exe");

                    bool isFullscreenRunning = Process.GetProcessesByName("Playnite.FullscreenApp").Length > 0;
                    if (isFullscreenRunning)
                    {
                        if (File.Exists(desktopExe))
                        {
                            Process.Start(new ProcessStartInfo(desktopExe, "--startdesktop") { UseShellExecute = false });
                            _toast.ShowToast("Playnite: Normal (Masaüstü) Mod", "Masaüstü fare kontrolü aktif", Color.FromArgb(46, 204, 113));
                        }
                    }
                    else
                    {
                        if (File.Exists(fullscreenExe))
                        {
                            Process.Start(new ProcessStartInfo(fullscreenExe, "--startfullscreen") { UseShellExecute = false });
                            // Switching to Playnite Fullscreen automatically disables Desktop Mode so games & Playnite have 100% clean gamepad control!
                            SetDesktopModeState(false, "Playnite Fullscreen — Oyun Moduna geçildi");
                        }
                    }
                }
                catch { }
            });
        }

        private void MinimizeActiveOrPlayniteWindow()
        {
            try
            {
                IntPtr fg = NativeMethods.GetForegroundWindow();
                if (fg != IntPtr.Zero)
                {
                    NativeMethods.ShowWindowAsync(fg, NativeMethods.SW_MINIMIZE);
                    _toast.ShowToast("Pencere Küçültüldü", "Aktif pencere simge durumuna alındı (RB + B)", Color.FromArgb(46, 204, 113));
                    return;
                }
                IntPtr playniteHwnd = FindProcessMainWindow("Playnite.");
                if (playniteHwnd != IntPtr.Zero)
                {
                    NativeMethods.ShowWindowAsync(playniteHwnd, NativeMethods.SW_MINIMIZE);
                    _toast.ShowToast("Playnite Küçültüldü", "Playnite simge durumuna alındı (RB + B)", Color.FromArgb(46, 204, 113));
                }
            }
            catch { }
        }

        private void PollTick(object state)
        {
            if (Interlocked.Exchange(ref _isPolling, 1) == 1) return;
            try
            {
                UnifiedPadState pad = ReadAllControllers();
                string newName = pad.Connected ? pad.SourceName : "Bağlı Değil (Bekleniyor)";
                if (newName != _activeControllerName)
                {
                    _activeControllerName = newName;
                    if (_toast.IsHandleCreated)
                        _toast.BeginInvoke(new Action(UpdateTrayVisuals));
                }

                if (!pad.Connected)
                {
                    ReleaseHeldInputsCleanly();
                    _prevButtons = PadButtons.None;
                    return;
                }

                ProcessInput(pad);
            }
            catch { }
            finally
            {
                Interlocked.Exchange(ref _isPolling, 0);
            }
        }

        private UnifiedPadState ReadAllControllers()
        {
            UnifiedPadState combined = new UnifiedPadState();
            List<string> activeSources = new List<string>();
            bool anyXInputConnected = false;

            for (uint i = 0; i < 4; i++)
            {
                NativeMethods.XINPUT_STATE st = new NativeMethods.XINPUT_STATE();
                if (NativeMethods.XInputGetStateSafe(i, ref st) == 0)
                {
                    anyXInputConnected = true;
                    combined.Connected = true;
                    activeSources.Add("XInput #" + (i + 1));

                    ushort b = st.Gamepad.wButtons;
                    if ((b & 0x0001) != 0) combined.Buttons |= PadButtons.DPadUp;
                    if ((b & 0x0002) != 0) combined.Buttons |= PadButtons.DPadDown;
                    if ((b & 0x0004) != 0) combined.Buttons |= PadButtons.DPadLeft;
                    if ((b & 0x0008) != 0) combined.Buttons |= PadButtons.DPadRight;
                    if ((b & 0x0010) != 0) combined.Buttons |= PadButtons.Start;
                    if ((b & 0x0020) != 0) combined.Buttons |= PadButtons.Back;
                    if ((b & 0x0040) != 0) combined.Buttons |= PadButtons.LThumb;
                    if ((b & 0x0080) != 0) combined.Buttons |= PadButtons.RThumb;
                    if ((b & 0x0100) != 0) combined.Buttons |= PadButtons.LB;
                    if ((b & 0x0200) != 0) combined.Buttons |= PadButtons.RB;
                    if ((b & 0x1000) != 0) combined.Buttons |= PadButtons.A;
                    if ((b & 0x2000) != 0) combined.Buttons |= PadButtons.B;
                    if ((b & 0x4000) != 0) combined.Buttons |= PadButtons.X;
                    if ((b & 0x8000) != 0) combined.Buttons |= PadButtons.Y;

                    float lx = NormalizeShort(st.Gamepad.sThumbLX);
                    float ly = NormalizeShort(st.Gamepad.sThumbLY);
                    float rx = NormalizeShort(st.Gamepad.sThumbRX);
                    float ry = NormalizeShort(st.Gamepad.sThumbRY);
                    float lt = st.Gamepad.bLeftTrigger / 255.0f;
                    float rt = st.Gamepad.bRightTrigger / 255.0f;

                    if (Math.Abs(lx) > Math.Abs(combined.LX)) combined.LX = lx;
                    if (Math.Abs(ly) > Math.Abs(combined.LY)) combined.LY = ly;
                    if (Math.Abs(rx) > Math.Abs(combined.RX)) combined.RX = rx;
                    if (Math.Abs(ry) > Math.Abs(combined.RY)) combined.RY = ry;
                    if (lt > combined.LT) combined.LT = lt;
                    if (rt > combined.RT) combined.RT = rt;
                }
            }

            // WinMM joyGetDevCapsW leaks 7 registry handles per disconnected slot in Windows 11 winmm.dll!
            // Only scan WinMM when no XInput controller is active AND a USB device change occurred (or once at startup).
            if (!anyXInputConnected && _winmmRescanNeeded)
            {
                _winmmRescanNeeded = false;
                _validWinmmIds.Clear();
                uint num = Math.Min(4u, NativeMethods.joyGetNumDevs());
                for (uint id = 0; id < num; id++)
                {
                    NativeMethods.JOYCAPSW caps = new NativeMethods.JOYCAPSW();
                    if (NativeMethods.joyGetDevCapsW((UIntPtr)id, ref caps, (uint)Marshal.SizeOf(typeof(NativeMethods.JOYCAPSW))) == 0)
                    {
                        if (caps.wNumButtons > 0 || caps.wNumAxes > 0)
                        {
                            _validWinmmIds.Add(id);
                            _winmmCaps[id] = caps;
                        }
                    }
                }
            }

            if (!anyXInputConnected)
            {
                for (int idx = _validWinmmIds.Count - 1; idx >= 0; idx--)
                {
                    uint id = _validWinmmIds[idx];
                    NativeMethods.JOYCAPSW caps = _winmmCaps[id];
                    bool isLogitechOrSony = (caps.wMid == 0x046D || caps.wMid == 0x054C);

                    NativeMethods.JOYINFOEX info = new NativeMethods.JOYINFOEX();
                    info.dwSize = (uint)Marshal.SizeOf(typeof(NativeMethods.JOYINFOEX));
                    info.dwFlags = NativeMethods.JOY_RETURNALL;
                    if (NativeMethods.joyGetPosEx(id, ref info) == 0)
                    {
                        uint b = info.dwButtons;

                        combined.Connected = true;
                        string devLabel = isLogitechOrSony
                            ? (caps.wMid == 0x046D ? "Logitech (G29/DInput)" : "PlayStation Controller")
                            : ("DInput #" + (id + 1));
                        activeSources.Add(devLabel);

                        if (!_winmmInitialAxes.ContainsKey(id))
                        {
                            _winmmInitialAxes[id] = new uint[] { info.dwXpos, info.dwYpos, info.dwZpos, info.dwRpos, info.dwUpos, info.dwVpos };
                        }

                        if (info.dwPOV != 0xFFFF && info.dwPOV <= 36000)
                        {
                            uint pov = info.dwPOV;
                            if (pov >= 31500 || pov <= 4500) combined.Buttons |= PadButtons.DPadUp;
                            if (pov >= 4500 && pov <= 13500) combined.Buttons |= PadButtons.DPadRight;
                            if (pov >= 13500 && pov <= 22500) combined.Buttons |= PadButtons.DPadDown;
                            if (pov >= 22500 && pov <= 31500) combined.Buttons |= PadButtons.DPadLeft;
                        }

                        if (caps.wMid == 0x046D && (caps.wPid == 0xC24F || caps.wPid == 0xC29B || caps.wPid == 0xC294))
                        {
                            if ((b & (1u << 0)) != 0) combined.Buttons |= PadButtons.A;
                            if ((b & (1u << 1)) != 0) combined.Buttons |= PadButtons.X;
                            if ((b & (1u << 2)) != 0) combined.Buttons |= PadButtons.B;
                            if ((b & (1u << 3)) != 0) combined.Buttons |= PadButtons.Y;
                            if ((b & (1u << 4)) != 0) combined.Buttons |= PadButtons.RB;
                            if ((b & (1u << 5)) != 0) combined.Buttons |= PadButtons.LB;
                            if ((b & (1u << 6)) != 0) combined.RT = 1.0f;
                            if ((b & (1u << 7)) != 0) combined.LT = 1.0f;
                            if ((b & (1u << 8)) != 0) combined.Buttons |= PadButtons.Back;
                            if ((b & (1u << 9)) != 0) combined.Buttons |= PadButtons.Start;
                            if ((b & (1u << 10)) != 0) combined.Buttons |= PadButtons.RThumb;
                            if ((b & (1u << 11)) != 0) combined.Buttons |= PadButtons.LThumb;

                            float wheelX = ((int)info.dwXpos - 32768) / 32768.0f;
                            if (Math.Abs(wheelX) > Math.Abs(combined.LX)) combined.LX = wheelX;
                        }
                        else
                        {
                            if ((b & (1u << 0)) != 0) combined.Buttons |= PadButtons.A;
                            if ((b & (1u << 1)) != 0) combined.Buttons |= PadButtons.B;
                            if ((b & (1u << 2)) != 0) combined.Buttons |= PadButtons.X;
                            if ((b & (1u << 3)) != 0) combined.Buttons |= PadButtons.Y;
                            if ((b & (1u << 4)) != 0) combined.Buttons |= PadButtons.LB;
                            if ((b & (1u << 5)) != 0) combined.Buttons |= PadButtons.RB;
                            if ((b & (1u << 6)) != 0) combined.Buttons |= PadButtons.Back;
                            if ((b & (1u << 7)) != 0) combined.Buttons |= PadButtons.Start;
                            if ((b & (1u << 8)) != 0) combined.Buttons |= PadButtons.LThumb;
                            if ((b & (1u << 9)) != 0) combined.Buttons |= PadButtons.RThumb;

                            float lx = ((int)info.dwXpos - 32768) / 32768.0f;
                            float ly = -(((int)info.dwYpos - 32768) / 32768.0f);
                            if (Math.Abs(lx) > Math.Abs(combined.LX)) combined.LX = lx;
                            if (Math.Abs(ly) > Math.Abs(combined.LY)) combined.LY = ly;

                            uint[] init = _winmmInitialAxes[id];
                            if (Math.Abs((int)init[2] - 32768) < 8000 && Math.Abs((int)init[3] - 32768) < 8000)
                            {
                                float rx = ((int)info.dwZpos - 32768) / 32768.0f;
                                float ry = -(((int)info.dwRpos - 32768) / 32768.0f);
                                if (Math.Abs(rx) > Math.Abs(combined.RX)) combined.RX = rx;
                                if (Math.Abs(ry) > Math.Abs(combined.RY)) combined.RY = ry;
                            }
                        }
                    }
                    else
                    {
                        // Disconnected or invalid DirectInput device: remove immediately so joyGetPosEx is not called repeatedly!
                        _validWinmmIds.RemoveAt(idx);
                    }
                }
            }

            combined.SourceName = activeSources.Count > 0 ? string.Join(" + ", activeSources.ToArray()) : "Yok";
            return combined;
        }

        private float NormalizeShort(short v)
        {
            if (v == -32768) return -1.0f;
            return v / 32767.0f;
        }

        private void ApplyRadialDeadzone(float inX, float inY, double deadzone, double curve, out double outX, out double outY)
        {
            double mag = Math.Sqrt(inX * inX + inY * inY);
            if (mag <= deadzone)
            {
                outX = 0;
                outY = 0;
                return;
            }
            double clampedMag = Math.Min(1.0, mag);
            double normMag = (clampedMag - deadzone) / (1.0 - deadzone);
            double curvedMag = Math.Pow(normMag, curve);
            outX = (inX / mag) * curvedMag;
            outY = (inY / mag) * curvedMag;
        }

        private bool IsPressed(PadButtons current, PadButtons btn)
        {
            return (current & btn) != 0;
        }

        private bool WasJustPressed(PadButtons current, PadButtons btn)
        {
            return ((current & btn) != 0) && ((_prevButtons & btn) == 0);
        }

        private bool WasJustReleased(PadButtons current, PadButtons btn)
        {
            return ((current & btn) == 0) && ((_prevButtons & btn) != 0);
        }

        private void ToggleOnScreenKeyboard()
        {
            try
            {
                Process[] oskProcs = Process.GetProcessesByName("osk");
                if (oskProcs != null && oskProcs.Length > 0)
                {
                    foreach (Process p in oskProcs)
                    {
                        try { p.Kill(); } catch { }
                    }
                    return;
                }
                ProcessStartInfo psi = new ProcessStartInfo("osk.exe")
                {
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                try
                {
                    string tabTip = @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe";
                    if (File.Exists(tabTip))
                        Process.Start(new ProcessStartInfo(tabTip) { UseShellExecute = true });
                }
                catch { }
            }
        }

        private void ProcessInput(UnifiedPadState pad)
        {
            PadButtons cur = pad.Buttons;

            // ═══════════════════════════════════════════════════════════════════
            // 1. STRICT START + SELECT (1 SECOND) TOGGLE
            //    This is the ONLY shortcut active when Desktop Mode is OFF!
            //    We also require that NO other buttons (no RB, LB, A, B, X, Y) are pressed
            //    so mortal kombat combos never accidentally trigger anything.
            // ═══════════════════════════════════════════════════════════════════
            PadButtons nonStartBackMask = ~(PadButtons.Start | PadButtons.Back);
            bool onlyStartAndBackHeld =
                IsPressed(cur, PadButtons.Start) &&
                IsPressed(cur, PadButtons.Back) &&
                ((cur & nonStartBackMask) == PadButtons.None);

            if (onlyStartAndBackHeld)
            {
                _startUsedForCombo = true;
                _backUsedForCombo = true;
                _backPressedTime = DateTime.MinValue;

                if (_altTabActive)
                {
                    NativeMethods.KeyUp(0x12);
                    _altTabActive = false;
                }

                if (_comboStartTime == DateTime.MinValue)
                {
                    _comboStartTime = DateTime.UtcNow;
                    _comboTriggered = false;
                }
                else if (!_comboTriggered && (DateTime.UtcNow - _comboStartTime).TotalMilliseconds >= _config.ToggleHoldMs)
                {
                    _comboTriggered = true;
                    ToggleDesktopModeManual();
                }

                _prevButtons = cur;
                return;
            }
            else
            {
                _comboStartTime = DateTime.MinValue;
                _comboTriggered = false;
            }

            // ═══════════════════════════════════════════════════════════════════
            // 2. WAIT-FOR-RELEASE BARRIER AFTER TOGGLING
            //    After toggling ON or OFF, ignore all button states until the user
            //    lets go of all buttons. This guarantees releasing Start/Select never
            //    sends Enter or Alt+Tab to the game or desktop!
            // ═══════════════════════════════════════════════════════════════════
            if (_waitForAllButtonsReleased)
            {
                if (cur == PadButtons.None)
                {
                    _waitForAllButtonsReleased = false;
                }
                _prevButtons = cur;
                return;
            }

            // ═══════════════════════════════════════════════════════════════════
            // 3. WHEN DESKTOP MODE IS OFF (GAME MODE): DO ABSOLUTELY NOTHING!
            //    No RB+B, no RB+Y, no RB+Start, no mouse, no keyboard.
            //    100% of controller inputs go exclusively to your game (e.g. Mortal Kombat).
            // ═══════════════════════════════════════════════════════════════════
            if (!_desktopModeEnabled)
            {
                _prevButtons = cur;
                return;
            }

            // ═══════════════════════════════════════════════════════════════════
            // 4. HELP OVERLAY SHORTCUT: LB + RB (Pressed Together in Desktop Mode)
            // ═══════════════════════════════════════════════════════════════════
            bool bothBumpersHeld = IsPressed(cur, PadButtons.LB) && IsPressed(cur, PadButtons.RB) && !_altTabActive;
            if (bothBumpersHeld)
            {
                bool justCompletedCombo = WasJustPressed(cur, PadButtons.LB) || WasJustPressed(cur, PadButtons.RB);
                _lbUsedAsModifier = true;
                _rbUsedAsModifier = true;
                if (justCompletedCombo)
                {
                    _helpOverlay.ToggleHelp(_activeControllerName);
                }
                _prevButtons = cur;
                return;
            }

            // If Help Overlay is currently open, pressing A, B, Start, Back, or LThumb closes it cleanly without clicking the desktop
            if (_helpOverlay != null && _helpOverlay.IsHelpVisible)
            {
                if (WasJustPressed(cur, PadButtons.A) || WasJustPressed(cur, PadButtons.B) ||
                    WasJustPressed(cur, PadButtons.Back) || WasJustPressed(cur, PadButtons.Start) ||
                    WasJustPressed(cur, PadButtons.LThumb))
                {
                    _helpOverlay.HideHelp();
                    _waitForAllButtonsReleased = true;
                }
                _prevButtons = cur;
                return;
            }

            // ═══════════════════════════════════════════════════════════════════
            // 5. PLAYNITE & WINDOW SHORTCUTS (Only when Desktop Mode is ON!)
            //    RB + Start : Restore / Focus Playnite
            //    RB + Y     : Toggle Playnite Fullscreen <-> Desktop Mode
            //    RB + B     : Minimize Active Window
            // ═══════════════════════════════════════════════════════════════════
            if (IsPressed(cur, PadButtons.RB) && !_altTabActive)
            {
                if (WasJustPressed(cur, PadButtons.Start))
                {
                    _rbUsedAsModifier = true;
                    _startUsedForCombo = true;
                    PlayniteBringToFrontOrRestore();
                    _prevButtons = cur;
                    return;
                }
                if (WasJustPressed(cur, PadButtons.Y))
                {
                    _rbUsedAsModifier = true;
                    PlayniteToggleFullscreenDesktopMode();
                    _prevButtons = cur;
                    return;
                }
                if (WasJustPressed(cur, PadButtons.B))
                {
                    _rbUsedAsModifier = true;
                    MinimizeActiveOrPlayniteWindow();
                    _prevButtons = cur;
                    return;
                }
            }

            // Reset modifier flags when buttons are released
            if (!IsPressed(cur, PadButtons.Start)) _startUsedForCombo = false;
            if (!IsPressed(cur, PadButtons.Back))
            {
                _backUsedForCombo = false;
                _backPressedTime = DateTime.MinValue;
            }
            if (!IsPressed(cur, PadButtons.LB)) _lbUsedAsModifier = false;
            if (!IsPressed(cur, PadButtons.RB)) _rbUsedAsModifier = false;

            // ═══════════════════════════════════════════════════════════════════
            // 6. DESKTOP CURSOR MOVEMENT + PRECISION (LT) / TURBO (RT)
            // ═══════════════════════════════════════════════════════════════════
            double moveX, moveY;
            ApplyRadialDeadzone(pad.LX, pad.LY, _config.DeadzoneLeft, _config.ResponseCurve, out moveX, out moveY);
            if (moveX != 0 || moveY != 0)
            {
                double speedMult = 1.0;
                if (pad.LT > 0.15f)
                    speedMult *= (1.0 - (1.0 - _config.SlowMultiplier) * pad.LT);
                if (pad.RT > 0.15f)
                    speedMult *= (1.0 + (_config.TurboMultiplier - 1.0) * pad.RT);

                _remMoveX += moveX * _config.BaseCursorSpeed * speedMult;
                _remMoveY += (-moveY) * _config.BaseCursorSpeed * speedMult;

                int dx = (int)_remMoveX;
                int dy = (int)_remMoveY;
                if (dx != 0 || dy != 0)
                {
                    _remMoveX -= dx;
                    _remMoveY -= dy;
                    NativeMethods.MoveMouse(dx, dy);
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // 7. SCROLL WHEEL (RIGHT STICK)
            // ═══════════════════════════════════════════════════════════════════
            double scrollH, scrollV;
            ApplyRadialDeadzone(pad.RX, pad.RY, _config.DeadzoneRight, 1.8, out scrollH, out scrollV);
            if (scrollV != 0 || scrollH != 0)
            {
                _remScrollV += scrollV * _config.ScrollSpeed;
                _remScrollH += scrollH * _config.ScrollSpeed;

                int dv = (int)_remScrollV;
                int dh = (int)_remScrollH;
                if (dv != 0 || dh != 0)
                {
                    _remScrollV -= dv;
                    _remScrollH -= dh;
                    NativeMethods.MouseScroll(dv, dh);
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // 8. CLICK & DRAG + DRAG LOCK (A = Left Click/Drag, LB + A = Drag Lock)
            // ═══════════════════════════════════════════════════════════════════
            if (WasJustPressed(cur, PadButtons.A))
            {
                if (IsPressed(cur, PadButtons.LB))
                {
                    _lbUsedAsModifier = true;
                    _suppressNextARelease = true;
                    _dragLocked = !_dragLocked;
                    if (_dragLocked)
                    {
                        if (!_leftButtonDown)
                        {
                            NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_LEFTDOWN);
                            _leftButtonDown = true;
                        }
                        _toast.ShowToast("Sürükleme Kilidi (Drag Lock): AÇIK", "Sol analogla sürükleyin; bırakmak için A'ya basın", Color.FromArgb(241, 196, 15));
                    }
                    else
                    {
                        if (_leftButtonDown)
                        {
                            NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_LEFTUP);
                            _leftButtonDown = false;
                        }
                        _toast.ShowToast("Sürükleme Kilidi: BIRAKILDI", "Normal tıklama moduna dönüldü", Color.FromArgb(46, 204, 113));
                    }
                }
                else if (_dragLocked)
                {
                    _dragLocked = false;
                    _suppressNextARelease = true;
                    if (_leftButtonDown)
                    {
                        NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_LEFTUP);
                        _leftButtonDown = false;
                    }
                    _toast.ShowToast("Sürükleme Kilidi: BIRAKILDI", "Öğeyi bıraktınız", Color.FromArgb(46, 204, 113));
                }
                else
                {
                    NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_LEFTDOWN);
                    _leftButtonDown = true;
                }
            }

            if (WasJustReleased(cur, PadButtons.A))
            {
                if (_suppressNextARelease)
                {
                    _suppressNextARelease = false;
                }
                else if (!_dragLocked && _leftButtonDown)
                {
                    NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_LEFTUP);
                    _leftButtonDown = false;
                }
            }

            // Right Click (B), Middle Click (X), On-Screen Keyboard (Y) — only when RB is not held
            if (!IsPressed(cur, PadButtons.RB))
            {
                if (WasJustPressed(cur, PadButtons.B)) NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_RIGHTDOWN);
                if (WasJustReleased(cur, PadButtons.B)) NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_RIGHTUP);

                if (WasJustPressed(cur, PadButtons.X)) NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_MIDDLEDOWN);
                if (WasJustReleased(cur, PadButtons.X)) NativeMethods.MouseButton(NativeMethods.MOUSEEVENTF_MIDDLEUP);

                if (WasJustPressed(cur, PadButtons.Y))
                {
                    ThreadPool.QueueUserWorkItem(_ => ToggleOnScreenKeyboard());
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // 9. DELAYED ALT + TAB SWITCHER VIA BACK (SELECT)
            // ═══════════════════════════════════════════════════════════════════
            if (WasJustPressed(cur, PadButtons.Back) && !IsPressed(cur, PadButtons.Start))
            {
                _backPressedTime = DateTime.UtcNow;
            }
            else if (IsPressed(cur, PadButtons.Back) && !_altTabActive && !_backUsedForCombo && _backPressedTime != DateTime.MinValue)
            {
                if ((DateTime.UtcNow - _backPressedTime).TotalMilliseconds >= 220)
                {
                    _altTabActive = true;
                    NativeMethods.KeyDown(0x12);
                    NativeMethods.KeyTap(0x09);
                }
            }
            else if (WasJustReleased(cur, PadButtons.Back))
            {
                if (_altTabActive)
                {
                    NativeMethods.KeyUp(0x12);
                    _altTabActive = false;
                }
                else if (!_backUsedForCombo)
                {
                    NativeMethods.KeyChord(new ushort[] { 0x12 }, 0x09);
                }
                _backPressedTime = DateTime.MinValue;
            }

            // ═══════════════════════════════════════════════════════════════════
            // 10. START, L3, R3, AND NON-CONFLICTING LB / RB BUMPERS
            // ═══════════════════════════════════════════════════════════════════
            if (WasJustReleased(cur, PadButtons.Start) && !_startUsedForCombo && !IsPressed(cur, PadButtons.RB))
            {
                NativeMethods.KeyTap(0x0D);
            }

            if (WasJustReleased(cur, PadButtons.LThumb))
            {
                NativeMethods.KeyTap(0x1B);
            }
            if (WasJustReleased(cur, PadButtons.RThumb))
            {
                NativeMethods.KeyChord(new ushort[] { 0x5B }, 0x09);
            }

            if (_altTabActive)
            {
                if (WasJustPressed(cur, PadButtons.RB))
                {
                    _rbUsedAsModifier = true;
                    NativeMethods.KeyTap(0x09);
                }
                if (WasJustPressed(cur, PadButtons.LB))
                {
                    _lbUsedAsModifier = true;
                    NativeMethods.KeyChord(new ushort[] { 0x10 }, 0x09);
                }
            }
            else
            {
                if (WasJustReleased(cur, PadButtons.RB) && !_rbUsedAsModifier)
                {
                    NativeMethods.KeyChord(new ushort[] { 0x11 }, 0x09);
                }
                if (WasJustReleased(cur, PadButtons.LB) && !_lbUsedAsModifier)
                {
                    NativeMethods.KeyChord(new ushort[] { 0x11, 0x10 }, 0x09);
                }
            }

            // ═══════════════════════════════════════════════════════════════════
            // 11. D-PAD ARROWS & LB + D-PAD VOLUME CONTROL
            // ═══════════════════════════════════════════════════════════════════
            if (IsPressed(cur, PadButtons.LB) && (IsPressed(cur, PadButtons.DPadUp) || IsPressed(cur, PadButtons.DPadDown)))
            {
                _lbUsedAsModifier = true;
            }

            HandleDPadKey(cur, PadButtons.DPadUp, IsPressed(cur, PadButtons.LB) ? (ushort)0xAF : (ushort)0x26);
            HandleDPadKey(cur, PadButtons.DPadDown, IsPressed(cur, PadButtons.LB) ? (ushort)0xAE : (ushort)0x28);
            HandleDPadKey(cur, PadButtons.DPadLeft, 0x25);
            HandleDPadKey(cur, PadButtons.DPadRight, 0x27);

            _prevButtons = cur;
        }

        private void HandleDPadKey(PadButtons cur, PadButtons btn, ushort vk)
        {
            if (WasJustPressed(cur, btn))
            {
                NativeMethods.KeyTap(vk, true);
                _dpadNextRepeat[btn] = DateTime.UtcNow.AddMilliseconds(380);
            }
            else if (IsPressed(cur, btn))
            {
                DateTime next;
                if (_dpadNextRepeat.TryGetValue(btn, out next) && DateTime.UtcNow >= next)
                {
                    NativeMethods.KeyTap(vk, true);
                    _dpadNextRepeat[btn] = DateTime.UtcNow.AddMilliseconds(85);
                }
            }
            else
            {
                _dpadNextRepeat.Remove(btn);
            }
        }

        protected override void ExitThreadCore()
        {
            ReleaseHeldInputsCleanly();
            if (_pollTimer != null) _pollTimer.Dispose();
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
            if (_helpOverlay != null) _helpOverlay.Dispose();
            if (_toast != null) _toast.Dispose();
            base.ExitThreadCore();
        }
    }

    static class Program
    {
        private static Mutex _singleInstanceMutex;

        [STAThread]
        static void Main()
        {
            bool createdNew;
            _singleInstanceMutex = new Mutex(true, @"Local\GamepadDesktopController_SingleInstance_Mutex", out createdNew);
            if (!createdNew)
            {
                return;
            }

            try { NativeMethods.SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GamepadDesktopContext());
        }
    }
}
