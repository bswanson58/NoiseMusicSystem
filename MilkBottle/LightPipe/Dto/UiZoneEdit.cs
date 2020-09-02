using System;
using System.Drawing;
using MilkBottle.Infrastructure.Dto;
using ReusableBits.Mvvm.ViewModelSupport;
using Color = System.Windows.Media.Color;

namespace LightPipe.Dto {
    public class UiZoneEdit : PropertyChangeBase {
        private string                      mZoneName;
        private GroupLightLocation          mZoneLocation;
        private float                       mTop;
        private float                       mLeft;
        private float                       mHeight;
        private float                       mWidth;

        public  Color                       LegendColor { get; }
        public  string                      Location => mZoneLocation.ToString();
        public  string                      AreaDescription => $"(Top: {Top / 10.0:N0}, Left: {Left / 10.0:N0}, Bottom: {Bottom / 10.0:N0}, Right: {Right / 10.0:N0})";

        public UiZoneEdit( ZoneDefinition zoneDefinition, Color color ) {
            LegendColor = color;

            mZoneName = zoneDefinition.ZoneName;
            mZoneLocation = zoneDefinition.LightLocation;
            mTop = zoneDefinition.ZoneArea.Top * 10;
            mLeft = zoneDefinition.ZoneArea.Left * 10;
            mHeight = zoneDefinition.ZoneArea.Height * 10;
            mWidth = zoneDefinition.ZoneArea.Width * 10;
        }

        public ZoneDefinition GetUpdatedZone() {
            return new ZoneDefinition( ZoneName, 
                                       new RectangleF((float)Math.Round( Left / 10.0 ), (float)Math.Round( Top / 10.0 ), (float)Math.Round( Width / 10.0 ), (float)Math.Round( Height / 10.0 )),
                                       LightLocation );
        }

        public string ZoneName {
            get => mZoneName;
            set {
                mZoneName = value;

                RaisePropertyChanged( () => ZoneName );
            }
        }

        public GroupLightLocation LightLocation {
            get => mZoneLocation;
            set {
                mZoneLocation = value;

                RaisePropertyChanged( () => Location );
                RaisePropertyChanged( () => LightLocation );
            }
        }

        public float Top {
            get => mTop;
            set {
                mTop = (float)Math.Round( Math.Max( Math.Min( value, 950 ), 0 ));
                if(( mTop + mHeight ) > 1000 ) {
                    mTop = 1000 - mHeight;
                }

                RaisePropertyChanged( () => Top );
                RaisePropertyChanged( () => Bottom );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Left {
            get => mLeft;
            set {
                mLeft = (float)( Math.Round( Math.Max( Math.Min( value, 950 ), 0 )));
                if(( mLeft + mWidth ) > 1000 ) {
                    mLeft = 1000 - mWidth;
                }

                RaisePropertyChanged( () => Left );
                RaisePropertyChanged( () => Right );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Height {
            get => mHeight;
            set {
                mHeight = (float)Math.Round( value );
                if( Bottom > 1000 ) {
                    mHeight = 1000 - mTop;
                }

                RaisePropertyChanged( () => Height );
                RaisePropertyChanged( () => Bottom );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Width {
            get => mWidth;
            set {
                mWidth = (float)Math.Round( value );
                if( Right > 1000 ) {
                    mWidth = 1000 - Left;
                }

                RaisePropertyChanged( () => Width );
                RaisePropertyChanged( () => Right );
                RaisePropertyChanged( () => AreaDescription );
            }
        }

        public float Right {
            get => mLeft + mWidth;
            set {
                mWidth = (float)Math.Round( value ) - mLeft;
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
                mHeight = (float)Math.Round( value ) - mTop;
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
