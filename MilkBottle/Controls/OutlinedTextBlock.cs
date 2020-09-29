using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace MilkBottle.Controls {
    // from: https://stackoverflow.com/questions/93650/apply-stroke-to-a-textblock-in-wpf

    public enum StrokePosition {
        Center,
        Outside,
        Inside
    }

    [ContentProperty( "Text" )]
    public class OutlinedTextBlock : FrameworkElement {
        private void UpdatePen() {
            mPen = new Pen( Stroke, StrokeThickness ) {
                DashCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round,
                StartLineCap = PenLineCap.Round
            };

            if( StrokePosition == StrokePosition.Outside || StrokePosition == StrokePosition.Inside ) {
                mPen.Thickness = StrokeThickness * 2;
            }

            InvalidateVisual();
        }

        public StrokePosition StrokePosition {
            get => (StrokePosition)GetValue( StrokePositionProperty );
            set => SetValue( StrokePositionProperty, value );
        }

        public static readonly DependencyProperty StrokePositionProperty =
            DependencyProperty.Register( "StrokePosition",
                typeof( StrokePosition ),
                typeof( OutlinedTextBlock ),
                new FrameworkPropertyMetadata( StrokePosition.Outside, FrameworkPropertyMetadataOptions.AffectsRender ) );

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
          "Fill",
          typeof( Brush ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender ) );

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
          "Stroke",
          typeof( Brush ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender ) );

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
          "StrokeThickness",
          typeof( double ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( 1d, FrameworkPropertyMetadataOptions.AffectsRender ) );

        public static readonly DependencyProperty FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner(
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty FontSizeProperty = TextElement.FontSizeProperty.AddOwner(
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty FontStretchProperty = TextElement.FontStretchProperty.AddOwner(
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty FontStyleProperty = TextElement.FontStyleProperty.AddOwner(
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty FontWeightProperty = TextElement.FontWeightProperty.AddOwner(
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
          "Text",
          typeof( string ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextInvalidated ) );

        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
          "TextAlignment",
          typeof( TextAlignment ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty TextDecorationsProperty = DependencyProperty.Register(
          "TextDecorations",
          typeof( TextDecorationCollection ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty TextTrimmingProperty = DependencyProperty.Register(
          "TextTrimming",
          typeof( TextTrimming ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( OnFormattedTextUpdated ) );

        public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register(
          "TextWrapping",
          typeof( TextWrapping ),
          typeof( OutlinedTextBlock ),
          new FrameworkPropertyMetadata( TextWrapping.NoWrap, OnFormattedTextUpdated ) );

        private FormattedText mFormattedText;
        private Geometry mTextGeometry;
        private Pen mPen;
        private PathGeometry mClipGeometry;

        public Brush Fill {
            get => (Brush)GetValue( FillProperty );
            set => SetValue( FillProperty, value );
        }

        public FontFamily FontFamily {
            get => (FontFamily)GetValue( FontFamilyProperty );
            set => SetValue( FontFamilyProperty, value );
        }

        [TypeConverter( typeof( FontSizeConverter ) )]
        public double FontSize {
            get => (double)GetValue( FontSizeProperty );
            set => SetValue( FontSizeProperty, value );
        }

        public FontStretch FontStretch {
            get => (FontStretch)GetValue( FontStretchProperty );
            set => SetValue( FontStretchProperty, value );
        }

        public FontStyle FontStyle {
            get => (FontStyle)GetValue( FontStyleProperty );
            set => SetValue( FontStyleProperty, value );
        }

        public FontWeight FontWeight {
            get => (FontWeight)GetValue( FontWeightProperty );
            set => SetValue( FontWeightProperty, value );
        }

        public Brush Stroke {
            get => (Brush)GetValue( StrokeProperty );
            set => SetValue( StrokeProperty, value );
        }

        public double StrokeThickness {
            get => (double)GetValue( StrokeThicknessProperty );
            set => SetValue( StrokeThicknessProperty, value );
        }

        public string Text {
            get => (string)GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }

        public TextAlignment TextAlignment {
            get => (TextAlignment)GetValue( TextAlignmentProperty );
            set => SetValue( TextAlignmentProperty, value );
        }

        public TextDecorationCollection TextDecorations {
            get => (TextDecorationCollection)GetValue( TextDecorationsProperty );
            set => SetValue( TextDecorationsProperty, value );
        }

        public TextTrimming TextTrimming {
            get => (TextTrimming)GetValue( TextTrimmingProperty );
            set => SetValue( TextTrimmingProperty, value );
        }

        public TextWrapping TextWrapping {
            get => (TextWrapping)GetValue( TextWrappingProperty );
            set => SetValue( TextWrappingProperty, value );
        }

        public OutlinedTextBlock() {
            UpdatePen();
            TextDecorations = new TextDecorationCollection();
        }

        protected override void OnRender( DrawingContext drawingContext ) {
            EnsureGeometry();

            drawingContext.DrawGeometry( Fill, null, mTextGeometry );

            if( StrokePosition == StrokePosition.Outside ) {
                drawingContext.PushClip( mClipGeometry );
            }
            else if( StrokePosition == StrokePosition.Inside ) {
                drawingContext.PushClip( mTextGeometry );
            }

            drawingContext.DrawGeometry( null, mPen, mTextGeometry );

            if( StrokePosition == StrokePosition.Outside || StrokePosition == StrokePosition.Inside ) {
                drawingContext.Pop();
            }
        }
        /*
        protected override Size MeasureOverride( Size availableSize ) {
            EnsureFormattedText();

            // constrain the formatted text according to the available size

            double w = availableSize.Width;
            double h = availableSize.Height;

            // the Math.Min call is important - without this constraint (which seems arbitrary, but is the maximum allowable text width), things blow up when availableSize is infinite in both directions
            // the Math.Max call is to ensure we don't hit zero, which will cause MaxTextHeight to throw
            mFormattedText.MaxTextWidth = Math.Min( 3579139, w );
            mFormattedText.MaxTextHeight = Math.Max( 0.0001d, h );

            // return the desired size
            return new Size( Math.Ceiling( mFormattedText.Width ), Math.Ceiling( mFormattedText.Height ) );
        }
        */
        protected override Size MeasureOverride( Size availableSize ) {
            EnsureFormattedText();

            if( mFormattedText == null ) {
                mFormattedText = new FormattedText(
                    Text ?? String.Empty,
                    CultureInfo.CurrentUICulture,
                    FlowDirection,
                    new Typeface( FontFamily, FontStyle, FontWeight, FontStretches.Normal ),
                    FontSize,
                    Brushes.Black, 
                    VisualTreeHelper.GetDpi( this ).PixelsPerDip );
            }

            // constrain the formatted text according to the available size
            // the Math.Min call is important - without this constraint (which seems arbitrary, but is the maximum allowable text width), things blow up when availableSize is infinite in both directions
            mFormattedText.MaxTextWidth = Math.Min( 3579139, availableSize.Width );
            mFormattedText.MaxTextHeight = availableSize.Height;

            // return the desired size
            return new Size( mFormattedText.Width, mFormattedText.Height );
        }

        protected override Size ArrangeOverride( Size finalSize ) {
            EnsureFormattedText();

            // update the formatted text with the final size
            mFormattedText.MaxTextWidth = finalSize.Width;
            mFormattedText.MaxTextHeight = Math.Max( 0.0001d, finalSize.Height );

            // need to re-generate the geometry now that the dimensions have changed
            mTextGeometry = null;
            UpdatePen();

            return finalSize;
        }

        private static void OnFormattedTextInvalidated( DependencyObject dependencyObject,
          DependencyPropertyChangedEventArgs e ) {
            var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
            outlinedTextBlock.mFormattedText = null;
            outlinedTextBlock.mTextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private static void OnFormattedTextUpdated( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e ) {
            var outlinedTextBlock = (OutlinedTextBlock)dependencyObject;
            outlinedTextBlock.UpdateFormattedText();
            outlinedTextBlock.mTextGeometry = null;

            outlinedTextBlock.InvalidateMeasure();
            outlinedTextBlock.InvalidateVisual();
        }

        private void EnsureFormattedText() {
            if( mFormattedText != null ) {
                return;
            }

            mFormattedText = new FormattedText( 
                Text ?? String.Empty, 
                CultureInfo.CurrentUICulture,
                FlowDirection, 
                new Typeface( FontFamily, FontStyle, FontWeight, FontStretch ),
                FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi( this ).PixelsPerDip );

            UpdateFormattedText();
        }

        private void UpdateFormattedText() {
            if( mFormattedText == null ) {
                return;
            }

            mFormattedText.MaxLineCount = TextWrapping == TextWrapping.NoWrap ? 1 : int.MaxValue;
            mFormattedText.TextAlignment = TextAlignment;
            mFormattedText.Trimming = TextTrimming;

            mFormattedText.SetFontSize( FontSize );
            mFormattedText.SetFontStyle( FontStyle );
            mFormattedText.SetFontWeight( FontWeight );
            mFormattedText.SetFontFamily( FontFamily );
            mFormattedText.SetFontStretch( FontStretch );
            mFormattedText.SetTextDecorations( TextDecorations );
        }

        private void EnsureGeometry() {
            if( mTextGeometry != null ) {
                return;
            }

            EnsureFormattedText();
            mTextGeometry = mFormattedText.BuildGeometry( new Point( 0, 0 ) );

            if( StrokePosition == StrokePosition.Outside ) {
                var boundsGeo = new RectangleGeometry( new Rect( 0, 0, ActualWidth, ActualHeight ) );
                mClipGeometry = Geometry.Combine( boundsGeo, mTextGeometry, GeometryCombineMode.Exclude, null );
            }
        }
    }
}