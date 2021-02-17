using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class BulkTrackRenameViewModel : PropertyChangeBase, IDialogAware {
        public  const string                        cTrackListParameter = "trackList";

        private readonly ITrackProvider             mTrackProvider;
        private readonly IEventAggregator           mEventAggregator;
        private string                              mReplacementName;

        public  BindableCollection<UiSelectableTrackNode>   TrackList { get; }

        public  string                              Title { get; }
        public  string                              TrackCount { get; private set; }
        public  string                              SelectedTrackCount { get; private set; }

        public  DelegateCommand                     SelectAll { get; }
        public  DelegateCommand                     DeselectAll { get; }

        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }
        public  event Action<IDialogResult>         RequestClose;

        public BulkTrackRenameViewModel( ITrackProvider trackProvider, IEventAggregator eventAggregator ) {
            mTrackProvider = trackProvider;
            mEventAggregator = eventAggregator;

            Title = "Bulk Track Rename";

            TrackList = new BindableCollection<UiSelectableTrackNode>();

            SelectAll = new DelegateCommand( OnSelectAll );
            DeselectAll = new DelegateCommand( OnDeselectAll );

            Ok = new DelegateCommand( OnOk, CanAccept );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            var trackList = parameters.GetValue<IEnumerable<UiAlbumTrack>>( cTrackListParameter );

            TrackList.Clear();
            TrackCount = String.Empty;
            SelectedTrackCount = String.Empty;

            if( trackList != null ) {
                TrackList.AddRange( from i in trackList select new UiSelectableTrackNode( i.Track, i.Album, OnTrackSelectionChanged ));

                TrackCount = $"{TrackList.Count:N0}";
                OnSelectAll();
            }

            ReplacementName = String.Empty;
            UpdateSelectionCount();
            RaisePropertyChanged( () => TrackCount );
        }

        public string ReplacementName {
            get => mReplacementName;
            set {
                mReplacementName = value;

                UpdateTrackMatches();
                UpdateSelectionCount();
                RaisePropertyChanged( () => ReplacementName );
                Ok.RaiseCanExecuteChanged();
            }
        }

        public UiSelectableTrackNode SelectedTrack {
            get => null;
            set {
                if( String.IsNullOrWhiteSpace( ReplacementName )) {
                    ReplacementName = value.TrackName;
                }
            }
        }

        private void OnSelectAll() {
            TrackList.ForEach( t => t.Selected = true );

            UpdateSelectionCount();
        }

        private void OnDeselectAll() {
            TrackList.ForEach( t => t.Selected = false );

            UpdateSelectionCount();
        }

        private void OnTrackSelectionChanged( UiSelectableTrackNode node ) {
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount() {
            var selectedCount = TrackList.Where( t => t.Selected && t.WillSelect );

            SelectedTrackCount = $"{selectedCount.Count():N0}";
            RaisePropertyChanged( () => SelectedTrackCount );

            Ok.RaiseCanExecuteChanged();
        }

        private void UpdateTrackMatches() {
            TrackList.ForEach( t => { t.WillSelect = !t.TrackName.Equals( ReplacementName ); });
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private bool CanAccept() {
            return TrackList.Any( t => t.Selected ) && !String.IsNullOrWhiteSpace( ReplacementName );
        }

        public void OnOk() {
            if( CanAccept()) {
                UpdateTracks( from t in TrackList where t.Selected select t, ReplacementName );

                RaiseRequestClose( new DialogResult( ButtonResult.OK, new DialogParameters()));
            }
        }

        private void UpdateTracks( IEnumerable<UiSelectableTrackNode> tracks, string replacementName ) {
            tracks.ForEach( track => {
                if(!track.TrackName.Equals( replacementName )) {
                    using( var updater = mTrackProvider.GetTrackForUpdate( track.Track.DbId )) {
                        updater.Item.Name = replacementName;

                        updater.Update();
                    }

                    mEventAggregator.PublishOnUIThread( new Events.TrackUserUpdate( mTrackProvider.GetTrack( track.Track.DbId )));
                    mEventAggregator.PublishOnUIThread( new Events.LibraryBackupPressure( 1, "Track Rename (Bulk)" ));
                }
            });
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
