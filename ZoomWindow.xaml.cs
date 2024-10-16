using System;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace ColorPicker
{
    public partial class ZoomWindow : Window
    {
        private const int INITIAL_CAPTURE_SIZE = 50;
        private int currentCaptureSize = INITIAL_CAPTURE_SIZE;

        public ZoomWindow()
        {
            InitializeComponent();
        }

        public void UpdateZoomImage(System.Drawing.Point position, int zoomLevel)
        {
            currentCaptureSize = INITIAL_CAPTURE_SIZE + (zoomLevel * 10);
            if (currentCaptureSize < INITIAL_CAPTURE_SIZE) currentCaptureSize = INITIAL_CAPTURE_SIZE;

            using (var bitmap = new Bitmap(currentCaptureSize, currentCaptureSize))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen((int)position.X - currentCaptureSize / 2, (int)position.Y - currentCaptureSize / 2, 0, 0, new System.Drawing.Size(currentCaptureSize, currentCaptureSize));
                }

                var bitmapSource = ConvertToBitmapSource(bitmap);
                ZoomImageBrush.ImageSource = bitmapSource;

                this.Left = position.X - (currentCaptureSize / 2);
                this.Top = position.Y - (currentCaptureSize / 2);
            }
        }

        public BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            DeleteObject(hBitmap);
            return bitmapSource;
        }

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
    }
}
