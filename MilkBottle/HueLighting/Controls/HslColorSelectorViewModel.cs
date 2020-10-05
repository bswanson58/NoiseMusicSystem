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
        private double              mLightnessSelectorX;
        private Color               mSelectedColor;

        public  LinearGradientBrush GradientBrush { get; }
        public  Color               HueColor { get; private set; }
        public  Color               ZeroSaturation { get; }
        public  Color               MinimumLightness { get; }
        public  Color               MaximumLightness { get; }

        public  Subject<Color>      ColorChanged { get; }

        public HslColorSelectorViewModel() {
            ColorChanged = new Subject<Color>();
            GradientBrush = CreateHueBrush();

            ZeroSaturation = Colors.Gray;

            MinimumLightness = Colors.Black;
            MaximumLightness = Colors.White;

            mHueSelectorX = 0.0;
            mLightnessSelectorX = 50.0;
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
            var hslColor = new HslColor( SelectedColor );

            HueSelectorX = hslColor.H;
            SaturationSelectorX = hslColor.S * 100;
            LightnessSelectorX = hslColor.L * 100;

            RaisePropertyChanged( () => HueSelectorX );
            RaisePropertyChanged( () => SaturationSelectorX );
            RaisePropertyChanged( () => LightnessSelectorX );
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

        public double LightnessSelectorX {
            get => mLightnessSelectorX;
            set {
                mLightnessSelectorX = Math.Min( Math.Max( 0, value ), 100 );

                OnColorSelectorMoved();
            }
        }

        private void OnColorSelectorMoved() {
            var color = new HslColor{ H = mHueSelectorX, L = 0.5, S = 1.0 }.ToRgb();
                
            HueColor = Color.FromRgb( color.R, color.G, color.B );

            color = new HslColor{ H = mHueSelectorX, L = mLightnessSelectorX / 100.0, S = mSaturationSelectorX / 100.0 }.ToRgb();
           
            mSelectedColor = Color.FromRgb( color.R, color.G, color.B );

            RaisePropertyChanged( () => HueColor );
            RaisePropertyChanged( () => SelectedColor );

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
