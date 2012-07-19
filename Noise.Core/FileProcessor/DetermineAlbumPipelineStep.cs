using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class DetermineAlbumPipelineStep : BasePipelineStep {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;

		public DetermineAlbumPipelineStep( IEventAggregator eventAggregator, IStorageFolderSupport storageFolderSupport,
										   IArtistProvider artistProvider, IAlbumProvider albumProvider ) :
			base( ePipelineStep.DetermineAlbum ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mAlbumProvider = albumProvider;
			mStorageFolderSupport = storageFolderSupport;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.Summary ).IsNotNull();

			if( context.Artist != null ) {
				var	albumName = "";

				foreach( var provider in context.MetaDataProviders ) {
					albumName = provider.Album;

					if(!string.IsNullOrWhiteSpace( albumName )) {
						break;
					}
				}

				if(!string.IsNullOrWhiteSpace( albumName )) {
					context.Album = context.AlbumCache.Find( album => album.Name == albumName && album.Artist == context.Artist.DbId );
					if( context.Album == null ) {
						context.Album = new DbAlbum { Name = albumName, Artist = context.Artist.DbId };

						mAlbumProvider.AddAlbum( context.Album );
						context.AlbumCache.Add( context.Album );

						using( var updater = mArtistProvider.GetArtistForUpdate( context.Artist.DbId )) {
							if( updater.Item != null ) {
								updater.Item.AlbumCount++;

								updater.Update();
							}
						}
						context.Artist.AlbumCount++;
						context.Summary.AlbumsAdded++;

						mEventAggregator.Publish( new Events.AlbumAdded( context.Album.DbId ));
						NoiseLogger.Current.LogInfo( "Added album: {0}", context.Album.Name );
					}
				}
				else {
					NoiseLogger.Current.LogMessage( "Album cannot be determined for file: {0}", mStorageFolderSupport.GetPath( context.StorageFile ));
				}
			}
		}
	}
}
