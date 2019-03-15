using System;
using System.Drawing;

// from: https://richnewman.wordpress.com/about/code-listings-and-diagrams/hslcolor-class/

namespace ReusableBits.Ui.Utility {
    public class HSLColor {
        private const double    cEpsilon = 0.0001;
        private const double    cScale = 240.0;

        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double          mHue = 1.0;
        private double          mSaturation = 1.0;
        private double          mLuminosity = 1.0;

        public HSLColor() { }

        public HSLColor( Color color ) {
            SetRGB(color.R, color.G, color.B);
        }

        public HSLColor( System.Windows.Media.Color color ) {
            SetRGB( color.R, color.G, color.B );
        }

        public HSLColor( int red, int green, int blue ) {
            SetRGB( red, green, blue );
        }

        public HSLColor( double hue, double saturation, double luminosity ) {
            Hue = hue;
            Saturation = saturation;
            Luminosity = luminosity;
        }

        public void SetRGB( int red, int green, int blue ) {
            HSLColor hslColor = Color.FromArgb( red, green, blue );
            
            mHue = hslColor.mHue;
            mSaturation = hslColor.mSaturation;
            mLuminosity = hslColor.mLuminosity;
        }

        public double Hue {
            get => mHue * cScale;
            set => mHue = CheckRange(value / cScale);
        }

        public double Saturation {
            get => mSaturation * cScale;
            set => mSaturation = CheckRange(value / cScale);
        }

        public double Luminosity {
            get => mLuminosity * cScale;
            set => mLuminosity = CheckRange(value / cScale);
        }

        private double CheckRange( double value ) {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString() {
            return $"H: {Hue:#0.##} S: {Saturation:#0.##} L: {Luminosity:#0.##}";
        }

        public string ToRGBString() {
            Color color = (Color)this;

            return $"R: {color.R:#0.##} G: {color.G:#0.##} B: {color.B:#0.##}";
        }

        public static explicit operator System.Windows.Media.Color( HSLColor hslColor ) {
            double r = 0, g = 0, b = 0;

            if( Math.Abs( hslColor.mLuminosity ) > cEpsilon ) {
                if( Math.Abs( hslColor.mSaturation ) < cEpsilon ) {
                    r = g = b = hslColor.mLuminosity;
                }
                else {
                    double temp2 = GetTemp2( hslColor );
                    double temp1 = 2.0 * hslColor.mLuminosity - temp2;

                    r = GetColorComponent( temp1, temp2, hslColor.mHue + 1.0 / 3.0 );
                    g = GetColorComponent( temp1, temp2, hslColor.mHue );
                    b = GetColorComponent( temp1, temp2, hslColor.mHue - 1.0 / 3.0 );
                }
            }

            return System.Windows.Media.Color.FromRgb( Convert.ToByte( 255 * r ), Convert.ToByte( 255 * g ), Convert.ToByte( 255 * b ));
        }

        public static implicit operator Color( HSLColor hslColor ) {
            double r = 0, g = 0, b = 0;

            if( Math.Abs( hslColor.mLuminosity ) > cEpsilon ) {
                if( Math.Abs( hslColor.mSaturation ) < cEpsilon ) {
                    r = g = b = hslColor.mLuminosity;
                }
                else {
                    double temp2 = GetTemp2( hslColor );
                    double temp1 = 2.0 * hslColor.mLuminosity - temp2;

                    r = GetColorComponent( temp1, temp2, hslColor.mHue + 1.0 / 3.0 );
                    g = GetColorComponent( temp1, temp2, hslColor.mHue );
                    b = GetColorComponent( temp1, temp2, hslColor.mHue - 1.0 / 3.0 );
                }
            }

            return Color.FromArgb( Convert.ToInt32( 255 * r ), Convert.ToInt32( 255 * g ), Convert.ToInt32( 255 * b ));
        }

        private static double GetColorComponent( double temp1, double temp2, double temp3 ) {
            temp3 = MoveIntoRange( temp3 );
            if( temp3 < 1.0 / 6.0 ) {
                return temp1 + ( temp2 - temp1 ) * 6.0 * temp3;
            }

            if( temp3 < 0.5 ) {
                return temp2;
            }

            if( temp3 < 2.0 / 3.0 ) {
                return temp1 + (( temp2 - temp1 ) * (( 2.0 / 3.0 ) - temp3 ) * 6.0 );
            }

            return temp1;
        }

        private static double MoveIntoRange(double temp3) {
            if( temp3 < 0.0 ) {
                temp3 += 1.0;
            }
            else if( temp3 > 1.0 ) {
                temp3 -= 1.0;
            }

            return temp3;
        }

        private static double GetTemp2( HSLColor hslColor ) {
            double temp2;

            if( hslColor.mLuminosity < 0.5 ) { //<=??
                temp2 = hslColor.mLuminosity * ( 1.0 + hslColor.mSaturation );
            }
            else {
                temp2 = hslColor.mLuminosity + hslColor.mSaturation - ( hslColor.mLuminosity * hslColor.mSaturation );
            }

            return temp2;
        }

        public static implicit operator HSLColor( Color color ) {
            HSLColor hslColor = new HSLColor();

            hslColor.mHue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.mLuminosity = color.GetBrightness();
            hslColor.mSaturation = color.GetSaturation();

            return hslColor;
        }
    }
}
