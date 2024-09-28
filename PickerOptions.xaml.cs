using ColorPicker;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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
            double roundedHue = Math.Round(hsl.h); // Round Hue
            double roundedSaturation = Math.Round(hsl.s * 100);
            double roundedLightness = Math.Round(hsl.l * 100);

            hslLabel.Content = $"hsl({roundedHue}, {roundedSaturation}%, {roundedLightness}%)";

            string cmykValue = RgbToCmyk(originalColor);
            cmykLabel.Content = cmykValue;
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

        public void UpdateRgbLabel(string rgbCode) { rgbLabel.Content = rgbCode.ToLower(); } // Update the RGB label

        public void UpdateHslLabel(string hslCode) { hslLabel.Content = hslCode; } // Update the HSL label

        public void UpdateCmykLabel(string cmykCode) { cmykLabel.Content = cmykCode; } // Update the CMYK label

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

                string cmykCode = RgbToCmyk(selectedColor);

                CopyToClipboard(hexCode);

                // Update other lables when the display color is clicked
                UpdateHexLabel(hexCode);
                UpdateRgbLabel(rgbCode);
                UpdateHslLabel(hslCode);
                UpdateCmykLabel(cmykCode);
            }
        }

        // Copy HEX value to clipboard
        private void CopyHexValue(object sender, MouseButtonEventArgs e)
        {
            string hexValue = hexLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(hexValue);
            ShowTooltip(sender, "Copied!");
        }

        // Copy RGB value to clipboard
        private void CopyRgbValue(object sender, MouseButtonEventArgs e)
        {
            string rgbValue = rgbLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(rgbValue);
            ShowTooltip(sender, "Copied!");
        }

        // Copy HSL value to clipboard
        private void CopyHslValue(object sender, MouseButtonEventArgs e)
        {
            string hslValue = hslLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(hslValue);
            ShowTooltip(sender, "Copied!");
        }

        // Convert RGB to CMYK
        private string RgbToCmyk(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float k = 1f - (float)Math.Max(Math.Max(r, g), b);
            if (k == 1)
            {
                return "cmyk(0%, 0%, 0%, 100%)";
            }

            float c = (1f - r - k) / (1f - k);
            float m = (1f - g - k) / (1f - k);
            float y = (1f - b - k) / (1f - k);

            c = (float)Math.Round(c * 100);
            m = (float)Math.Round(m * 100);
            y = (float)Math.Round(y * 100);
            k = (float)Math.Round(k * 100);

            return $"cmyk({c}%, {m}%, {y}%, {k}%)";
        }

        // Copy CMYK value to clipboard
        private void CopyCmykValue(object sender, MouseButtonEventArgs e)
        {
            string cmykValue = cmykLabel.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(cmykValue);
            ShowTooltip(sender, "Copied!");
        }

        // Copies a HEX when clicked on the display colors
        private void CopyToClipboard(string hexCode)
        {
            System.Windows.Forms.Clipboard.SetText(hexCode);
        }

        private void ShowTooltip(object sender, string message)
        {
            if (sender is UIElement element)
            {
                ToolTip toolTip = new ToolTip
                {
                    Content = message,
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0f0f0f")),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    FontFamily = new FontFamily("Consolas"),
                    PlacementTarget = element,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse,
                    Opacity = 0
                };

                toolTip.IsOpen = true;

                // Fade-in animation
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(100));
                toolTip.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(400)
                };
                timer.Tick += (s, args) =>
                {
                    // Fade-out animation
                    DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100));
                    fadeOut.Completed += (s2, e2) =>
                    {
                        toolTip.IsOpen = false;
                    };
                    toolTip.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                    timer.Stop();
                };
                timer.Start();
            }
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
