using AutoMapper;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;
using Noise.UI.Dto;
using Noise.UI.Support;
using Observal.Extensions;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class ArtistEditRequest : Confirmation {
		private readonly UiArtist	mArtist;

		public ArtistEditRequest(UiArtist artist ) {
			mArtist = artist;
			Content = artist;
		} 

		public UiArtist Artist {
			get{ return( mArtist ); }
		}
	}

	public class ArtistViewModel : AutomaticCommandBase,
								   IHandle<Events.ArtistFocusRequested>, IHandle<Events.AlbumFocusRequested>, 
								   IHandle<Events.ArtistContentUpdated>, IHandle<Events.DatabaseItemChanged> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITagManager			mTagManager;
		private readonly Observal.Observer		mChangeObserver;
		private UiArtist						mCurrentArtist;
		private LinkNode						mArtistWebsite;
		private TaskHandler<DbArtist>			mTaskHandler; 
		private readonly InteractionRequest<ArtistEditRequest>		mArtistEditRequest;

		public ArtistViewModel( IEventAggregator eventAggregator, IArtistProvider artistProvider, ITagManager tagManager ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;

			mEventAggregator.Subscribe( this );

			mChangeObserver = new Observal.Observer();
			mChangeObserver.Extend( new PropertyChangedExtension()).WhenPropertyChanges( OnArtistChanged );

			mArtistEditRequest = new InteractionRequest<ArtistEditRequest>(); 
		}

		public UiArtist Artist {
			get{ return( mCurrentArtist ); }
		}

		[DependsUpon( "Artist" )]
		public bool ArtistValid {
			get{ return( Artist != null ); }
		}

		private UiArtist TransformArtist( DbArtist dbArtist ) {
			var retValue = new UiArtist();

			Mapper.DynamicMap( dbArtist, retValue );
			retValue.DisplayGenre = mTagManager.GetGenre( dbArtist.Genre );

			return( retValue );
		}

		private void SetCurrentArtist( DbArtist artist ) {
			CurrentArtist = artist != null ? TransformArtist( artist ) : null;
		}

		private UiArtist CurrentArtist {
			get{ return( mCurrentArtist ); }
			set {
				if( mCurrentArtist != null ) {
					mChangeObserver.Release( mCurrentArtist );

					if(( value != null ) &&
					   ( mCurrentArtist.DbId != value.DbId )) {
					}
				}

				if( value != null ) {
					mEventAggregator.Publish( new Events.ArtistContentRequest( value.DbId ));

					mCurrentArtist = value;
					mChangeObserver.Add( mCurrentArtist );
					RaisePropertyChanged( () => Artist );

					mArtistWebsite = new LinkNode( CurrentArtist.Website, 0, OnWebsiteRequested );
					RaisePropertyChanged( () => ArtistWebsite );
				}
				else {
					mCurrentArtist = null;
				}
			}
		}

		public void Handle( Events.ArtistContentUpdated eventArgs ) {
			if(( CurrentArtist != null ) &&
			   ( CurrentArtist.DbId == eventArgs.ArtistId )) {
				CurrentArtist = TransformArtist( mArtistProvider.GetArtist( eventArgs.ArtistId ));
			}
		}

		public void Handle( Events.ArtistFocusRequested request ) {
			if( CurrentArtist != null ) {
				if( request.ArtistId != CurrentArtist.DbId ) {
					RequestArtist( request.ArtistId );
				}
			}
			else {
				RequestArtist( request.ArtistId );
			}
		}

		public void Handle( Events.AlbumFocusRequested request ) {
			if( CurrentArtist != null ) {
				if( request.ArtistId != CurrentArtist.DbId ) {
					RequestArtist( request.ArtistId );
				}
			}
			else {
				RequestArtist( request.ArtistId );
			}
		}

		internal TaskHandler<DbArtist> TaskHandler {
			get {
				if( mTaskHandler == null ) {
					mTaskHandler = new TaskHandler<DbArtist>();
				}

				return( mTaskHandler );
			}

			set { mTaskHandler = value; }
		} 

		private void RequestArtist( long artistId ) {
			TaskHandler.StartTask( () => mArtistProvider.GetArtist( artistId ), 
									SetCurrentArtist,
									exception => NoiseLogger.Current.LogException( "ArtistViewModel:GetArtist", exception ));
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
				mEventAggregator.Publish( new Events.UrlLaunchRequest( CurrentArtist.Website ));
			}
		}

		[DependsUpon( "Artist" )]
		public byte[] ArtistImage {
			get {
				byte[]	retValue = null;

				return( retValue );
			}
		}

		public LinkNode ArtistWebsite {
			get{ return( mArtistWebsite ); }
		}

		public IInteractionRequest ArtistEditRequest {
			get{ return( mArtistEditRequest ); }
		}

		public void Execute_EditArtist() {
			if( mCurrentArtist != null ) {
				mArtistEditRequest.Raise( new ArtistEditRequest( mCurrentArtist ), OnArtistEdited );
			}
		}

		[DependsUpon( "Artist" )]
		public bool CanExecute_EditArtist() {
			return( CurrentArtist != null );
		}

		private void OnArtistEdited( ArtistEditRequest confirmation ) {
			if( confirmation.Confirmed ) {
				using( var updater = mArtistProvider.GetArtistForUpdate( confirmation.Artist.DbId )) {
					if( updater.Item != null ) {
						Mapper.DynamicMap( confirmation.Artist, updater.Item );
						updater.Update();
					}
				}
			}
		}
	}
}
