using Org.DocUI.FormBuilder;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Org.DocUI.Wpf
{
    /// <summary>
    /// Textbox with extended functionalities. It can contain an icon.
    /// It turns red when the assigned regular expression is false. It can also contain
    /// a watermark. Finally it can be expandable using an ExpanderBox.
    /// </summary>
    public partial class ExtendedTextBox : UserControl
    {
        private Regex _regeExpr;
        private bool _required;
        private DynamicForm _parentForm;
        private string _watermarkText;
        /// <summary>
        /// The color for a correct value.
        /// </summary>
        public static SolidColorBrush CorrectColor = Brushes.White;
        /// <summary>
        /// The color for an incorrect value.
        /// </summary>
        public static SolidColorBrush IncorrectColor = Brushes.MistyRose;

        /// <summary>
        /// Creates a new instance of an ExtendedTextbox.
        /// </summary>
        /// <param name="iconPath">path to the icon that should be displayed.</param>
        /// <param name="watermarkText">The watermark text.</param>
        /// <param name="regexCondition">The regularexpresion that should be validated.</param>
        /// <param name="required">Whether this textbox needs a content.</param>
        /// <param name="parentForm">The form in which this textbox is located.</param>
        public ExtendedTextBox(string iconPath, string watermarkText, string regexCondition, bool required, DynamicForm parentForm)
        {
            InitializeComponent();
            this._parentForm = parentForm;
            this._required = required;
            this._watermarkText = watermarkText;

            // Initiate most of the content áfter everything is loaded.
            // This is necessary to know the textboxes width and height.
            this.Loaded += (object sender, RoutedEventArgs args) =>
            {
                int image_width = 0;

                if (File.Exists(iconPath))
                {
                    BitmapImage bmp = new BitmapImage(new Uri(iconPath));
                    image_width = (int)((this.ActualHeight * bmp.PixelWidth) / bmp.PixelHeight);
                    ImageDrawing i = new ImageDrawing(bmp, new Rect(new Size(image_width, this.ActualHeight)));

                    img.Source = new DrawingImage(i);
                }

                watermark.Text = watermarkText;
                watermark.Foreground = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));

                _regeExpr = new Regex(regexCondition);

                TextBlock.LostFocus += CheckWatermark;
                TextBlock.LostFocus += CheckRegex;
                TextBlock.TextChanged += CheckWatermark;
                TextBlock.TextChanged += CheckRegex;
                TextBlock.GotFocus += RemoveWatermark;
                TextBlock.GotFocus += CheckRegex;

                CheckWatermark(this, EventArgs.Empty);
            };

        }

        /// <summary>
        /// Checks if the watermark text should be visible or not.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void CheckWatermark(object sender, EventArgs args)
        {
            watermark.Visibility = TextBlock.Text != "" ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Removes the watermark text from the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void RemoveWatermark(object sender, EventArgs args)
        {
            watermark.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Checks if the regular expression is matched.
        /// If so, the color of the textbox should change accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void CheckRegex(object sender, EventArgs args)
        {
            if (!Valid())
            {
                TextBlock.Background = IncorrectColor;
            }
            else
            {
                TextBlock.Background = CorrectColor;
            }
        }

        public bool Valid()
        {
            return !((_required && TextBlock.Text == "") || !_regeExpr.Match(TextBlock.Text).Success);
        }
    }
}
