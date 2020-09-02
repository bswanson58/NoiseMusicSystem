using System;
using System.Windows.Media;
using MilkBottle.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.Dto {
    public class UiZoneEdit : PropertyChangeBase {
        private readonly ZoneDefinition     mZoneDefinition;
        private string                      mZoneName;
        private GroupLightLocation          mZoneLocation;
        private float                       mTop;
        private float                       mLeft;
        private float                       mHeight;
        private float                       mWidth;

        public  Color                       LegendColor { get; }
        public  string                      Location => mZoneLocation.ToString();
        public  string                      AreaDescription => $"(Top: {Top / 10.0:N0} Left:{Left / 10.0:N0} Bottom:{Bottom / 10.0:N0} Right:{Right / 10.0:N0})";

        public UiZoneEdit( ZoneDefinition zone, Color color ) {
            mZoneDefinition = zone;
            LegendColor = color;

            mZoneName = mZoneDefinition.ZoneName;
            mZoneLocation = mZoneDefinition.LightLocation;
            mTop = mZoneDefinition.ZoneArea.Top * 10;
            mLeft = mZoneDefinition.ZoneArea.Left * 10;
            mHeight = mZoneDefinition.ZoneArea.Height * 10;
            mWidth = mZoneDefinition.ZoneArea.Width * 10;
        }

        public string ZoneName {
            get => mZoneName;
            set {
                mZoneName = value;

                RaisePropertyChanged( () => mZoneName );
            }
        }

        private GroupLightLocation LightLocation {
            get => mZoneLocation;
            set {
                mZoneLocation = value;

                RaisePropertyChanged( () => LightLocation );
            }
        }

        public float Top {
            get => mTop;
            set {
                mTop = Math.Max( Math.Min( value, 950 ), 0 );
                if(( mTop + mHeight ) > 1000 ) {
                    mTop = 1000 - mHeight;
                }

                RaisePropertyChanged( () => Top );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Left {
            get => mLeft;
            set {
                mLeft = Math.Max( Math.Min( value, 950 ), 0 );
                if(( mLeft + mWidth ) > 1000 ) {
                    mLeft = 1000 - mWidth;
                }

                RaisePropertyChanged( () => Left );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Height {
            get => mHeight;
            set {
                mHeight = value;
                if( Bottom > 1000 ) {
                    mHeight = 1000 - mTop;
                }

                RaisePropertyChanged( () => Height );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Width {
            get => mWidth;
            set {
                mWidth = value;
                if( Right > 1000 ) {
                    mWidth = 1000 - Left;
                }

                RaisePropertyChanged( () => Width );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Right {
            get => mLeft + mWidth;
            set {
                mWidth = value - mLeft;
                if( Right > 1000 ) {
                    mWidth = 1000 - mLeft;
                }

                RaisePropertyChanged( () => Width );
                RaisePropertyChanged( () => Right );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Bottom {
            get => mTop + mHeight;
            set {
                mHeight = value - mTop;
                if( Bottom > 1000 ) {
                    mHeight = 1000 - mTop;
                }

                RaisePropertyChanged( () => Height );
                RaisePropertyChanged( () => Bottom );
                RaisePropertyChanged( () => AreaDescription );
            }
        }
    }
}
