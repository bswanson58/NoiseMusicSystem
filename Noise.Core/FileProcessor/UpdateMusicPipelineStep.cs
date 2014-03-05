using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class UpdateMusicPipelineStep :BasePipelineStep {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IArtistProvider		mArtistProvider;
		private readonly ITrackProvider			mTrackProvider;

		public UpdateMusicPipelineStep( IEventAggregator eventAggregator, IArtistProvider artistProvider, ITrackProvider trackProvider,
										IStorageFileProvider storageFileProvider ) :
			base( ePipelineStep.UpdateMusic ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mTrackProvider = trackProvider;
			mStorageFileProvider = storageFileProvider;
		}

		// Update the database tracks with properties that could have been changed from the file metadata.
		private void UpdateTrack( DbTrack targetTrack, DbTrack sourceTrack ) {
			targetTrack.Name = sourceTrack.Name;
			targetTrack.Performer = sourceTrack.Performer;
			targetTrack.DurationMilliseconds = sourceTrack.DurationMilliseconds;
			targetTrack.Bitrate = sourceTrack.Bitrate;
			targetTrack.SampleRate = sourceTrack.SampleRate;
			targetTrack.Channels = sourceTrack.Channels;
			targetTrack.TrackNumber = sourceTrack.TrackNumber;
			targetTrack.VolumeName = sourceTrack.VolumeName;
			targetTrack.Encoding = sourceTrack.Encoding;
			targetTrack.ExternalGenre = sourceTrack.ExternalGenre;
			targetTrack.ReplayGainAlbumGain = sourceTrack.ReplayGainAlbumGain;
			targetTrack.ReplayGainAlbumPeak = sourceTrack.ReplayGainAlbumPeak;
			targetTrack.ReplayGainTrackGain = sourceTrack.ReplayGainTrackGain;
			targetTrack.ReplayGainTrackPeak = sourceTrack.ReplayGainTrackPeak;

			// Fields that could be updated:
			//	Rating
			//	IsFavorite
			//	PlayCount
			//	PublishedYear
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();
			Condition.Requires( context.Track ).IsNotNull();
			Condition.Requires( context.Summary ).IsNotNull();

			if(( context.Artist != null ) &&
			   ( context.Album != null )) {
				context.Track.Artist = context.Artist.DbId;
				context.Track.Album = context.Album.DbId;

				if( context.StorageFile.WasUpdated ) {
					using( var updater = mTrackProvider.GetTrackForUpdate( context.Track.DbId )) {
						if( updater.Item != null ) {
							UpdateTrack( updater.Item, context.Track );

							updater.Update();
						}
					}
				}
				else {
					mTrackProvider.AddTrack( context.Track );
				}

				using( var updater = mStorageFileProvider.GetFileForUpdate( context.StorageFile.DbId )) {
					if( updater.Item != null ) {
						updater.Item.MetaDataPointer = context.Track.DbId;
						updater.Item.FileType = eFileType.Music;
						updater.Item.WasUpdated = false;

						updater.Update();
					}
				}

				context.Summary.TracksAdded++;

				var albumsAdded = false;
				using( var updater = mArtistProvider.GetArtistForUpdate( context.Artist.DbId )) {
					if( updater.Item != null ) {
						albumsAdded = updater.Item.AlbumCount != context.Artist.AlbumCount;

						updater.Item.AlbumCount = context.Artist.AlbumCount;
						updater.Item.UpdateLastChange();

						updater.Update();
					}
				}

				if( albumsAdded ) {
					mEventAggregator.Publish( new Events.ArtistContentUpdated( context.Artist.DbId ));
				}
			}
		}
	}
}
