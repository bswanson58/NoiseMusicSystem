using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LightPipe.Dto;
using LightPipe.Interfaces;
using LightPipe.Utility;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    public class ZoneInfo {
        private readonly ZoneSummary    mZoneSummary;

        public  string                  ZoneId => mZoneSummary.ZoneId;
        public  List<ColorBin>          Colors => mZoneSummary.Colors;
        public  List<Color>             MeanColors { get; }

        public ZoneInfo( ZoneSummary summary, List<Color> meanColors ) {
            mZoneSummary = summary;
            MeanColors = meanColors;
        }
    }

    public class LightPipeStatusViewModel : PropertyChangeBase, IDisposable {
        private readonly IImageProcessor            mImageProcessor;
        private IDisposable                         mImageProcessSubscription;

        public  ObservableCollection<ZoneInfo>      ZoneSummaries { get; }
        public  string                              ElapsedTime { get; private set; }

        public LightPipeStatusViewModel( IImageProcessor imageProcessor ) {
            mImageProcessor = imageProcessor;

            ZoneSummaries = new ObservableCollection<ZoneInfo>();

            mImageProcessSubscription = mImageProcessor.ZoneUpdate.Subscribe( OnZoneUpdated );
        }

        private void OnZoneUpdated( ZoneSummary summary ) {
            var existing = ZoneSummaries.FirstOrDefault( z => z.ZoneId.Equals( summary.ZoneId ));

            if( existing != null ) {
                ZoneSummaries.Remove( existing );
            }

            ZoneSummaries.Add( new ZoneInfo( summary, FindMeanColors( summary.Colors, 3 )));

            ElapsedTime = $"{mImageProcessor.ElapsedTime} ms";
            RaisePropertyChanged( () => ElapsedTime );
        }

        private List<Color> FindMeanColors( List<ColorBin> colorList, int maxColors ) {
            var retValue = new List<Color>();

            if( colorList.Count > maxColors ) {
                var sortedColors = ( from bin in colorList orderby HueOf( bin.Color ) select bin.Color ).ToList();
                var takeIndex = colorList.Count / (float)( maxColors - 1 );

                // Start with the predominate color
                retValue.Add( colorList.First().Color );
                retValue.AddRange( sortedColors.Where(( bin, index ) => index % (int)Math.Round( takeIndex ) == 0 ));

                if( retValue.Count > maxColors ) {
                    retValue.RemoveAt( retValue.Count - 1 );
                }
                if( retValue.Count < maxColors ) {
                    retValue.Add( sortedColors.Last());
                }
            }
            else {
                retValue.AddRange( from bin in colorList select bin.Color );
            }

            return retValue;
        }

        private double HueOf( Color color ) {
            ColorSpace.ColorToHSV( color, out var hue, out var saturation, out var value );

            return hue;
        }

        public void Dispose() {
            mImageProcessSubscription?.Dispose();
            mImageProcessSubscription = null;
        }
    }
}
