using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public class AlbumListViewModel : BaseListViewModel<DbAlbum>, IAlbumList {
		private readonly IAlbumTrackList				mAlbumTrackList;
		private readonly IAlbumProvider					mAlbumProvider;
		private long									mCurrentArtist;
		private TaskHandler								mAlbumRetrievalTaskHandler;

		public AlbumListViewModel( IEventAggregator eventAggregator, IAlbumTrackList trackListViewModel, IAlbumProvider albumProvider ) :
			base( eventAggregator ) {
			mAlbumTrackList = trackListViewModel;
			mAlbumProvider = albumProvider;
		}

		public void SetContext( long artistId ) {
			if( mCurrentArtist != artistId ) {
				ItemList.Clear();

				mCurrentArtist = artistId;
				RetrieveAlbumsForArtist( mCurrentArtist );
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
			                                     		ItemList.AddRange( albumList.List );
			                                     	}
			                                     },
												 () => { SelectedItem = ItemList.FirstOrDefault(); },
												 ex => NoiseLogger.Current.LogException( "AlbumListViewModel:RetrieveAlbumsForArtist", ex )
				);
		}

		protected override void NextItem() {
			if( ItemList.Any()) {
				if( SelectedItem != null ) {
					var index = ItemList.IndexOf( SelectedItem );

					if(( index != -1 ) &&
					  (( index + 1 ) < ItemList.Count )) {
						SelectedItem = ItemList[index + 1];
					}
					else {
						SelectedItem = ItemList[0];
					}
				}
				else {
					SelectedItem = ItemList[0];
				}
			}
		}

		protected override void PreviousItem() {
			if( ItemList.Any()) {
				if( SelectedItem != null ) {
					var index = ItemList.IndexOf( SelectedItem );

					if( index > 0 ) {
						SelectedItem = ItemList[index - 1 ];
					}
				}
				else {
					SelectedItem = ItemList[ItemList.Count - 1];
				}
			}
		}

		protected override void DisplayItem() {
			if(( Parent is INavigate ) &&
			   ( SelectedItem != null )) {
				var controller = Parent as INavigate;

				mAlbumTrackList.SetContext( SelectedItem.DbId );
				controller.NavigateTo( mAlbumTrackList );
			}
		}

		protected override void PlayItem() {
			GlobalCommands.PlayAlbum.Execute( SelectedItem );
		}
	}
}
