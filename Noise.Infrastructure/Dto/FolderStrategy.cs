using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public enum eFolderStrategy {
		Artist = 1,
		Album = 2,
		Genre = 3,
		Volume = 4,
		Undefined = 0
	}

	public class FolderStrategy : DbBase {
		private	const int		cMaxStrategyLevel = 5;

		private readonly List<eFolderStrategy>	mFolderLevelStrategies;
		public	bool							PreferFolderStrategy { get; set; }

		public FolderStrategy() {
			mFolderLevelStrategies = new List<eFolderStrategy>();

			for( int level = 0; level < cMaxStrategyLevel; level++ ) {
				mFolderLevelStrategies.Add( eFolderStrategy.Undefined );
			}
		}

		public void EloqueraFixUp() {
//			mFolderLevelStrategies = Array.ConvertAll( mFolderLevelStrategies, value => (eFolderStrategy)value );
		}

		public eFolderStrategy StrategyForLevel( int folderLevel ) {
			var retValue = eFolderStrategy.Undefined;

			if( folderLevel < cMaxStrategyLevel ) {
				retValue = mFolderLevelStrategies[folderLevel];
			}

			return( retValue );
		}

		public void SetStrategyForLevel( int folderLevel, eFolderStrategy strategy ) {
			if( folderLevel < cMaxStrategyLevel ) {
				mFolderLevelStrategies[folderLevel] = strategy;
			}
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( FolderStrategy )); }
		}
	}
}
