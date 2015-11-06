using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class TrackEditInfo : InteractionRequestData<TrackEditDialogModel> {
		public TrackEditInfo( TrackEditDialogModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumTracksViewModel : AutomaticPropertyBase,
										  IHandle<Events.DatabaseClosing>, IHandle<Events.TrackUserUpdate> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly ISelectionState				mSelectionState;
		private readonly ITrackProvider					mTrackProvider;
		private readonly Observal.Observer				mChangeObserver;
		private readonly BindableCollection<UiTrack>	mTracks;
		private readonly InteractionRequest<TrackEditInfo>	mTrackEditRequest; 
		private TaskHandler<IEnumerable<UiTrack>>		mTrackRetrievalTaskHandler;
		private CancellationTokenSource					mCancellationTokenSource;
		private long									mCurrentAlbumId;

		public AlbumTracksViewModel( IEventAggregator eventAggregator, ISelectionState selectionState, ITrackProvider trackProvider, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mTrackProvider = trackProvider;

			mEventAggregator.Subscribe( this );

			mTracks = new BindableCollection<UiTrack>();
			mCurrentAlbumId = Constants.cDatabaseNullOid;

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mTrackEditRequest = new InteractionRequest<TrackEditInfo>();

			mSelectionState.CurrentAlbumChanged.Subscribe( OnAlbumChanged );
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearTrackList();
		}

		private void OnAlbumChanged( DbAlbum album ) {
			if( album != null ) {
				UpdateTrackList( album.DbId );
			}
			else {
				ClearTrackList();
			}
		}

		private void UpdateTrackList( long albumId ) {
			if( albumId == Constants.cDatabaseNullOid ) {
				ClearTrackList();
			}
			else {
				if( mCurrentAlbumId != albumId ) {
					ClearTrackList();

					RetrieveTracks( albumId );
				}
			}
		}

		internal TaskHandler<IEnumerable<UiTrack>>  TracksRetrievalTaskHandler {
			get {
				if( mTrackRetrievalTaskHandler == null ) {
					Execute.OnUIThread( () => mTrackRetrievalTaskHandler = new TaskHandler<IEnumerable<UiTrack>> ());
				}

				return( mTrackRetrievalTaskHandler );
			}
			set{ mTrackRetrievalTaskHandler = value; }
		}

		private CancellationToken GenerateCanellationToken() {
			mCancellationTokenSource = new CancellationTokenSource();

			return( mCancellationTokenSource.Token );
		}

		private void CancelRetrievalTask() {
			if( mCancellationTokenSource != null ) {
				mCancellationTokenSource.Cancel();
				mCancellationTokenSource = null;
			}
		}

		private void ClearCurrentTask() {
			mCancellationTokenSource = null;
		}

		private void RetrieveTracks( long forAlbumId ) {
			CancelRetrievalTask();
	
			var cancellationToken = GenerateCanellationToken();

			TracksRetrievalTaskHandler.StartTask( () => LoadTracks( forAlbumId, cancellationToken ),
												albumList => UpdateUi( albumList, forAlbumId ),
												ex => mLog.LogException( string.Format( "Retrieve tracks for {0}", forAlbumId ), ex ),
												cancellationToken );
		}

		private IEnumerable<UiTrack> LoadTracks( long albumId, CancellationToken cancellationToken ) {
			var retValue = new List<UiTrack>();

			using( var tracks = mTrackProvider.GetTrackList( albumId )) {
				if(!cancellationToken.IsCancellationRequested ) {
					var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
														orderby track.VolumeName, track.TrackNumber 
														ascending select track );

					retValue.AddRange( sortedList.Select( TransformTrack ));
				}
			}

			return( retValue );
		}

		private void UpdateUi( IEnumerable<UiTrack> trackList, long albumId ) {
			ClearTrackList();

			mTracks.AddRange( trackList );
			mCurrentAlbumId = albumId;
			AlbumPlayTime = TimeSpan.FromSeconds( mTracks.Sum( track => track.Duration.TotalSeconds ));

			foreach( var track in mTracks ) {
				mChangeObserver.Add( track );
			}

			ClearCurrentTask();
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, OnTrackEdit  );

			if( dbTrack != null ) {
				Mapper.DynamicMap( dbTrack, retValue );
			}

			return( retValue );
		}

		private void ClearTrackList() {
			foreach( var track in mTracks ) {
				mChangeObserver.Release( track );
			}

			mTracks.Clear();
			mCurrentAlbumId = Constants.cDatabaseNullOid;

			AlbumPlayTime = new TimeSpan();
		}

		private void OnTrackPlay( long trackId ) {
			GlobalCommands.PlayTrack.Execute( mTrackProvider.GetTrack( trackId ));
		}

		private static void OnNodeChanged( PropertyChangeNotification propertyNotification ) {
			if( propertyNotification.Source is UiBase ) {
				var item = propertyNotification.Source as UiBase;

				if( propertyNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( item.DbId, item.UiRating ));
				}
				if( propertyNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( item.DbId, item.UiIsFavorite ));
				}
			}
		}

		public void Handle( Events.TrackUserUpdate eventArgs ) {
			var track = ( from UiTrack node in mTracks where node.DbId == eventArgs.TrackId select node ).FirstOrDefault();

			if( track != null ) {
				var newTrack = TransformTrack( mTrackProvider.GetTrack( eventArgs.TrackId ));

				mChangeObserver.Release( track );
				mTracks[mTracks.IndexOf( track )] = newTrack;
				mChangeObserver.Add( newTrack );
			}
		}

		public BindableCollection<UiTrack> TrackList {
			get{ return( mTracks ); }
		}

		public TimeSpan AlbumPlayTime {
			get{ return( Get( () => AlbumPlayTime )); }
			set{ Set( () => AlbumPlayTime, value ); }
		}

		public InteractionRequest<TrackEditInfo> TrackEditRequest {
			get{ return( mTrackEditRequest ); }
		}  

		private void OnTrackEdit( long trackId ) {
			var track = mTrackProvider.GetTrack( trackId );

			if( track != null ) {
				var dialogModel = new TrackEditDialogModel( track );
				
				mTrackEditRequest.Raise( new TrackEditInfo( dialogModel ), OnTrackEdited );
			}
		}

		private void OnTrackEdited( TrackEditInfo confirmation ) {
			if( confirmation.Confirmed ) {
				using( var updater = mTrackProvider.GetTrackForUpdate( confirmation.ViewModel.Track.DbId )) {
					if(( updater != null ) &&
					   ( updater.Item != null )) {
						updater.Item.Name = confirmation.ViewModel.Track.Name;
						updater.Item.PublishedYear = confirmation.ViewModel.Track.PublishedYear;

						updater.Update();

						if( confirmation.ViewModel.UpdateFileTags ) {
							GlobalCommands.SetMp3Tags.Execute( new SetMp3TagCommandArgs( updater.Item )
																						{ PublishedYear = updater.Item.PublishedYear,
																						  Name = updater.Item.Name });
						}
					}
				}
			}
		}
	}
}
