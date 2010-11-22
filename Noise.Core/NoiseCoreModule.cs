﻿using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataBuilders;
using Noise.Core.FileStore;
using Noise.Core.MediaPlayer;
using Noise.Core.PlayHistory;
using Noise.Core.PlayQueue;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core {
	public class NoiseCoreModule : IModule {
		private readonly IUnityContainer    mContainer;

		public NoiseCoreModule( IUnityContainer container ) {
			mContainer = container;
		}

		public void Initialize() {
			mContainer.RegisterType<IAudioPlayer, AudioPlayer>();
			mContainer.RegisterType<IContentManager, ContentManager>();
			mContainer.RegisterType<IDatabase, EloqueraDatabase>();
			mContainer.RegisterType<IDataProvider, DataProvider>();
			mContainer.RegisterType<IDataUpdates, DataUpdates>();
			mContainer.RegisterType<IDatabaseManager, DatabaseManager>( Constants.NewInstance );
			mContainer.RegisterType<IFolderExplorer, FolderExplorer>();
			mContainer.RegisterType<IFileUpdates, FileUpdates>();
//			mContainer.RegisterType<ILibraryBuilder, LibraryBuilder>();
			mContainer.RegisterType<IMetaDataExplorer, MetaDataExplorer>();
			mContainer.RegisterType<ISearchBuilder, SearchBuilder>();
			mContainer.RegisterType<ISummaryBuilder, SummaryBuilder>();
			mContainer.RegisterType<INoiseManager, NoiseManager>();
			mContainer.RegisterType<IPlayQueue, PlayQueueMgr>();
			mContainer.RegisterType<IPlayHistory, PlayHistoryMgr>();
			mContainer.RegisterType<IPlayListMgr, PlayListMgr>();
			mContainer.RegisterType<IPlayController, PlayController>();
			mContainer.RegisterType<ISearchProvider, LuceneSearchProvider>();
			mContainer.RegisterType<ITagManager, TagManager>();
			mContainer.RegisterType<DatabaseStatistics, DatabaseStatistics>();
		}
	}
}
