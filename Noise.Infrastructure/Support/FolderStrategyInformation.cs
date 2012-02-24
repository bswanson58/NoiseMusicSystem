using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Support {
	public class FolderStrategyInformation {
		private readonly Dictionary<eFolderStrategy, string>	mStrategyDefinitions;
		public	bool											PreferFolderStrategy { get; set; }

		public FolderStrategyInformation() {
			mStrategyDefinitions = new Dictionary<eFolderStrategy, string>();
		}

		public void SetStrategyInformation( eFolderStrategy forStrategy, string definition ) {
			if(!mStrategyDefinitions.ContainsKey( forStrategy )) {
				mStrategyDefinitions.Add( forStrategy, definition );
			}
		}

		public string GetStrategyDefinition( eFolderStrategy forStrategy ) {
			var retValue = "";

			if( mStrategyDefinitions.ContainsKey( forStrategy )) {
				retValue = mStrategyDefinitions[forStrategy];
			}

			return( retValue );
		}
	}
}
