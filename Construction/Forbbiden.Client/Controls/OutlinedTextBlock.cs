using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Forbbiden.Client.Controls
{
    public class OutlinedTextBlock : FrameworkElement
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty =
            DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(OutlinedTextBlock),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        public Brush Stroke { get => (Brush)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
        public FontFamily FontFamily { get => (FontFamily)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

        private FormattedText CreateFormattedText()
        {
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            return new FormattedText(
                Text ?? string.Empty,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
                FontSize,
                Fill,
                dpi)
            {
                Trimming = TextTrimming.None
            };
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var ft = CreateFormattedText();

            double width = ft.WidthIncludingTrailingWhitespace + StrokeThickness;
            double height = ft.Height + StrokeThickness;

            width = Math.Min(width, double.IsInfinity(availableSize.Width) ? width : availableSize.Width);
            height = Math.Min(height, double.IsInfinity(availableSize.Height) ? height : availableSize.Height);

            return new Size(width, height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var ft = CreateFormattedText();

            double x = 0;
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    x = (ActualWidth - ft.WidthIncludingTrailingWhitespace) / 2;
                    break;
                case HorizontalAlignment.Right:
                    x = ActualWidth - ft.WidthIncludingTrailingWhitespace;
                    break;
                default:
                    x = 0;
                    break;
            }

            double y = 0;
            // FormattedText.Height representa el alto total; centrar verticalmente
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    y = (ActualHeight - ft.Height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    y = ActualHeight - ft.Height;
                    break;
                default:
                    y = 0;
                    break;
            }

            var geometry = ft.BuildGeometry(new Point(x, y));
            drawingContext.DrawGeometry(Fill, new Pen(Stroke, StrokeThickness), geometry);
        }
    }
}
