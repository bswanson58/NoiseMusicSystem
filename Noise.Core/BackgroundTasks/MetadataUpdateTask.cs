using System;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Logging;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.BackgroundTasks {
	public class MetadataUpdateTask : IBackgroundTask,
									  IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing>, IHandle<Events.ArtistMetadataUpdated> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITagManager			mTagManager;
		private readonly IMetadataManager		mMetadataManager;
		private readonly ILogBackgroundTasks	mLog;
		private bool							mDatabaseOpen;

		public string TaskId { get; }

		public MetadataUpdateTask( IEventAggregator eventAggregator, IArtistProvider artistProvider,
								   ITagManager tagManager, IMetadataManager metadataManager, ILogBackgroundTasks log ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mTagManager = tagManager;
			mMetadataManager = metadataManager;
			mLog = log;

			TaskId = "Task_MetadataUpdate";

			mEventAggregator.Subscribe( this );
		}

		public void ExecuteTask() {
			// We just handle metadata update events.
		}

		public void Handle( Events.DatabaseOpened args ) {
			mDatabaseOpen = true;
		}

		public void Handle( Events.DatabaseClosing args ) {
			mDatabaseOpen = false;
		}

		public void Handle( Events.ArtistMetadataUpdated args ) {
			if( mDatabaseOpen ) {
				try {
					var artist = mArtistProvider.FindArtist( args.ArtistName );

					if( artist != null ) {
						var artistMetadata = mMetadataManager.GetArtistMetadata( args.ArtistName );
						var genre = artistMetadata.GetMetadataArray( eMetadataType.Genre ).FirstOrDefault();

						using( var updater = mArtistProvider.GetArtistForUpdate( artist.DbId )) {
							if(!string.IsNullOrWhiteSpace( genre )) {
								updater.Item.ExternalGenre = mTagManager.ResolveGenre( genre );

								updater.Update();

								mEventAggregator.PublishOnUIThread( new Events.ArtistContentUpdated( artist.DbId ));
							}
						}
					}
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "Updating metadata for artist \"{0}\"", args.ArtistName ), ex );
				}
			}
		}
	}
}
