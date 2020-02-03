using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	internal class ArtistInfoViewModel : AutomaticCommandBase, IActiveAware, IHandle<Events.ViewDisplayRequest>, IHandle<Events.DatabaseClosing>,
										 IHandle<Events.ArtistMetadataUpdated> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IUiLog							mLog;
		private readonly ISelectionState				mSelectionState;
		private readonly IArtistProvider				mArtistProvider;
		private readonly ITrackProvider					mTrackProvider;
		private readonly IMetadataManager				mMetadataManager;
		private readonly List<DbArtist>					mArtistList;
		private long									mCurrentArtistId;
		private string									mCurrentArtistName;
		private TaskHandler								mTaskHandler; 
		private CancellationTokenSource					mCancellationTokenSource;
		private readonly Random							mRandom;
		private readonly BindableCollection<LinkNode>	mSimilarArtists;
		private readonly BindableCollection<LinkNode>	mTopAlbums;
		private readonly BindableCollection<LinkNode>	mTopTracks; 
		private readonly BindableCollection<string>		mBandMembers;
		private readonly BindableCollection<DbDiscographyRelease>	mDiscography;

		private IDisposable								mSelectionStateSubscription;
		private bool									mIsActive;
		public	event	EventHandler					IsActiveChanged = delegate { };

		public ArtistInfoViewModel( IEventAggregator eventAggregator, ISelectionState selectionState, IMetadataManager metadataManager,
									IArtistProvider artistProvider, ITrackProvider trackProvider, IUiLog log ) {
			mEventAggregator = eventAggregator;
			mLog = log;
			mSelectionState = selectionState;
			mArtistProvider = artistProvider;
			mTrackProvider = trackProvider;
			mMetadataManager = metadataManager;
			mCurrentArtistId = Constants.cDatabaseNullOid;
			mCurrentArtistName = string.Empty;
			mArtistList = new List<DbArtist>();
			mRandom = new Random( DateTime.Now.Millisecond );

			mEventAggregator.Subscribe( this );

			mSimilarArtists = new BindableCollection<LinkNode>();
			mTopAlbums = new BindableCollection<LinkNode>();
			mTopTracks = new BindableCollection<LinkNode>();
			mBandMembers = new BindableCollection<string>();
			mDiscography = new SortableCollection<DbDiscographyRelease>();
		}

		public bool IsActive {
			get{ return( mIsActive ); }
			set {
				if( mIsActive ) {
					if( mSelectionStateSubscription != null ) {
						mSelectionStateSubscription.Dispose();
						mSelectionStateSubscription = null;
					}
				}
				else {
					if( mSelectionStateSubscription == null ) {
						mSelectionStateSubscription = mSelectionState.CurrentArtistChanged.Subscribe( OnArtistChanged );

						OnArtistChanged( mSelectionState.CurrentArtist );
					}
				}

				mIsActive = value;
				IsActiveChanged( this, new EventArgs());
			}
		}

		private void ClearCurrentArtist() {
			mSimilarArtists.Clear();
			mTopAlbums.Clear();
			mBandMembers.Clear();
			mDiscography.Clear();
			ArtistBiography = string.Empty;
			mCurrentArtistId = Constants.cDatabaseNullOid;
			mCurrentArtistName = string.Empty;

			ArtistValid = false;
		}

		private void SetCurrentArtist( long artistId ) {
			if( mCurrentArtistId != artistId ) {
				ClearCurrentArtist();

				mCurrentArtistId = artistId;

				var	artist = mArtistProvider.GetArtist( artistId );
				if( artist != null ) {
					mCurrentArtistName = artist.Name;

					RetrieveArtistMetadata( mCurrentArtistName );
				}
			}
		}

		private void OnArtistChanged( DbArtist artist ) {
			if( artist != null ) {
				SetCurrentArtist( artist.DbId );
			}
			else {
				ClearCurrentArtist();
			}
		}

		public void Handle( Events.DatabaseClosing args ) {
			ClearCurrentArtist();
			mArtistList.Clear();
		}

		public void Handle( Events.ArtistMetadataUpdated eventArgs ) {
			if( string.Equals( mCurrentArtistName, eventArgs.ArtistName )) {
				RetrieveArtistMetadata( mCurrentArtistName );
			}
		}

		internal TaskHandler TaskHandler {
			get {
				if( mTaskHandler == null ) {
					Execute.OnUIThread( () => mTaskHandler = new TaskHandler());
				}

				return( mTaskHandler );
			}

			set { mTaskHandler = value; }
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

		private void RetrieveArtistMetadata( string artistName ) {
			CancelRetrievalTask();

			var cancellationToken = GenerateCanellationToken();

			TaskHandler.StartTask( () => {
									if(!mArtistList.Any()) {
										using( var artistList = mArtistProvider.GetArtistList()) {
											mArtistList.AddRange( artistList.List );
										}
									}

									if( !cancellationToken.IsCancellationRequested ) {
										var info = mMetadataManager.GetArtistMetadata( artistName );

										ArtistBiography = info.GetMetadata( eMetadataType.Biography );

										if( !cancellationToken.IsCancellationRequested ) {
											mBandMembers.Clear();
											// current members
											mBandMembers.AddRange( from m in info.GetMetadataArray( eMetadataType.BandMembers ) where !m.StartsWith( "-" ) && !m.StartsWith( "(" ) orderby m select m );
											// other groups
											mBandMembers.AddRange( from m in info.GetMetadataArray( eMetadataType.BandMembers ) where m.StartsWith( "(" ) orderby m select m );
											// past members
											mBandMembers.AddRange( from m in info.GetMetadataArray( eMetadataType.BandMembers ) where m.StartsWith( "-" ) orderby m select m );
										}

										if( !cancellationToken.IsCancellationRequested ) {
											mTopAlbums.Clear();
											mTopAlbums.AddRange( info.GetMetadataArray( eMetadataType.TopAlbums ).Select( item => new LinkNode( item )));
										}

										if( !cancellationToken.IsCancellationRequested ) {
											var discography = mMetadataManager.GetArtistDiscography( artistName );
									
											mDiscography.Clear();
											mDiscography.AddRange( from d in discography.Discography orderby d.Year descending select  d );
										}

										if( !cancellationToken.IsCancellationRequested ) {
											mSimilarArtists.Clear();
											mSimilarArtists.AddRange( LinkSimiliarArtists( info.GetMetadataArray( eMetadataType.SimilarArtists ), cancellationToken ));
										}

										if( !cancellationToken.IsCancellationRequested ) {
											mTopTracks.Clear();
											mTopTracks.AddRange( LinkTopTracks( info.GetMetadataArray( eMetadataType.TopTracks ), cancellationToken ));
										}
									}

									ClearCurrentTask();
								},
								() => ArtistValid = true,
								exception => mLog.LogException( string.Format( "RetrieveSupportInfo for \"{0}\"", artistName ), exception ),
								cancellationToken );
		}

		private IEnumerable<LinkNode> LinkSimiliarArtists( IEnumerable<string> similarArtistList, CancellationToken cancellationToken ) {
			return( from similarArtist in similarArtistList.TakeWhile( similarArtist => !cancellationToken.IsCancellationRequested )
					let matchingArtist = ( from artist in mArtistList where artist.Name == similarArtist select artist ).FirstOrDefault()
					select matchingArtist != null ? new LinkNode( similarArtist, matchingArtist.DbId, OnSimilarArtistClicked ) :
													new LinkNode( similarArtist ));
		} 

		private IEnumerable<LinkNode> LinkTopTracks( IEnumerable<string> topTracks, CancellationToken cancellationToken ) {
			var retValue = new List<LinkNode>();
			var artist = mArtistList.FirstOrDefault( a => a.DbId == mCurrentArtistId );

			if( artist != null ) {
				var allTracks = mTrackProvider.GetTrackList( artist );

				foreach( var trackName in topTracks ) {
					if( cancellationToken.IsCancellationRequested ) {
						break;
					}

					string	name = trackName;
					var		trackList = allTracks.List.Where( t => t.Name.Equals( name, StringComparison.CurrentCultureIgnoreCase )).ToList();

					if( trackList.Any()) {
						var selectedTrack = trackList.Skip( NextRandom( trackList.Count )).Take( 1 ).FirstOrDefault();

						if( selectedTrack != null ) {
							retValue.Add( new LinkNode( trackName, selectedTrack.Album, OnTopTrackClicked ));
						}
					}
					else {
						retValue.Add( new LinkNode( trackName ));
					}
				}
			}

			return( retValue );
		} 

		private int NextRandom( int maxValue ) {
			return( mRandom.Next( maxValue ));
		}

		private void OnSimilarArtistClicked( long artistId ) {
			mEventAggregator.PublishOnUIThread( new Events.ArtistFocusRequested( artistId ));
		}

		private void OnTopTrackClicked( long albumId ) {
			mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mCurrentArtistId, albumId ));
		}

