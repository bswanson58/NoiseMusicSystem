using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class UiPlayHistory : ViewModelBase {
		private readonly Action<DbTrack>	mPlayTrackAction;

		public DbArtist		Artist { get; private set; }
		public DbAlbum		Album { get; private set; }
		public DbTrack		Track { get; private set; }
		public DateTime		PlayedOn { get; private set; }

		public UiPlayHistory( DbArtist artist, DbAlbum album, DbTrack track, DateTime playedOn, Action<DbTrack> playTrackAction ) {
			Artist = artist;
			Album = album;
			Track = track;
			PlayedOn = playedOn;

			mPlayTrackAction = playTrackAction;
		}

		public void Execute_PlayTrack( object sender ) {
			var track = sender as DbTrack;

			if(( track != null ) &&
			   ( mPlayTrackAction != null )) {
				mPlayTrackAction( Track );
			}
		}
	}

	public class PlayHistoryViewModel : ViewModelBase {
		private IUnityContainer			mContainer;
		private IEventAggregator		mEvents;
		private INoiseManager			mNoiseManager;
		private readonly ObservableCollectionEx<UiPlayHistory>	mHistoryList;

		public PlayHistoryViewModel() {
			mHistoryList = new ObservableCollectionEx<UiPlayHistory>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.PlayHistoryChanged>().Subscribe( OnPlayHistoryChanged );

				PopulatePlayHistory();
			}
		}

		private void OnPlayHistoryChanged( IPlayHistory playHistory ) {
			PopulatePlayHistory();
		}

		private void PopulatePlayHistory() {
			Invoke( () => {
				mHistoryList.SuspendNotification();
				mHistoryList.Clear();

				var historyList = from DbPlayHistory history in mNoiseManager.PlayHistory.PlayHistory orderby history.PlayedOn descending select history;
				foreach( var history in historyList ) {
					var track = mNoiseManager.DataProvider.GetTrack( history.Track );

					if( track != null ) {
						var album = mNoiseManager.DataProvider.GetAlbumForTrack( track );

						if( album != null ) {
							var artist = mNoiseManager.DataProvider.GetArtistForAlbum( album );

							if( artist != null ) {
								mHistoryList.Add( new UiPlayHistory( artist, album, track, history.PlayedOn, OnPlayTrack ));
							}
						}
					}
				}

				mHistoryList.ResumeNotification();
				RaisePropertyChanged( () => HistoryList );
			});
		}

		private void OnPlayTrack( DbTrack track ) {
			mEvents.GetEvent<Events.TrackPlayRequested>().Publish( track );
		}

		public ObservableCollection<UiPlayHistory> HistoryList {
			get{ return( mHistoryList ); }
		}
	}
}
