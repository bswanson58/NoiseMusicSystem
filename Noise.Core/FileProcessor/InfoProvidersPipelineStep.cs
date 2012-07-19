using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Core.DataProviders;

namespace Noise.Core.FileProcessor {
	internal class InfoProvidersPipelineStep : BasePipelineStep {
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly DefaultProvider		mDefaultProvider;

		public InfoProvidersPipelineStep( FolderStrategyProvider folderStrategyProvider, DefaultProvider defaultProvider ) :
			base( ePipelineStep.BuildInfoProviders ) {
			mStrategyProvider = folderStrategyProvider;
			mDefaultProvider = defaultProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context ).IsNotNull();
			Condition.Requires( context.StorageFile ).IsNotNull();

			context.MetaDataProviders = new List<IMetaDataProvider>{ mStrategyProvider.GetProvider( context.StorageFile ),
			                                                       	 mDefaultProvider.GetProvider( context.StorageFile ) };
		}
	}
}
