using System;
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
		private	const int	cMaxStrategyLevel = 5;

		private eFolderStrategy[]	mFolderLevelStrategies;
		public	bool				PreferFolderStrategy { get; set; }

		public FolderStrategy() {
			mFolderLevelStrategies = new eFolderStrategy[cMaxStrategyLevel];

			for( int level = 0; level < cMaxStrategyLevel; level++ ) {
				mFolderLevelStrategies[level] = eFolderStrategy.Undefined;
			}
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

		public int DbFolderStrategy1 {
			get{ return((int)mFolderLevelStrategies[0]); }
			protected set{ mFolderLevelStrategies[0] = (eFolderStrategy)value; }
		}

		public int DbFolderStrategy2 {
			get{ return((int)mFolderLevelStrategies[1]); }
			protected set{ mFolderLevelStrategies[1] = (eFolderStrategy)value; }
		}

		public int DbFolderStrategy3 {
			get{ return((int)mFolderLevelStrategies[2]); }
			protected set{ mFolderLevelStrategies[2] = (eFolderStrategy)value; }
		}

		public int DbFolderStrategy4 {
			get{ return((int)mFolderLevelStrategies[3]); }
			protected set{ mFolderLevelStrategies[3] = (eFolderStrategy)value; }
		}

		public int DbFolderStrategy5 {
			get{ return((int)mFolderLevelStrategies[4]); }
			protected set{ mFolderLevelStrategies[4] = (eFolderStrategy)value; }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( FolderStrategy )); }
		}
	}
}
