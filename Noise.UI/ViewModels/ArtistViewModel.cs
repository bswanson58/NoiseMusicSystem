using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Events;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistViewModel : AutomaticCommandBase,
								   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, 
								   IHandle<Events.ArtistContentUpdated>, IHandle<Events.DatabaseItemChanged> {
		private readonly IEventAggregator		mEvents;
		private readonly ICaliburnEventAggregator	mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IDiscographyProvider	mDiscographyProvider;
		private readonly ITagManager			mTagManager;
		private readonly IDialogService			mDialogService;
		private readonly Observal.Observer		mChangeObserver;
		private UiArtist						mCurrentArtist;
		private LinkNode						mArtistWebsite;
		private TaskHandler<ArtistSupportInfo>	mTaskHandler; 
		private readonly BindableCollection<LinkNode>				mSimilarArtists;
		private readonly BindableCollection<LinkNode>				mTopAlbums;
		private readonly BindableCollection<LinkNode>				mBandMembers;
		private readonly SortableCollection<DbDiscographyRelease>	mDiscography;

		public ArtistViewModel( IEventAggregator eventAggregator, ICaliburnEventAggregator caliburnEventAggregator,
								IArtistProvider artistProvider, IAlbumProvider albumProvider, IDiscographyProvider discographyProvider,
								ITagManager tagManager, IDialogService dialogService ) {
			mEvents = eventAggregator;
			mEventAggregator = caliburnEventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mDiscographyProvider = discographyProvider;
			mTagManager = tagManager;
			mDialogService = dialogService;

			mEventAggregator.Subscribe( this );

			mSimilarArtists = new BindableCollection<LinkNode>();
			mTopAlbums = new BindableCollection<LinkNode>();
			mBandMembers = new BindableCollection<LinkNode>();
			mDiscography = new SortableCollection<DbDiscographyRelease>();

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

		public ArtistSupportInfo SupportInfo {
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
						if( discoList != null ) {
							mDiscography.AddRange( discoList.List );
							mDiscography.Sort( release => release.Year, ListSortDirection.Descending );
						}
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

		private void SetCurrentArtist( long artistId ) {
			var artist = mArtistProvider.GetArtist( artistId );

			if( artist != null ) {
				CurrentArtist = TransformArtist( artist );
			}
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
					mEventAggregator.Publish( new Events.ArtistContentRequest( value.DbId ));

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

		public void Handle( Events.ArtistContentUpdated eventArgs ) {
			if(( CurrentArtist != null ) &&
			   ( CurrentArtist.DbId == eventArgs.ArtistId )) {
				RetrieveSupportInfo( CurrentArtist );
			}
		}

		internal TaskHandler<ArtistSupportInfo> TaskHandler {
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
			mEventAggregator.Publish( new Events.ArtistFocusRequested( artistId ));
		}

		private void OnTopAlbumClicked( long albumId ) {
			var album = mAlbumProvider.GetAlbum( albumId );

			if( album != null ) {
				mEventAggregator.Publish( new Events.AlbumFocusRequested( album ));
			}
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			if( CurrentArtist != null ) {
				if( request.ArtistId != CurrentArtist.DbId ) {
					SetCurrentArtist( request.ArtistId );
				}
			}
			else {
				SetCurrentArtist( request.ArtistId );
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			if( CurrentArtist != null ) {
				if( request.ArtistId != CurrentArtist.DbId ) {
					SetCurrentArtist( request.ArtistId );
				}
			}
			else {
				SetCurrentArtist( request.ArtistId );
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

		public void Handle( Events.DatabaseItemChanged eventArgs ) {
			var item = eventArgs.ItemChangedArgs.Item;

			if(( item is DbArtist ) &&
			   ( CurrentArtist != null ) &&
			   ( eventArgs.ItemChangedArgs.Change == DbItemChanged.Update ) &&
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