//		private void OnTopAlbumClicked( long albumId ) {
//			mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mCurrentArtistId, albumId ));
//		}

		public void Handle( Events.ViewDisplayRequest eventArgs ) {
			if( ViewNames.ArtistInfoView.Equals( eventArgs.ViewName )) {
				IsDisplayed = !IsDisplayed;

				eventArgs.ViewWasOpened = IsDisplayed;
			}
		}

		public bool IsDisplayed {
			get{ return( Get( () => IsDisplayed )); }
			set{ Set( () => IsDisplayed, value ); }
		}

		public bool ArtistValid {
			get{ return( Get( () => ArtistValid )); }
			set{ Set( () => ArtistValid, value ); }
		}

		public string ArtistBiography {
			get { return( Get( () => ArtistBiography )); }
			set { Set( () => ArtistBiography, value ); }
		}

		public IEnumerable<LinkNode> TopAlbums {
			get{ return( mTopAlbums ); }
		}

		public IEnumerable<LinkNode> TopTracks {
			get { return( mTopTracks ); }
		} 

		public IEnumerable<LinkNode> SimilarArtist {
			get { return( mSimilarArtists ); }
		}

		public IEnumerable<string> BandMembers {
			get { return( mBandMembers ); }
		}

		public IEnumerable<DbDiscographyRelease> Discography {
			get{ return( mDiscography ); }
		}
	}
}
