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
        private const int CaptureSize = 50;

        public ZoomWindow()
        {
            InitializeComponent();
        }

        public void UpdateZoomImage(System.Drawing.Point position)
        {
            using (var bitmap = new Bitmap(CaptureSize, CaptureSize))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen((int)position.X - CaptureSize / 2, (int)position.Y - CaptureSize / 2, 0, 0, new System.Drawing.Size(CaptureSize, CaptureSize));
                }

                var bitmapSource = ConvertToBitmapSource(bitmap);
                ZoomImageBrush.ImageSource = bitmapSource;
            }
        }

        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
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
