using System.Collections.Generic;
using CuttingEdge.Conditions;
using Noise.Core.DataProviders;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileProcessor {
	internal class MusicProvidersPipelineStep : BasePipelineStep {
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly FileNameProvider		mFileNameProvider;
		private readonly FileTagProvider		mTagProvider;
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly DefaultProvider		mDefaultProvider;

		public MusicProvidersPipelineStep( IStorageFolderSupport storageFolderSupport,
										   FileNameProvider fileNameProvider, FileTagProvider fileTagProvider,
										   FolderStrategyProvider folderStrategyProvider, DefaultProvider defaultProvider ) :
			base( ePipelineStep.BuildMusicProviders ) {
			mStorageFolderSupport = storageFolderSupport;
			mFileNameProvider = fileNameProvider;
			mTagProvider = fileTagProvider;
			mStrategyProvider = folderStrategyProvider;
			mDefaultProvider = defaultProvider;
		}

		public override void ProcessStep( PipelineContext context ) {
			Condition.Requires( context, "context" ).IsNotNull();
			Condition.Requires( context.StorageFile, "StorageFile" ).IsNotNull();
			Condition.Requires( context.Track, "Track" ).IsNotNull();

			var folderStrategy = mStorageFolderSupport.GetFolderStrategy( context.StorageFile );

			context.MetaDataProviders = new List<IMetadataInfoProvider>();
			if( folderStrategy.PreferFolderStrategy ) {
				context.MetaDataProviders.Add( mStrategyProvider.GetProvider( context.StorageFile ));
				context.MetaDataProviders.Add( mFileNameProvider.GetProvider( context.StorageFile ) );
				context.MetaDataProviders.Add( mTagProvider.GetProvider( context.StorageFile, context.Track.Encoding ) );
				context.MetaDataProviders.Add( mDefaultProvider.GetProvider( context.StorageFile ));
			}
			else {
				context.MetaDataProviders.Add( mTagProvider.GetProvider( context.StorageFile, context.Track.Encoding ));
				context.MetaDataProviders.Add( mStrategyProvider.GetProvider( context.StorageFile ));
				context.MetaDataProviders.Add( mFileNameProvider.GetProvider( context.StorageFile ));
				context.MetaDataProviders.Add( mDefaultProvider.GetProvider( context.StorageFile ));
			}

            foreach( var provider in context.MetaDataProviders ) {
                Condition.Ensures( provider, "MetaDataProvider" ).IsNotNull();
            }
		}
	}
}
