using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;
using LightPipe.Dto;
using LightPipe.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    public class ZoneInfo {
        private readonly ZoneSummary    mZoneSummary;

        public  string                  ZoneId => mZoneSummary.ZoneId;
        public  List<Color>             MeanColors { get; }

        public ZoneInfo( ZoneSummary summary, List<Color> meanColors ) {
            mZoneSummary = summary;
            MeanColors = meanColors;

            while( MeanColors.Count < 4 ) {
                MeanColors.Add( Colors.Black );
            }
        }
    }

    public class LightPipeStatusViewModel : PropertyChangeBase, IDisposable, IHandle<MilkBottle.Infrastructure.Events.CurrentZoneChanged> {
        private const int                               cProcessingLimit = 60;

        private readonly IImageProcessor                mImageProcessor;
        private readonly IEventAggregator               mEventAggregator;
        private readonly BindableCollection<ZoneInfo>   mZoneSummaries;
        private IDisposable                             mImageProcessSubscription;
        private bool                                    mClearZones;

        public  ICollectionView                         ZoneSummaries { get; }
        public  string                                  ElapsedTime { get; private set; }
        public  bool                                    HighProcessingTime { get; private set; }

        public LightPipeStatusViewModel( IImageProcessor imageProcessor, IEventAggregator eventAggregator ) {
            mImageProcessor = imageProcessor;
            mEventAggregator = eventAggregator;

            mZoneSummaries = new BindableCollection<ZoneInfo>();
            ZoneSummaries = CollectionViewSource.GetDefaultView( mZoneSummaries );
            ZoneSummaries.SortDescriptions.Add( new SortDescription( nameof( ZoneInfo.ZoneId ), ListSortDirection.Ascending ));

            mImageProcessSubscription = mImageProcessor.ZoneUpdate.ObserveOnDispatcher().Subscribe( OnZoneUpdated );
            mEventAggregator.Subscribe( this );
        }

        public void Handle( MilkBottle.Infrastructure.Events.CurrentZoneChanged args ) {
            mClearZones = true;
        }

        private void OnZoneUpdated( ZoneSummary summary ) {
            if( mClearZones ) {
                mZoneSummaries.Clear();
                mClearZones = false;
            }
            else {
                var existing = mZoneSummaries.FirstOrDefault( z => z.ZoneId.Equals( summary.ZoneId ));

                mZoneSummaries.IsNotifying = false;
                if( existing != null ) {
                    mZoneSummaries.Remove( existing );
                }

                mZoneSummaries.Add( new ZoneInfo( summary, summary.FindMeanColors( 4 )));
                mZoneSummaries.IsNotifying = true;
                mZoneSummaries.Refresh();
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
