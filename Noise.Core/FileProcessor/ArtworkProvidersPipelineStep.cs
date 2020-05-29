using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Core.DataProviders;

namespace Noise.Core.FileProcessor {
	internal class ArtworkProvidersPipelineStep : BasePipelineStep {
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly DefaultProvider		mDefaultProvider;

		public ArtworkProvidersPipelineStep( FolderStrategyProvider folderStrategyProvider, DefaultProvider defaultProvider ) :
			base( ePipelineStep.BuildArtworkProviders ) {
			mStrategyProvider = folderStrategyProvider;
			mDefaultProvider = defaultProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			context.MetaDataProviders = new List<IMetadataInfoProvider>{ mStrategyProvider.GetProvider( context.StorageFile ),
			                                                       	 mDefaultProvider.GetProvider( context.StorageFile ) };
		}
	}
}
