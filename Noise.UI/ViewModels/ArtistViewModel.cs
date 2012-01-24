using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;

namespace Noise.UI.ViewModels {
	public class ArtistViewModel : ViewModelBase {
		private readonly IEventAggregator		mEvents;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IDiscographyProvider	mDiscographyProvider;
		private readonly ITagManager			mTagManager;
		private readonly IDialogService			mDialogService;
		private readonly Observal.Observer		mChangeObserver;
		private UiArtist						mCurrentArtist;
		private LinkNode						mArtistWebsite;
		private TaskHandler<ArtistSupportInfo>	mTaskHandler; 
		private readonly ObservableCollectionEx<LinkNode>				mSimilarArtists;
		private readonly ObservableCollectionEx<LinkNode>				mTopAlbums;
		private readonly ObservableCollectionEx<LinkNode>				mBandMembers;
		private readonly ObservableCollectionEx<DbDiscographyRelease>	mDiscography;

		public ArtistViewModel( IEventAggregator eventAggregator,
								IArtistProvider artistProvider, IAlbumProvider albumProvider, IDiscographyProvider discographyProvider,
								ITagManager tagManager, IDialogService dialogService ) {
			mEvents = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mDiscographyProvider = discographyProvider;
			mTagManager = tagManager;
			mDialogService = dialogService;

			mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
			mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
			mEvents.GetEvent<Events.ArtistContentUpdated>().Subscribe( OnArtistInfoUpdate );
			mEvents.GetEvent<Events.DatabaseItemChanged>().Subscribe( OnDatabaseItemChanged );

			mSimilarArtists = new ObservableCollectionEx<LinkNode>();
			mTopAlbums = new ObservableCollectionEx<LinkNode>();
			mBandMembers = new ObservableCollectionEx<LinkNode>();
			mDiscography = new ObservableCollectionEx<DbDiscographyRelease>();

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );
		}

		public UiArtist Artist {
			get{ return( mCurrentArtist ); }
		}

		[DependsUpon( "Artist" )]
		public bool ArtistValid {
			get{ return( Artist != null ); }
		}

		private ArtistSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set {
				mSimilarArtists.Clear();
				mTopAlbums.Clear();
				mBandMembers.Clear();
				mDiscography.Clear();

				if(( CurrentArtist != null ) &&
				   ( value != null )) {
					if(( value.SimilarArtist != null ) &&
						( value.SimilarArtist.Items.GetLength( 0 ) > 0 )) {
						mSimilarArtists.AddRange( from DbAssociatedItem artist in value.SimilarArtist.Items
													select artist.IsLinked ? new LinkNode( artist.Item, artist.AssociatedId, OnSimilarArtistClicked ) :
																				new LinkNode( artist.Item ));
					}

					if(( value.TopAlbums != null ) &&
					   ( value.TopAlbums.Items.GetLength( 0 ) > 0 )) {
						mTopAlbums.AddRange( from DbAssociatedItem album in value.TopAlbums.Items 
												select album.IsLinked ? new LinkNode( album.Item, album.AssociatedId, OnTopAlbumClicked ) :
																		new LinkNode( album.Item ));
					}

					if(( value.BandMembers != null ) &&
					   ( value.BandMembers.Items.GetLength( 0 ) > 0 )) {
						mBandMembers.AddRange( from DbAssociatedItem member in value.BandMembers.Items select new LinkNode( member.Item ));
					}

					using( var discoList = mDiscographyProvider.GetDiscography( CurrentArtist.DbId )) {
						mDiscography.SuspendNotification();

						if( discoList != null ) {
							mDiscography.AddRange( discoList.List );
							mDiscography.Sort( release => release.Year, ListSortDirection.Descending );
						}

						mDiscography.ResumeNotification();
					}
				}

				Set( () => SupportInfo, value  );
			}
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist();

