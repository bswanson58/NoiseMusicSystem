using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
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

    public class LightPipeStatusViewModel : PropertyChangeBase, IDisposable, IHandle<MilkBottle.Infrastructure.Events.CurrentZoneChanged> {
        private const int                           cProcessingLimit = 60;

        private readonly IImageProcessor            mImageProcessor;
        private readonly IEventAggregator           mEventAggregator;
        private IDisposable                         mImageProcessSubscription;
        private bool                                mClearZones;

        public  ObservableCollection<ZoneInfo>      ZoneSummaries { get; }
        public  string                              ElapsedTime { get; private set; }
        public  bool                                HighProcessingTime { get; private set; }

        public LightPipeStatusViewModel( IImageProcessor imageProcessor, IEventAggregator eventAggregator ) {
            mImageProcessor = imageProcessor;
            mEventAggregator = eventAggregator;

            ZoneSummaries = new ObservableCollection<ZoneInfo>();

            mImageProcessSubscription = mImageProcessor.ZoneUpdate.Subscribe( OnZoneUpdated );
            mEventAggregator.Subscribe( this );
        }

        public void Handle( MilkBottle.Infrastructure.Events.CurrentZoneChanged args ) {
            mClearZones = true;
        }

        private void OnZoneUpdated( ZoneSummary summary ) {
            if( mClearZones ) {
                ZoneSummaries.Clear();
                mClearZones = false;
            }
            else {
                var existing = ZoneSummaries.FirstOrDefault( z => z.ZoneId.Equals( summary.ZoneId ));

                if( existing != null ) {
                    ZoneSummaries.Remove( existing );
                }

                ZoneSummaries.Add( new ZoneInfo( summary, summary.FindMeanColors( 4 )));
            }

            ElapsedTime = $"{mImageProcessor.ElapsedTime}";
            HighProcessingTime = mImageProcessor.ElapsedTime > cProcessingLimit;

            RaisePropertyChanged( () => ElapsedTime );
            RaisePropertyChanged( () => HighProcessingTime );
        }

        public void Dispose() {
            mImageProcessSubscription?.Dispose();
            mImageProcessSubscription = null;

            mEventAggregator.Unsubscribe( this );
        }
    }
}
