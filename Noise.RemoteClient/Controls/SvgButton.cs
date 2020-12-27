using System;
using System.Linq;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Noise.RemoteClient.Controls {
    public class SvgButton : Frame {
        private readonly SKCanvasView   mCanvasView;
        private TapGestureRecognizer    mTapGestureRecognizer;

        public static readonly BindableProperty SourceProperty = 
            BindableProperty.Create( nameof( Source ), typeof( string ), typeof( SvgButton ), default( string ), propertyChanged: RedrawCanvas );

        public string Source {
            get => (string)GetValue( SourceProperty );
            set => SetValue( SourceProperty, value );
        }

        public static readonly BindableProperty CommandProperty = 
            BindableProperty.Create( nameof( Command ), typeof( ICommand ), typeof( SvgButton ), propertyChanged: OnCommandChanged  );

        public ICommand Command {
            get => GetValue( CommandProperty ) as ICommand;
            set => SetValue( CommandProperty, value );
        }

        public static readonly BindableProperty ImageColorProperty = 
            BindableProperty.Create( nameof( ImageColor ), typeof( Color ), typeof( SvgButton ), Color.Black, propertyChanged: OnColorChanged  );

        public Color ImageColor {
            get => (Color)GetValue( ImageColorProperty );
            set => SetValue( ImageColorProperty, value );
        }

        private static void OnColorChanged( BindableObject sender, object oldValue, object newValue ) { }

        public SvgButton() {
            mCanvasView = new SKCanvasView();

            Padding = new Thickness( 0 );
            BackgroundColor = Color.Transparent;
            HasShadow = false;
            Content = mCanvasView;

            mCanvasView.PaintSurface += CanvasViewOnPaintSurface;
        }

        private static void OnCommandChanged( BindableObject sender, object oldValue, object newValue ) {
            if( sender is SvgButton svgImage ) {
                if(( svgImage.Command != null ) &&
                   ( svgImage.mTapGestureRecognizer == null )) {
                    svgImage.mTapGestureRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
                    svgImage.mCanvasView.GestureRecognizers.Add( svgImage.mTapGestureRecognizer );
                }

                if( svgImage.mTapGestureRecognizer != null ) {
                    svgImage.mTapGestureRecognizer.Command = svgImage.Command;
                }
            }
        }

        private static void RedrawCanvas( BindableObject bindable, object oldValue, object newValue ) {
            var svgIcon = bindable as SvgButton;
            svgIcon?.mCanvasView.InvalidateSurface();
        }

        private void CanvasViewOnPaintSurface( object sender, SKPaintSurfaceEventArgs args ) {
            if( string.IsNullOrEmpty( Source )) {
                return;
            }

            using( var canvas = args.Surface.Canvas ) {
                canvas.Clear();

                using( var stream = GetType().Assembly.GetManifestResourceStream( ResourceName( Source ))) {
                    if( stream != null ) {
                        var svg = new SKSvg();

                        svg.Load( stream );

                        var info = args.Info;
                        canvas.Translate( info.Width / 2f, info.Height / 2f );

                        var bounds = svg.ViewBox;

                        bounds.Inflate( (float)( Padding.Left + Padding.Right ), (float)( Padding.Top + Padding.Bottom ));
                        bounds.Offset( (float)( Padding.Left - Padding.Right ), (float)( Padding.Top - Padding.Bottom ));

                        float xRatio = info.Width / bounds.Width;
                        float yRatio = info.Height / bounds.Height;

                        float ratio = Math.Min( xRatio, yRatio );

                        canvas.Scale( ratio );
                        canvas.Translate( -bounds.MidX, -bounds.MidY );

                        if(!ImageColor.Equals( Color.Black )) {
                            using (var paint = new SKPaint()) {
                                paint.ColorFilter = SKColorFilter.CreateBlendMode( SKColor.Parse( ImageColor.ToHex()), SKBlendMode.SrcIn );

                                canvas.DrawPicture( svg.Picture, paint );
                            }
                        }
                        else {
                            canvas.DrawPicture( svg.Picture );
                        }
                    }
                }
            }
        }

        private string ResourceName( string sourceName ) {
            var retValue = sourceName;

            if(!String.IsNullOrWhiteSpace( sourceName )) {
                if(!sourceName.ToLower().EndsWith( ".svg" )) {
                    var allResources = GetType().Assembly.GetManifestResourceNames();
                    var matchedResource = allResources.FirstOrDefault( r => r.EndsWith( sourceName + ".svg" ));

                    if( matchedResource != null ) {
                        retValue = matchedResource;
                    }
                }
            }

            return retValue;
        }
    }
}
