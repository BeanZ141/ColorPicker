using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;

namespace ColorPicker
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _updateTimer;
        private const int TimerInterval = 0;
        private NotifyIcon _notifyIcon;
        private const int HOTKEY_ID = 9000;
        private const int ESCAPE_HOTKEY_ID = 9001;

        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint VK_C = 0x43;
        private const uint VK_ESCAPE = 0x1B;

        private ZoomWindow _zoomWindow;


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.StateChanged += MainWindow_StateChanged;

            _zoomWindow = new ZoomWindow();
            _zoomWindow.Visibility = Visibility.Collapsed;

            CreateNotifyIcon();
            RegisterHotKeys();
            this.Topmost = true;
            
        }

        private void ColorCodeTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Key == Key.K)
            {
                if (_zoomWindow.Visibility == Visibility.Collapsed)
                {
                    _zoomWindow.Topmost = true;
                    _zoomWindow.Show();
                    _zoomWindow.Visibility = Visibility.Visible;
                }
                else
                {
                    _zoomWindow.Visibility = Visibility.Collapsed;
                    isZoomWindowPositionUpdated = false;
                }
                this.Focus();
                this.KeyDown += MainWindow_KeyDown;
            }
        }

        private void UpdateZoomWindowPosition()
        {
            var cursorPosition = System.Windows.Forms.Cursor.Position;

            _zoomWindow.Left = cursorPosition.X - (_zoomWindow.Width / 2);
            _zoomWindow.Top = cursorPosition.Y - (_zoomWindow.Height / 2);
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(SystemIcons.Application, 40, 40),
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Settings", null, Settings_Click);
            contextMenu.Items.Add("Exit", null, Exit_Click);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e) {ShowApp();}

        private void Settings_Click(object sender, EventArgs e) {System.Windows.Forms.MessageBox.Show("Settings clicked.");}

        private void Exit_Click(object sender, EventArgs e) {System.Windows.Application.Current.Shutdown();}

        private void RegisterHotKeys()
        {
            var helper = new WindowInteropHelper(this);
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
                    handled = true;
                }
            }
        }

        private void ShowApp() {this.Show(); this.WindowState = WindowState.Normal; this.Activate();}

        private void MinimizeToTray() {this.WindowState = WindowState.Minimized; this.Hide();}

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Hide();
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(TimerInterval)
            };
            _updateTimer.Tick += UpdateColorDisplay;
            _updateTimer.Start();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void UpdateColorDisplay(object sender, EventArgs e)
        {
            var cursorPosition = System.Windows.Forms.Cursor.Position;
            this.Left = cursorPosition.X + 11;
            this.Top = cursorPosition.Y + -3;

            using (var bitmap = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(cursorPosition.X, cursorPosition.Y, 0, 0, new System.Drawing.Size(1, 1));
                }

                var pixelColor = bitmap.GetPixel(0, 0);
                var color = System.Windows.Media.Color.FromArgb(pixelColor.A, pixelColor.R, pixelColor.G, pixelColor.B);
                var brush = new SolidColorBrush(color);

                ColorDisplay.Background = brush;
                ColorCodeTextBlock.Text = $"{color.R:X2}{color.G:X2}{color.B:X2}".ToLower();
            }
        }
    }
}
