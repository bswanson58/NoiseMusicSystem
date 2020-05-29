using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Core.DataProviders;

namespace Noise.Core.FileProcessor {
	class SidecarProvidersPipelineStep : BasePipelineStep {
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly DefaultProvider		mDefaultProvider;

		public SidecarProvidersPipelineStep( FolderStrategyProvider folderStrategyProvider, DefaultProvider defaultProvider ) :
			base( ePipelineStep.BuildSidecarProviders ) {
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
