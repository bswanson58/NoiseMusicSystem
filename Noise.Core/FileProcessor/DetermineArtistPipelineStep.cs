using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class DetermineArtistPipelineStep : BasePipelineStep {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly IArtistProvider		mArtistProvider;

		public DetermineArtistPipelineStep( IArtistProvider artistProvider, IStorageFolderSupport storageFolderSupport, IEventAggregator eventAggregator ) :
			base( ePipelineStep.DetermineArtist ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
			mStorageFolderSupport = storageFolderSupport;
		}

		public override void ProcessStep( PipelineContext context ) {
			var	artistName = "";

			foreach( var provider in context.MetaDataProviders ) {
				artistName = provider.Artist;

				if(!string.IsNullOrWhiteSpace( artistName )) {
					break;
				}
			}

			if(!string.IsNullOrWhiteSpace( artistName )) {
				context.Artist = context.ArtistCache.Find( a => a.Name == artistName );
				if( context.Artist == null ) {
					context.Artist = new DbArtist { Name = artistName };

					mArtistProvider.AddArtist( context.Artist );
					context.ArtistCache.Add( context.Artist );

					context.Summary.ArtistsAdded++;

					mEventAggregator.Publish( new Events.ArtistAdded( context.Artist.DbId ));
					NoiseLogger.Current.LogInfo( "Added artist: {0}", context.Artist.Name );
				}
			}
			else {
				NoiseLogger.Current.LogMessage( "Artist cannot determined for file: {0}", mStorageFolderSupport.GetPath( context.StorageFile ));
			}
		}
	}
}
