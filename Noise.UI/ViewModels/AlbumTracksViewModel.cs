﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Observal.Extensions;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class TrackEditInfo : InteractionRequestData<TrackEditDialogModel> {
		public TrackEditInfo( TrackEditDialogModel viewModel ) : base( viewModel ) { }
	}

	internal class AlbumTracksViewModel : AutomaticPropertyBase,
										  IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.TrackUserUpdate> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ITrackProvider			mTrackProvider;
		private long							mCurrentArtistId;
		private long							mCurrentAlbumId;
		private TaskHandler						mTrackRetrievalTaskHandler;
		private readonly Observal.Observer		mChangeObserver;
		private readonly BindableCollection<UiTrack>	mTracks;
		private readonly InteractionRequest<TrackEditInfo>	mTrackEditRequest; 

		public AlbumTracksViewModel( IEventAggregator eventAggregator, ITrackProvider trackProvider ) {
			mEventAggregator = eventAggregator;
			mTrackProvider = trackProvider;

			mEventAggregator.Subscribe( this );

			mTracks = new BindableCollection<UiTrack>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );

			mTrackEditRequest = new InteractionRequest<TrackEditInfo>();
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, OnTrackEdit  );

			if( dbTrack != null ) {
				Mapper.DynamicMap( dbTrack, retValue );
			}

			return( retValue );
		}

		private void ClearTrackList() {
			mTracks.Each( node => mChangeObserver.Release( node ));
			mTracks.Clear();

			AlbumPlayTime = new TimeSpan();
		}

		private void SetTrackList( IEnumerable<DbTrack> trackList ) {
			ClearTrackList();

			foreach( var dbTrack in trackList ) {
				mTracks.Add( TransformTrack( dbTrack ));

				AlbumPlayTime += dbTrack.Duration;
			}

			mTracks.Each( track => mChangeObserver.Add( track ));
		} 

		public void Handle( Events.ArtistFocusRequested request ) {
			if( mCurrentArtistId != request.ArtistId ) {
				mCurrentArtistId = request.ArtistId;

				ClearTrackList();
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			UpdateTrackList( request.ArtistId, request.AlbumId );

			mEventAggregator.Publish( new Events.ViewDisplayRequest( ViewNames.AlbumInfoView ));
		}

		private void UpdateTrackList( long artistId, long albumId ) {
			if( albumId == Constants.cDatabaseNullOid ) {
				ClearTrackList();
			}
			else {
				if( mCurrentAlbumId != albumId ) {
					mCurrentArtistId = artistId;
					mCurrentAlbumId = albumId;
					ClearTrackList();

					RetrieveTracks( mCurrentAlbumId );
				}
			}
		}

		internal TaskHandler TracksRetrievalTaskHandler {
			get {
				if( mTrackRetrievalTaskHandler == null ) {
					mTrackRetrievalTaskHandler = new TaskHandler();
				}

				return( mTrackRetrievalTaskHandler );
			}
			set{ mTrackRetrievalTaskHandler = value; }
		}

		private void RetrieveTracks( long forAlbumId ) {
			TracksRetrievalTaskHandler.StartTask( () => {
														using( var tracks = mTrackProvider.GetTrackList( forAlbumId )) {
															var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
																								orderby track.VolumeName, track.TrackNumber 
																								ascending select track );
															SetTrackList( sortedList );
														}
			                                      },
												  () => {},
												  ex => NoiseLogger.Current.LogException( "AlbumTracksViewModel:RetrieveTracks", ex ));
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
