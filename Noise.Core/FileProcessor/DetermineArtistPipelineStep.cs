using Caliburn.Micro;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class DetermineArtistPipelineStep : BasePipelineStep {
		private readonly IEventAggregator		mEventAggregator;
		private readonly IArtistProvider		mArtistProvider;

		public DetermineArtistPipelineStep( IArtistProvider artistProvider, IEventAggregator eventAggregator ) :
			base( ePipelineStep.DetermineArtist ) {
			mEventAggregator = eventAggregator;
			mArtistProvider = artistProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.MetaDataProviders ).IsNotEmpty();

			var	artistName = string.Empty;

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

					mEventAggregator.PublishOnUIThread( new Events.ArtistAdded( context.Artist.DbId ));
					context.Log.LogArtistAdded( context.StorageFile, context.Artist );
				}
				else {
					context.Log.LogArtistFound( context.StorageFile, context.Artist );
				}
			}
			else {
				context.Log.LogArtistNotFound( context.StorageFile );
			}
		}
	}
}
