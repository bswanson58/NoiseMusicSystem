using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class DetermineTrackPipelineStep : BasePipelineStep {
		private readonly IStorageFolderSupport	mStorageFolderSupport;

		public DetermineTrackPipelineStep( IStorageFolderSupport storageFolderSupport ) :
			base( ePipelineStep.DetermineTrackName ) {
			mStorageFolderSupport = storageFolderSupport;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.Track ).IsNotNull();
			Condition.Requires( context.MetaDataProviders ).IsNotEmpty();

			foreach( var provider in context.MetaDataProviders ) {
				context.Track.Name = provider.TrackName;

				if(!string.IsNullOrWhiteSpace( context.Track.Name )) {
					break;
				}
			}

			if( string.IsNullOrWhiteSpace( context.Track.Name )) {
				NoiseLogger.Current.LogMessage( "Track name cannot be determined for file: {0}", mStorageFolderSupport.GetPath( context.StorageFile ));
			}
		}
	}
}
