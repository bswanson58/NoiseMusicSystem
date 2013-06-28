using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class LibraryAdditionsViewModel : AutomaticCommandBase,
											 IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>,
											 IHandle<Events.LibraryUpdateCompleted> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;
		private readonly DateTime				mHorizonTime;
		private readonly UInt32					mHorizonCount;
		private readonly BindableCollection<LibraryAdditionNode>	mNodeList;
		private TaskHandler<IEnumerable<LibraryAdditionNode>>		mTaskHandler;

		public LibraryAdditionsViewModel( IEventAggregator eventAggregator, IDatabaseInfo databaseInfo,
										  IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;

			mNodeList = new BindableCollection<LibraryAdditionNode>();

			mHorizonCount = 200;
			mHorizonTime = DateTime.Now - new TimeSpan( 90, 0, 0, 0 );

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );
			if( configuration != null ) {
				mHorizonCount = configuration.NewAdditionsHorizonCount;
				mHorizonTime = DateTime.Now - new TimeSpan( configuration.NewAdditionsHorizonDays, 0, 0, 0 );
			}

			if( databaseInfo.IsOpen ) {
				RetrieveWhatsNew();
			}

			mEventAggregator.Subscribe( this );
		}

		internal TaskHandler<IEnumerable<LibraryAdditionNode>>  TracksRetrievalTaskHandler {
			get {
				if( mTaskHandler == null ) {
					Execute.OnUIThread( () => mTaskHandler = new TaskHandler<IEnumerable<LibraryAdditionNode>>());
				}

				return( mTaskHandler );
			}
			set{ mTaskHandler = value; }
		}

		private void RetrieveWhatsNew() {
			TracksRetrievalTaskHandler.StartTask( RetrieveAdditions, UpdateList,
												  ex => NoiseLogger.Current.LogException( "LibraryAdditionsViewModel:RetrieveWhatsNew", ex ));
		}

		public void Handle( Events.DatabaseOpened args ) {
			RetrieveWhatsNew();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mNodeList.Clear();
		}

		public void Handle( Events.LibraryUpdateCompleted eventArgs ) {
			RetrieveWhatsNew();

			if( eventArgs.Summary.TracksAdded > 0 ) {
				DisplayMarker = true;
			}
		}

		private void UpdateList( IEnumerable<LibraryAdditionNode> list ) {
			mNodeList.Clear();
			mNodeList.AddRange( list );
		}

		private IEnumerable<LibraryAdditionNode> RetrieveAdditions() {
			var	retValue = new List<LibraryAdditionNode>();
			var	trackList = new List<DbTrack>();

			using( var additions = mTrackProvider.GetNewlyAddedTracks()) {
				if(( additions != null ) &&
				   ( additions.List != null )) {
					UInt32	count = 0;

					foreach( var track in additions.List ) {
						if(( count < mHorizonCount ) &&
						   ( track.DateAdded > mHorizonTime )) {
							trackList.Add( track );
						}
						else {
							break;
						}

						count++;
					}
				}
			}

			if( trackList.Count > 0 ) {
				
				foreach( var track in trackList ) {
					var album = mAlbumProvider.GetAlbumForTrack( track );
					if( album != null ) {
						var artist = mArtistProvider.GetArtistForAlbum( album );

						if( artist != null ) {
							var treeNode = retValue.Find( node => node.Artist.DbId == artist.DbId && node.Album.DbId == album.DbId );

							if( treeNode != null ) {
								treeNode.AddTrack( track );
							}

							else {
								retValue.Add( new LibraryAdditionNode( artist, album, track, OnNodeSelected, OnAlbumPlayRequested, OnTrackPlayRequested ));
							}
						}
					}
				}
			}

			if( retValue.Any()) {
				var maximumDate = DateTime.Now.Date.Ticks;
				// Make sure the minimum is at least 7 days old.
				var minimumDate = Math.Min( retValue.Min( node => node.Album.DateAddedTicks ), ( DateTime.Now - new TimeSpan( 7, 0, 0, 0 )).Ticks );

				foreach( var node in retValue ) {
					node.RelativeAge = (double)( node.Album.DateAddedTicks - minimumDate ) / ( maximumDate - minimumDate );
				}
			}

			return( retValue );
		}

		public BindableCollection<LibraryAdditionNode> NodeList {
			get{ return( mNodeList ); }
		}

		private void OnNodeSelected( LibraryAdditionNode node ) {
			if( node.Artist != null ) {
				mEventAggregator.Publish( new Events.ArtistFocusRequested( node.Artist.DbId ));
			}
			if( node.Album != null ) {
				mEventAggregator.Publish( new Events.AlbumFocusRequested( node.Album ));
			}
		}

		private static void OnAlbumPlayRequested( LibraryAdditionNode node ) {
			GlobalCommands.PlayAlbum.Execute( node.Album );
		}

		private void OnTrackPlayRequested( long trackId ) {
			GlobalCommands.PlayTrack.Execute( mTrackProvider.GetTrack( trackId ));
		}

		public bool DisplayMarker {
			get{ return( Get( () => DisplayMarker )); }
			set{ Set( () => DisplayMarker, value ); }
		}

		public void Execute_ViewDisplayed( bool state ) {
			DisplayMarker = false;
		}
	}
}
