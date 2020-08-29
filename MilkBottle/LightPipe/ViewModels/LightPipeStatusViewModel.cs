using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LightPipe.Dto;
using LightPipe.Interfaces;
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

            ZoneSummaries.Add( new ZoneInfo( summary, summary.FindMeanColors( 4 )));

            ElapsedTime = $"{mImageProcessor.ElapsedTime} ms";
            RaisePropertyChanged( () => ElapsedTime );
        }

        public void Dispose() {
            mImageProcessSubscription?.Dispose();
            mImageProcessSubscription = null;
        }
    }
}
