using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class AlbumTracksViewModel : AutomaticPropertyBase,
											IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, IHandle<Events.DatabaseItemChanged> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ITrackProvider			mTrackProvider;
		private readonly IDialogService			mDialogService;
		private long							mCurrentArtistId;
		private long							mCurrentAlbumId;
		private TaskHandler						mTrackRetrievalTaskHandler;
		private readonly Observal.Observer		mChangeObserver;
		private readonly BindableCollection<UiTrack>	mTracks;

		public AlbumTracksViewModel( IEventAggregator eventAggregator, ITrackProvider trackProvider, IDialogService dialogService ) {
			mEventAggregator = eventAggregator;
			mTrackProvider = trackProvider;
			mDialogService = dialogService;

			mEventAggregator.Subscribe( this );

			mTracks = new BindableCollection<UiTrack>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnNodeChanged );
		}

		private UiTrack TransformTrack( DbTrack dbTrack ) {
			var retValue = new UiTrack( OnTrackPlay, OnTrackEdit  );

			Mapper.DynamicMap( dbTrack, retValue );

			return( retValue );
		}

		private void ClearTrackList() {
			mTracks.Each( node => mChangeObserver.Release( node ));
			mTracks.Clear();
			AlbumPlayTime = new TimeSpan();
		}

		private void SetTrackList( IEnumerable<DbTrack> trackList ) {
			Execute.OnUIThread( () => {
				ClearTrackList();

				foreach( var dbTrack in trackList ) {
					mTracks.Add( TransformTrack( dbTrack ));

					AlbumPlayTime += dbTrack.Duration;
				}

				mTracks.Each( track => mChangeObserver.Add( track ));
			});
		} 

		public void Handle( Events.ArtistFocusRequested request ) {
			if( mCurrentArtistId != request.ArtistId ) {
				mCurrentArtistId = request.ArtistId;

				ClearTrackList();
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			UpdateAlbum( request.ArtistId, request.AlbumId );

			mEventAggregator.Publish( new Events.ViewDisplayRequest( ViewNames.AlbumInfoView ));
		}

		private void UpdateAlbum( long artistId, long albumId ) {
			if( albumId == Constants.cDatabaseNullOid ) {
				ClearTrackList();
			}
			else {
				if( mCurrentAlbumId != albumId ) {
					mCurrentArtistId = artistId;
					mCurrentAlbumId = albumId;
					ClearTrackList();

					RetrieveAlbum( mCurrentAlbumId );
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

		private void RetrieveAlbum( long albumId ) {
			TracksRetrievalTaskHandler.StartTask( () => {
														using( var tracks = mTrackProvider.GetTrackList( albumId )) {
															var sortedList = new List<DbTrack>( from DbTrack track in tracks.List
																								orderby track.VolumeName, track.TrackNumber 
																								ascending select track );
															SetTrackList( sortedList );
														}
			                                      },
												  () => {},
												  ex => NoiseLogger.Current.LogException( "", ex ));
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

		public void Handle( Events.DatabaseItemChanged eventArgs ) {
			var item = eventArgs.ItemChangedArgs.Item;

			if(( item is DbTrack ) &&
			   ( eventArgs.ItemChangedArgs.Change == DbItemChanged.Update ) &&
			   ((item as DbTrack).Album == mCurrentAlbumId )) {
				Execute.OnUIThread( () => {
					var track = ( from UiTrack node in mTracks where node.DbId == item.DbId select node ).FirstOrDefault();

					if( track != null ) {
						switch( eventArgs.ItemChangedArgs.Change ) {
							case DbItemChanged.Update:
								var newTrack = TransformTrack( item as DbTrack );

								mChangeObserver.Release( track );
								mTracks[mTracks.IndexOf( track )] = newTrack;
								mChangeObserver.Add( newTrack );

								break;
							case DbItemChanged.Delete:
								mTracks.Remove( track );

								break;
						}
					}
				});
			}
		}

		public BindableCollection<UiTrack> TrackList {
			get{ return( mTracks ); }
		}

		public TimeSpan AlbumPlayTime {
			get{ return( Get( () => AlbumPlayTime )); }
			set{ Set( () => AlbumPlayTime, value ); }
		}

		private void OnTrackEdit( long trackId ) {
			using( var trackUpdate = mTrackProvider.GetTrackForUpdate( trackId )) {
				if( trackUpdate != null ) {
					var dialogModel = new TrackEditDialogModel();

					if( mDialogService.ShowDialog( DialogNames.TrackEdit, trackUpdate.Item, dialogModel ) == true ) {
						trackUpdate.Update();

						if( dialogModel.UpdateFileTags ) {
							GlobalCommands.SetMp3Tags.Execute( new SetMp3TagCommandArgs( trackUpdate.Item )
																						{ PublishedYear = trackUpdate.Item.PublishedYear,
																						  Name = trackUpdate.Item.Name });
						}
					}
				}
			}
		}
	}
}
