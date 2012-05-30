using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistInfoViewModel : AutomaticCommandBase, IActiveAware,
									   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>,
									   IHandle<Events.ArtistContentUpdated>, IHandle<Events.ViewDisplayRequest> {
		private readonly IEventAggregator				mEventAggregator;
		private readonly IArtistProvider				mArtistProvider;
		private readonly IDiscographyProvider			mDiscographyProvider;
		private long									mCurrentArtistId;
		private TaskHandler<ArtistSupportInfo>			mTaskHandler; 
		private readonly BindableCollection<LinkNode>	mSimilarArtists;
		private readonly BindableCollection<LinkNode>	mTopAlbums;
		private readonly BindableCollection<LinkNode>	mBandMembers;
		private readonly SortableCollection<DbDiscographyRelease>	mDiscography;

		public	event	EventHandler					IsActiveChanged;

		public ArtistInfoViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, IDiscographyProvider discographyProvider ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mDiscographyProvider = discographyProvider;
			mCurrentArtistId = Constants.cDatabaseNullOid;

			mEventAggregator.Subscribe( this );

			mSimilarArtists = new BindableCollection<LinkNode>();
			mTopAlbums = new BindableCollection<LinkNode>();
			mBandMembers = new BindableCollection<LinkNode>();
			mDiscography = new SortableCollection<DbDiscographyRelease>();
		}

		public bool IsActive {
			get{ return( Get( () => IsActive )); }
			set{ Set( () => IsActive, value ); }
		}

		private void ClearCurrentArtist() {
			mSimilarArtists.Clear();
			mTopAlbums.Clear();
			mBandMembers.Clear();
			mDiscography.Clear();

			RaisePropertyChanged( () => SupportInfo );
		}

		public ArtistSupportInfo SupportInfo {
			get { return( Get( () => SupportInfo )); }
			set {
				ClearCurrentArtist();

				if( value != null ) {
					if(( value.SimilarArtist != null ) &&
						( value.SimilarArtist.Items.Any())) {
						mSimilarArtists.AddRange( from DbAssociatedItem artist in value.SimilarArtist.Items
													select artist.IsLinked ? new LinkNode( artist.Item, artist.AssociatedId, OnSimilarArtistClicked ) :
																				new LinkNode( artist.Item ));
					}

					if(( value.TopAlbums != null ) &&
					   ( value.TopAlbums.Items.Any())) {
						mTopAlbums.AddRange( from DbAssociatedItem album in value.TopAlbums.Items 
												select album.IsLinked ? new LinkNode( album.Item, album.AssociatedId, OnTopAlbumClicked ) :
																		new LinkNode( album.Item ));
					}

					if(( value.BandMembers != null ) &&
					   ( value.BandMembers.Items.Any())) {
						mBandMembers.AddRange( from DbAssociatedItem member in value.BandMembers.Items select new LinkNode( member.Item ));
					}

					using( var discoList = mDiscographyProvider.GetDiscography( mCurrentArtistId )) {
						if( discoList != null ) {
							mDiscography.AddRange( discoList.List );
							mDiscography.Sort( release => release.Year, ListSortDirection.Descending );
						}
					}
				}

				Set( () => SupportInfo, value  );
			}
		}

		private void SetCurrentArtist( long artistId ) {
			if( mCurrentArtistId != artistId ) {
				ClearCurrentArtist();

				mCurrentArtistId = artistId;
				RetrieveSupportInfo( mCurrentArtistId );
			}
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			SetCurrentArtist( request.ArtistId );

			if(!IsActive ) {
				mEventAggregator.Publish( new Events.ViewDisplayRequest( ViewNames.ArtistInfoView ));
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			SetCurrentArtist( request.ArtistId );
		}

		public void Handle( Events.ArtistContentUpdated eventArgs ) {
			if( mCurrentArtistId == eventArgs.ArtistId ) {
				RetrieveSupportInfo( mCurrentArtistId );
			}
		}

		internal TaskHandler<ArtistSupportInfo> TaskHandler {
			get {
				if( mTaskHandler == null ) {
					Execute.OnUIThread( () => mTaskHandler = new TaskHandler<ArtistSupportInfo>());
				}

				return( mTaskHandler );
			}

			set { mTaskHandler = value; }
		} 

		private void RetrieveSupportInfo( long artistId ) {
			TaskHandler.StartTask( () => mArtistProvider.GetArtistSupportInfo( artistId ), 
									result => SupportInfo = result,
									exception => NoiseLogger.Current.LogException( "ArtistInfoViewModel:RetrieveSupportInfo", exception ));
		}

		private void OnSimilarArtistClicked( long artistId ) {
			mEventAggregator.Publish( new Events.ArtistFocusRequested( artistId ));
		}

		private void OnTopAlbumClicked( long albumId ) {
			mEventAggregator.Publish( new Events.AlbumFocusRequested( mCurrentArtistId, albumId ));
		}

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

		[DependsUpon( "SupportInfo" )]
		public bool ArtistValid {
			get{ return( SupportInfo != null ); }
		}

		[DependsUpon( "SupportInfo" )]
		public string ArtistBio {
			get {
				var retValue = "";

				if(( SupportInfo != null ) &&
				   ( SupportInfo.Biography != null )) {
					retValue = SupportInfo.Biography.Text;
				}

				return( retValue );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public IEnumerable<LinkNode> TopAlbums {
			get{ return( mTopAlbums ); }
		}

		[DependsUpon( "SupportInfo" )]
		public IEnumerable<LinkNode> SimilarArtist {
			get { return( mSimilarArtists ); }
		}

		[DependsUpon( "SupportInfo" )]
		public IEnumerable<LinkNode> BandMembers {
			get { return( mBandMembers ); }
		}

		[DependsUpon( "SupportInfo" )]
		public IEnumerable<DbDiscographyRelease> Discography {
			get{ return( mDiscography ); }
		}
	}
}
