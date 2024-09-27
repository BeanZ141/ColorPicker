using ColorPicker;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace PickerOptions
{
    public partial class PickerOptionsWindow : Window
    {
        public PickerOptionsWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        public void UpdateColors(string hexCode)
        {
            UpdateHexLabel(hexCode);

            Color originalColor = (Color)ColorConverter.ConvertFromString("#" + hexCode);

            rect1.Fill = new SolidColorBrush(originalColor);
            rect2.Fill = new SolidColorBrush(GetShade(originalColor, 0.8));
            rect3.Fill = new SolidColorBrush(GetShade(originalColor, 0.6));
            rect4.Fill = new SolidColorBrush(GetShade(originalColor, 0.4));
            rect5.Fill = new SolidColorBrush(GetShade(originalColor, 0.2));
            rect6.Fill = new SolidColorBrush(GetShade(originalColor, 0.1));

            rgbLabel.Content = $"rgb({originalColor.R}, {originalColor.G}, {originalColor.B})";

            var hsl = RgbToHsl(originalColor);
            hslLabel.Content = $"hsl({hsl.h}, {hsl.s}%, {hsl.l}%)";
        }

        private (double h, double s, double l) RgbToHsl(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(Math.Max(r, g), b);
            double min = Math.Min(Math.Min(r, g), b);

            double h = 0, s, l = (max + min) / 2;

            if (max == min)
            {
                h = s = 0;
            }
            else
            {
                double d = max - min;
                s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                if (max == r)
                    h = (g - b) / d + (g < b ? 6 : 0);
                else if (max == g)
                    h = (b - r) / d + 2;
                else if (max == b)
                    h = (r - g) / d + 4;

                h /= 6;
            }
            return (h * 360, s, l);
        }


        private Color GetShade(Color color, double factor)
        {
            return Color.FromArgb(color.A, (byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
        }

        public void UpdateHexLabel(string hexCode) { hexLabel.Content = $"#{hexCode.ToLower()}"; } // Update the HEX label

        public void UpdateRgbLabel(string rgbCode) { rgbLabel.Content = rgbCode.ToLower(); } // Update the HEX label

        public void UpdateHslLabel(string hslCode) { hslLabel.Content = hslCode; } // Update the HSL label

        // Copy HEX value to clipboard when rectangle is clicked
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle rectangle && rectangle.Fill is SolidColorBrush brush)
            {
                Color selectedColor = brush.Color;
                string hexCode = $"{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}".ToLower();

                string rgbCode = $"rgb({selectedColor.R}, {selectedColor.G}, {selectedColor.B})";

                (double h, double s, double l) = RgbToHsl(selectedColor);

                h = Math.Round(h);
                s = Math.Round(s * 100);
                l = Math.Round(l * 100);

                string hslCode = $"hsl({h}, {s}%, {l}%)";

                CopyToClipboard(hexCode);

                // Update other lables when the display color is clicked
                UpdateHexLabel(hexCode);
                UpdateRgbLabel(rgbCode);
                UpdateHslLabel(hslCode);
            }
        }

        // Copy HEX value to clipboard
        private void CopyHexValue(object sender, MouseButtonEventArgs e)
        {
            string hexValue = hexLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(hexValue);
        }

        // Copy RGB value to clipboard
        private void CopyRgbValue(object sender, MouseButtonEventArgs e)
        {
            string rgbValue = rgbLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(rgbValue);
        }

        // Copy HSL value to clipboard
        private void CopyHslValue(object sender, MouseButtonEventArgs e)
        {
            string hslValue = hslLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(hslValue);
        }

        // Copies a HEX when clicked on the display colors
        private void CopyToClipboard(string hexCode)
        {
            System.Windows.Forms.Clipboard.SetText(hexCode);
        }

        // Opens the Color Picker from the PickerOptions Menu and closes the PickerOptions
        private void OpenColorPicker_Click(object sender, EventArgs e)
        {
            Close();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.ShowApp();
        }
    }
}
