using ColorPicker;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

        private void MainWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                this.DragMove();
            }
        }

        public void UpdateHexLabel(string hexCode)
        {
            hexLabel.Content = $"#{hexCode.ToLower()}"; 
        }

        // Displays colors in shapes with additional shades
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
        }

        private Color GetShade(Color color, double factor)
        {
            return Color.FromArgb(color.A, (byte)(color.R * factor), (byte)(color.G * factor), (byte)(color.B * factor));
        }

        // Updates the element's (rectangle) content and copies it to clipboard
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Rectangle rectangle && rectangle.Fill is SolidColorBrush brush)
            {
                string hexCode = $"{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}".ToLower();
                CopyToClipboard(hexCode);
            }
        }

        private void CopyToClipboard(string hexCode)
        {
            System.Windows.Forms.Clipboard.SetText(hexCode);
        }
    }
}
