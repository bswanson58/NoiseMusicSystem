using System;
using System.Windows.Input;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Noise.RemoteClient.Controls {
    public class SvgButton : Frame {
        private readonly SKCanvasView           mCanvasView;
        private readonly TapGestureRecognizer   mTapGestureRecognizer;

        public static readonly BindableProperty ResourceIdProperty = 
            BindableProperty.Create( nameof( Source ), typeof( string ), typeof( SvgButton ), default( string ), propertyChanged: RedrawCanvas );

        public string Source {
            get => (string)GetValue( ResourceIdProperty );
            set => SetValue( ResourceIdProperty, value );
        }

        public static readonly BindableProperty CommandProperty = 
            BindableProperty.Create( nameof( Command ), typeof( ICommand ), typeof( SvgButton ), propertyChanged: OnCommandChanged  );

        public ICommand Command {
            get => GetValue( CommandProperty ) as ICommand;
            set => SetValue( CommandProperty, value );
        }

        public SvgButton() {
            mCanvasView = new SKCanvasView();
            mTapGestureRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = 1 };

            Padding = new Thickness( 0 );
            BackgroundColor = Color.Transparent;
            HasShadow = false;
            Content = mCanvasView;

            mCanvasView.PaintSurface += CanvasViewOnPaintSurface;
            mCanvasView.GestureRecognizers.Add( mTapGestureRecognizer );
        }

        private static void OnCommandChanged( BindableObject sender, object oldValue, object newValue ) {
            if( sender is SvgButton svgImage ) {
                svgImage.mTapGestureRecognizer.Command = svgImage.Command;
            }
        }

        private static void RedrawCanvas( BindableObject bindable, object oldValue, object newValue ) {
            var svgIcon = bindable as SvgButton;
            svgIcon?.mCanvasView.InvalidateSurface();
        }

        private void CanvasViewOnPaintSurface( object sender, SKPaintSurfaceEventArgs args ) {
            var canvas = args.Surface.Canvas;

            canvas.Clear();

            if( string.IsNullOrEmpty( Source )) {
                return;
            }

            using( var stream = GetType().Assembly.GetManifestResourceStream( Source )) {
                if( stream != null ) {
                    var svg = new SKSvg();

                    svg.Load( stream );

                    var info = args.Info;
                    canvas.Translate( info.Width / 2f, info.Height / 2f );

                    var bounds = svg.ViewBox;
                    float xRatio = info.Width / bounds.Width;
                    float yRatio = info.Height / bounds.Height;

                    float ratio = Math.Min( xRatio, yRatio );

                    canvas.Scale( ratio );
                    canvas.Translate( -bounds.MidX, -bounds.MidY );

                    canvas.DrawPicture( svg.Picture );
                }
            }
        }
    }
}
