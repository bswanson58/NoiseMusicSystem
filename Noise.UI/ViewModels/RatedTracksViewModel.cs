using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using ReusableBits;

namespace Noise.UI.ViewModels {
    class RatedTracksViewModel : ViewModelBase, IActiveAware, IDisposable,
                                                IHandle<Events.DatabaseClosing> {
        private readonly IEventAggregator	mEventAggregator;
        private readonly IUiLog				mLog;
        private readonly ISelectionState	mSelectionState;
        private readonly IAlbumProvider		mAlbumProvider;
        private readonly ITrackProvider		mTrackProvider;
        private readonly IPlayCommand		mPlayCommand;
        private readonly BindableCollection<UiArtistTrackNode>	mTrackList;
        private ICollectionView				mTrackView;
        private	bool						mIsActive;
        private TaskHandler<IEnumerable<UiArtistTrackNode>>	mUpdateTask;
        private CancellationTokenSource						mCancellationTokenSource;

//        public	bool									IsListFiltered => !String.IsNullOrWhiteSpace( FilterText );
        public	event EventHandler						IsActiveChanged = delegate { };

        public RatedTracksViewModel( IEventAggregator eventAggregator, ISelectionState selectionState, IPlayCommand playCommand,
                                      IAlbumProvider albumProvider, ITrackProvider trackProvider, IUiLog log ) {
            mEventAggregator = eventAggregator;
            mLog = log;
            mSelectionState = selectionState;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mPlayCommand = playCommand;

            mEventAggregator.Subscribe( this );

            mTrackList = new BindableCollection<UiArtistTrackNode>();
            TracksValid = false;

//            mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );
        }

        public bool IsActive {
            get => ( mIsActive );
            set {
                mIsActive = value;

                if( mIsActive ) {
                    if( mSelectionState.CurrentArtist != null ) {
//                        UpdateTrackList( mSelectionState.CurrentArtist );
                    }
                }
                else {
//                    CancelRetrievalTask();
                    mTrackList.Clear();
                }

                IsActiveChanged( this, new EventArgs());
            }
        }

        public bool TracksValid {
            get{ return( Get( () => TracksValid )); }
            set{ Set( () => TracksValid, value ); }
        }

        public void Handle( Events.DatabaseClosing args ) {
            mTrackList.Clear();
            TracksValid = false;
        }

        public void Dispose() {
            mCancellationTokenSource?.Dispose();
            mEventAggregator?.Unsubscribe( this );
        }
    }
}
