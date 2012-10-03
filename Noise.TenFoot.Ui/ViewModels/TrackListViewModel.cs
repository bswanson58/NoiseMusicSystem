﻿using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.TenFoot.Ui.Interfaces;
using ReusableBits;

namespace Noise.TenFoot.Ui.ViewModels {
	public class TrackListViewModel : BaseListViewModel<DbTrack>, IAlbumTrackList, ITitledScreen {
		private readonly ITrackProvider		mTrackProvider;
		private long						mCurrentAlbum;
		private TaskHandler					mTrackRetrievalTaskHandler;

		public	string						ScreenTitle { get; private set; }
		public	string						Context { get; private set; }

		public TrackListViewModel( IEventAggregator eventAggregator, ITrackProvider trackProvider ) :
			base( eventAggregator ) {
			mTrackProvider = trackProvider;

			ScreenTitle = "Tracks";
			Context = "";
		}

		public void SetContext( DbAlbum album ) {
			if(( album != null ) &&
			   ( mCurrentAlbum != album.DbId )) {
				ItemList.Clear();

				mCurrentAlbum = album.DbId;
				RetrieveTracksForAlbum( mCurrentAlbum );

				Context = album.Name;
			}
		}

		internal TaskHandler TrackRetrievalTaskHandler {
			get {
				if( mTrackRetrievalTaskHandler == null ) {
					mTrackRetrievalTaskHandler = new TaskHandler();
				}

				return( mTrackRetrievalTaskHandler );
			}

			set{ mTrackRetrievalTaskHandler = value; }
		}

		private void RetrieveTracksForAlbum( long albumId ) {
			TrackRetrievalTaskHandler.StartTask( () => {
			                                     	using( var trackList = mTrackProvider.GetTrackList( albumId )) {
			                                     		ItemList.AddRange( trackList.List );
			                                     	}
			                                     },
												 () => { SelectedItem = ItemList.FirstOrDefault(); }, 
												 ex => NoiseLogger.Current.LogException( "TrackListViewModel:RetrieveTracksForAlbum", ex )
				);
		}

		protected override void DisplayItem() {
			GlobalCommands.PlayTrack.Execute( SelectedItem );
		}

		protected override void EnqueueItem() {
			GlobalCommands.PlayTrack.Execute( SelectedItem );
		}

		protected override void DequeueItem() {
			EventAggregator.Publish( new Input.Events.DequeueTrack( SelectedItem ));
		}
	}
}
