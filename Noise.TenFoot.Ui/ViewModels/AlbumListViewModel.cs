using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Dto;
using Noise.TenFoot.Ui.Interfaces;
using Noise.UI.Logging;
using ReusableBits;

namespace Noise.TenFoot.Ui.ViewModels {
	public class AlbumListViewModel : BaseListViewModel<DbAlbum>, IAlbumList, ITitledScreen {
		private readonly IAlbumTrackList	mAlbumTrackList;
		private readonly IAlbumProvider		mAlbumProvider;
		private readonly IUiLog				mLog;
		private long						mCurrentArtist;
		private TaskHandler					mAlbumRetrievalTaskHandler;

		public	string						ScreenTitle { get; private set; }
		public	string						Context { get; private set; }

		public AlbumListViewModel( IEventAggregator eventAggregator, IAlbumTrackList trackListViewModel, IAlbumProvider albumProvider, IUiLog log ) :
			base( eventAggregator ) {
			mAlbumTrackList = trackListViewModel;
			mAlbumProvider = albumProvider;
			mLog = log;

			ScreenTitle = "Albums";
			Context = "";
		}

		public void SetContext( UiArtist artist ) {
			if(( artist != null ) &&
			   ( mCurrentArtist != artist.DbId )) {
				ItemList.Clear();

				mCurrentArtist = artist.DbId;
				RetrieveAlbumsForArtist( mCurrentArtist );

				Context = artist.Name;
			}
		}

		internal TaskHandler AlbumRetrievalTaskHandler {
			get {
				if( mAlbumRetrievalTaskHandler == null ) {
					mAlbumRetrievalTaskHandler = new TaskHandler();
				}

				return( mAlbumRetrievalTaskHandler );
			}

			set{ mAlbumRetrievalTaskHandler = value; }
		}

		private void RetrieveAlbumsForArtist( long artistId ) {
			AlbumRetrievalTaskHandler.StartTask( () => {
			                                     	using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
			                                     		ItemList.AddRange( from album in albumList.List orderby album.Name select album );
			                                     	}
			                                     },
												 () => { SelectedItem = ItemList.FirstOrDefault(); },
												 ex => mLog.LogException( string.Format( "Retrieving Albums for Artist:{0}", artistId ), ex )
				);
		}

		protected override void DisplayItem() {
			mAlbumTrackList.SetContext( SelectedItem );
			EventAggregator.Publish( new Input.Events.NavigateToScreen( mAlbumTrackList ));
		}

		protected override void EnqueueItem() {
			GlobalCommands.PlayAlbum.Execute( SelectedItem );
		}

		protected override void DequeueItem() {
			EventAggregator.Publish( new Input.Events.DequeueAlbum( SelectedItem ));
		}
	}
}