			Mapper.DynamicMap( dbArtist, retValue );
			retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );

			return( retValue );
		}

		private UiArtist CurrentArtist {
			get{ return( mCurrentArtist ); }
			set {
				if( mCurrentArtist != null ) {
					mChangeObserver.Release( mCurrentArtist );

					if(( value != null ) &&
					   ( mCurrentArtist.DbId != value.DbId )) {
						SupportInfo = null;
					}
				}

				if( value != null ) {
					mEvents.GetEvent<Events.ArtistContentRequested>().Publish( mArtistProvider.GetArtist( value.DbId ));

					mCurrentArtist = value;
					mChangeObserver.Add( mCurrentArtist );
					RaisePropertyChanged( () => Artist );

					mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
					RaisePropertyChanged( () => ArtistWebsite );

					RetrieveSupportInfo( mCurrentArtist );
				}
				else {
					mCurrentArtist = null;
				}
			}
		}

		private void OnArtistInfoUpdate( DbArtist artist ) {
			if(( artist != null ) &&
			   ( CurrentArtist != null ) &&
			   ( CurrentArtist.DbId == artist.DbId )) {
				RetrieveSupportInfo( CurrentArtist );
			}
		}

		public TaskHandler<ArtistSupportInfo> TaskHandler {
			get {
				if( mTaskHandler == null ) {
					mTaskHandler = new TaskHandler<ArtistSupportInfo>();
				}

				return( mTaskHandler );
			}

			set { mTaskHandler = value; }
		} 

		private void RetrieveSupportInfo( UiArtist forArtist ) {
			TaskHandler.StartTask( () => mArtistProvider.GetArtistSupportInfo( forArtist.DbId ), 
									result => SupportInfo = result,
									exception => NoiseLogger.Current.LogException( "ArtistViewModel:RetrieveSupportInfo", exception ));
		}

		private void OnSimilarArtistClicked( long artistId ) {
			var artist = mArtistProvider.GetArtist( artistId  );

			if( artist != null ) {
				mEvents.GetEvent<Events.ArtistFocusRequested>().Publish( artist );
			}
		}

		private void OnTopAlbumClicked( long albumId ) {
			var album = mAlbumProvider.GetAlbum( albumId );

			if( album != null ) {
				mEvents.GetEvent<Events.AlbumFocusRequested>().Publish( album );
			}
		}

		private void OnArtistFocus( DbArtist artist ) {
			if( artist != null ) {
				if( CurrentArtist != null ) {
					if( artist.DbId != CurrentArtist.DbId ) {
						CurrentArtist = TransformArtist( artist );
					}
				}
				else {
					CurrentArtist = TransformArtist( artist );
				}
			}
		}

		private void OnAlbumFocus( DbAlbum album ) {
			if( CurrentArtist != null ) {
				if( album.Artist != CurrentArtist.DbId ) {
					CurrentArtist = TransformArtist( mArtistProvider.GetArtistForAlbum( album ));
				}
			}
			else {
				CurrentArtist = TransformArtist( mArtistProvider.GetArtistForAlbum( album ));
			}
		}

		private static void OnArtistChanged( PropertyChangeNotification changeNotification ) {
			var notifier = changeNotification.Source as UiArtist;

			if( notifier != null ) {
				if( changeNotification.PropertyName == "UiRating" ) {
					GlobalCommands.SetRating.Execute( new SetRatingCommandArgs( notifier.DbId, notifier.UiRating ));
				}
				if( changeNotification.PropertyName == "UiIsFavorite" ) {
					GlobalCommands.SetFavorite.Execute( new SetFavoriteCommandArgs( notifier.DbId, notifier.UiIsFavorite ));
				}
			}
		}

		private void OnDatabaseItemChanged( DbItemChangedArgs args ) {
			var item = args.Item;

			if(( item is DbArtist ) &&
			   ( CurrentArtist != null ) &&
			   ( args.Change == DbItemChanged.Update ) &&
			   ( item.DbId == CurrentArtist.DbId )) {
				CurrentArtist = TransformArtist( item as DbArtist );
			}
		}

		private void OnWebsiteRequested( long id ) {
			if(( CurrentArtist != null ) &&
			   (!string.IsNullOrWhiteSpace( CurrentArtist.Website ))) {
				mEvents.GetEvent<Events.WebsiteRequest>().Publish( CurrentArtist.Website );
			}
		}

		[DependsUpon( "SupportInfo" )]
		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				if(( SupportInfo != null ) &&
				   ( SupportInfo.ArtistImage != null )) {
					retValue = SupportInfo.ArtistImage.Image;
				}

				return( retValue );
			}
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

		public LinkNode ArtistWebsite {
			get{ return( mArtistWebsite ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<LinkNode> TopAlbums {
			get{ return( mTopAlbums ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<LinkNode> SimilarArtist {
			get { return( mSimilarArtists ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<LinkNode> BandMembers {
			get { return( mBandMembers ); }
		}

		[DependsUpon( "SupportInfo" )]
		public ObservableCollectionEx<DbDiscographyRelease> Discography {
			get{ return( mDiscography ); }
		}

		public void Execute_EditArtist() {
			if( mCurrentArtist != null ) {
				using( var artistUpdate = mArtistProvider.GetArtistForUpdate( mCurrentArtist.DbId )) {
					if( artistUpdate != null ) {
						artistUpdate.Item.Website = artistUpdate.Item.Website.Replace( Environment.NewLine, "" ).Replace( "\n", "" ).Replace( "\r", "" );

						if( mDialogService.ShowDialog( DialogNames.ArtistEdit, artistUpdate.Item ) == true ) {
							artistUpdate.Update();

							Mapper.DynamicMap( artistUpdate.Item, mCurrentArtist );

							mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
							RaisePropertyChanged( () => ArtistWebsite );
						}
					}
				}
			}
		}
	}
}
