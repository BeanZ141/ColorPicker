using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using DrawingColorConverter = System.Drawing.ColorConverter;
using DrawingColor = System.Drawing.Color;
using PickerOptions;

namespace ColorPicker

{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _updateTimer; // Guessing its set to 0ms so the window tracks the cursor smoothly
        private NotifyIcon _notifyIcon;
        private const int HOTKEY_ID = 9000;
        private const int ESCAPE_HOTKEY_ID = 9001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_C = 0x43;
        private const uint VK_ESCAPE = 0x1B;
        private readonly ZoomWindow _zoomWindow;
        private bool isZoomWindowVisible = false;
        private readonly IntPtr _hookID;
        private const int WH_MOUSE_LL = 14;
        public string copiedHexCode;

        public PickerOptionsWindow _pickerOptionsWindow;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT    // What the fuck even is this
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Dont ask me where i got ts shi from :pray:
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly LowLevelMouseProc _proc;

        public MainWindow()
        {
            InitializeComponent();
            _zoomWindow = new ZoomWindow { Visibility = Visibility.Collapsed }; // Initially collapsed. Wont display if the MainWindow is collapsed
            _proc = HookCallback;
            _hookID = SetHook(_proc);

            CreateNotifyIcon();
            RegisterHotKeys();

            this.Loaded += (s, e) =>
            {
                this.Hide();
                _updateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(0)
                };
                _updateTimer.Tick += UpdateColorDisplay;
                _updateTimer.Start();
            };

            this.StateChanged += (s, e) =>
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.Hide();
                    if (isZoomWindowVisible)
                    {
                        _zoomWindow.Visibility = Visibility.Collapsed;
                        isZoomWindowVisible = false;
                    }
                }
            };
        }

        // Displays the ColorPicker
        public void ShowApp()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void ColorCodeTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CopyColorCodeToClipboard();
        }

        // Copies text to clipboard
        private void CopyColorCodeToClipboard()
        {
            copiedHexCode = ColorCodeTextBlock.Text;
            System.Windows.Forms.Clipboard.SetText(copiedHexCode);
            ShowPickerOptions();
            _pickerOptionsWindow.UpdateColors(copiedHexCode);
        }

        // Displays after the MainWindow is collapsed
        private void ShowPickerOptions()
        {
            if (_pickerOptionsWindow != null && _pickerOptionsWindow.IsVisible)
            {
                _pickerOptionsWindow.UpdateHexLabel(copiedHexCode);
            }
            else
            {
                _pickerOptionsWindow = new PickerOptionsWindow();
                _pickerOptionsWindow.Closed += (s, e) => _pickerOptionsWindow = null;
                _pickerOptionsWindow.UpdateHexLabel(copiedHexCode);
                _pickerOptionsWindow.Show();
            }
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // Logic to display/collapse ZoomWindow 
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_MOUSEWHEEL = 0x020A;
            const int WM_LBUTTONDOWN = 0x0201;

            if (nCode >= 0)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                if ((int)wParam == WM_MOUSEWHEEL)
                {
                    int wheelDelta = (short)((hookStruct.mouseData >> 16) & 0xFFFF);

                    if (wheelDelta > 0 && this.Visibility == Visibility.Visible && !isZoomWindowVisible)
                    {
                        _zoomWindow.Topmost = false;
                        _zoomWindow.Show();
                        _zoomWindow.Visibility = Visibility.Visible;
                        UpdateZoomWindowPosition(hookStruct.pt);
                        isZoomWindowVisible = true;
                    }
                    else if (wheelDelta < 0 && isZoomWindowVisible)
                    {
                        _zoomWindow.Visibility = Visibility.Collapsed;
                        isZoomWindowVisible = false;
                    }
                }
                else if ((int)wParam == WM_LBUTTONDOWN)
                {
                    if (this.Visibility == Visibility.Visible)
                    {
                        CopyColorCodeToClipboard();
                        MinimizeToTray();
                    }
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        protected override void OnClosed(EventArgs e)
        {
            UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        // Adds a menu for the application in the system tray
        private void CreateNotifyIcon()     
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(SystemIcons.Application, 40, 40),
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip
                {
                    Items =
                    {
                        new ToolStripMenuItem("Settings", null, Settings_Click),
                        new ToolStripMenuItem("Exit", null, Exit_Click)
                    }
                }
            };

            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e) { ShowApp(); }

        private void Settings_Click(object sender, EventArgs e) { System.Windows.Forms.MessageBox.Show("Settings clicked."); }

        private void Exit_Click(object sender, EventArgs e) { System.Windows.Application.Current.Shutdown(); }

        private void RegisterHotKeys()
        {
            var helper = new WindowInteropHelper(_zoomWindow)
            {
                Owner = new WindowInteropHelper(this).Handle
            };
            RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, VK_C);
            RegisterHotKey(helper.Handle, ESCAPE_HOTKEY_ID, 0, VK_ESCAPE);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg.message == WM_HOTKEY)
            {
                if ((int)msg.wParam == HOTKEY_ID)
                {
                    ShowApp();
                    handled = true;
                }
                else if ((int)msg.wParam == ESCAPE_HOTKEY_ID)
                {
                    MinimizeToTray();
                    if (isZoomWindowVisible)
                    {
                        _zoomWindow.Visibility = Visibility.Collapsed;
                        isZoomWindowVisible = false;
                    }
                    handled = true;
                }
            }
        }

        private void MinimizeToTray()
        {
            this.WindowState = WindowState.Minimized;
            this.Hide();
            if (isZoomWindowVisible)
            {
                _zoomWindow.Visibility = Visibility.Collapsed;
                isZoomWindowVisible = false;
            }
        }

        private void UpdateZoomWindowPosition(POINT cursorPosition)
        {
            double offsetX = -25;
            double offsetY = -25;

            _zoomWindow.Left = cursorPosition.x + offsetX;
            _zoomWindow.Top = cursorPosition.y + offsetY;
        }

        // Logic to Display Color
        private void UpdateColorDisplay(object sender, EventArgs e)
        {
            var cursorPosition = System.Windows.Forms.Cursor.Position;
            this.Left = cursorPosition.X + 11;
            this.Top = cursorPosition.Y - 3;
    
            using (var bitmap = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(cursorPosition.X, cursorPosition.Y, 0, 0, new System.Drawing.Size(1, 1));
                var pixelColor = bitmap.GetPixel(0, 0);
                var color = System.Windows.Media.Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G, pixelColor.B);
                ColorDisplay.Background = new SolidColorBrush(color);
                ColorCodeTextBlock.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}".ToLower();
            }  
        }
    }
}
