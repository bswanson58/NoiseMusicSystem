using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using ReusableBits;

namespace Noise.UI.ViewModels {
	public class LibraryAdditionsViewModel : ViewModelBase,
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

			mHorizonCount = 1000;
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

			return( from LibraryAdditionNode node in retValue orderby node.Artist.Name + node.Album.Name ascending select node );
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
	}
}
