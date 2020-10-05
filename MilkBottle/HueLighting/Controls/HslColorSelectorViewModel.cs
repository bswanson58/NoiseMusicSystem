using System;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Models;

namespace HueLighting.Controls {
    public class HslColorSelectorViewModel : PropertyChangeBase {
        private double              mHueSelectorX;
        private double              mSaturationSelectorX;
        private double              mBrightnessSelectorX;
        private Color               mSelectedColor;

        public  LinearGradientBrush GradientBrush { get; }
        public  Color               HueColor { get; private set; }
        public  Color               ZeroSaturation { get; }
        public  Color               MinimumBrightness { get; private set; }
        public  Color               MaximumBrightness { get; private set; }

        public  Subject<Color>      ColorChanged { get; }

        public HslColorSelectorViewModel() {
            ColorChanged = new Subject<Color>();
            GradientBrush = CreateHueBrush();

            ZeroSaturation = Colors.White;

            MinimumBrightness = Colors.Black;
            MaximumBrightness = Colors.White;

            mHueSelectorX = 0.0;
            mBrightnessSelectorX = 50.0;
            mSaturationSelectorX = 100.0;

            OnColorSelectorMoved();
        }

        public Color SelectedColor {
            get => mSelectedColor;
            set {
                mSelectedColor = value;

                OnColorChanged();
            }
        }

        private void OnColorChanged() {
            var hslColor = new HslColor( mSelectedColor );

            mHueSelectorX = hslColor.H;
            mSaturationSelectorX = hslColor.S * 100;
            mBrightnessSelectorX = hslColor.A * 100;

            OnColorSelectorMoved();
            RaisePropertyChanged( () => HueSelectorX );
            RaisePropertyChanged( () => SaturationSelectorX );
            RaisePropertyChanged( () => BrightnessSelectorX );
        }

        public double HueSelectorX {
            get => mHueSelectorX;
            set {
                mHueSelectorX = Math.Min( Math.Max( 0, value ), 360 );

                OnColorSelectorMoved();
            }
        }

        public double SaturationSelectorX {
            get => mSaturationSelectorX;
            set {
                mSaturationSelectorX = Math.Min( Math.Max( 0, value ), 100 );

                OnColorSelectorMoved();
            }
        }

        public double BrightnessSelectorX {
            get => mBrightnessSelectorX;
            set {
                mBrightnessSelectorX = Math.Min( Math.Max( 0, value ), 100 );

                OnColorSelectorMoved();
            }
        }

        private void OnColorSelectorMoved() {
            var color = new HslColor{ H = mHueSelectorX, L = 0.5, S = 1.0 }.ToRgb();
                
            HueColor = Color.FromRgb( color.R, color.G, color.B );
            MaximumBrightness = HueColor;
            MinimumBrightness = Color.FromArgb( 25, HueColor.R, HueColor.G, HueColor.B );

            color = new HslColor{ H = mHueSelectorX, L = 0.5, S = mSaturationSelectorX / 100.0 }.ToRgb();
           
            mSelectedColor = Color.FromArgb((byte)( mBrightnessSelectorX * 2.54 ), color.R, color.G, color.B );

            RaisePropertyChanged( () => HueColor );
            RaisePropertyChanged( () => SelectedColor );
            RaisePropertyChanged( () => MinimumBrightness );
            RaisePropertyChanged( () => MaximumBrightness );

            ColorChanged.OnNext( SelectedColor );
        }

        private LinearGradientBrush CreateHueBrush() {
            var retValue = new LinearGradientBrush();
            var hslColor = new HslColor{ H = 0.0, L = 0.5, S = 1.0 };

            retValue.StartPoint = new Point( 0.0, 0.0 );
            retValue.EndPoint = new Point( 1.0, 0.0 );

            for( var hue = 0; hue < 360; hue++ ) {
                hslColor.H = hue;
                
                var stopColor = hslColor.ToRgb();
                retValue.GradientStops.Add( new GradientStop( Color.FromRgb( stopColor.R, stopColor.G, stopColor.B ), ( 1.0 / 360.0 ) * hue ));
            }

            return retValue;
        }
    }
}
