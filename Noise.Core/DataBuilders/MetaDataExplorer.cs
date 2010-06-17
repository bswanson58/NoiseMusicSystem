using Microsoft.Practices.Unity;
using Noise.Core.Database;

namespace Noise.Core.DataBuilders {
	public class MetaDataExplorer : IMetaDataExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabase;

		public  MetaDataExplorer( IUnityContainer container ) {
			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
		}

		public void BuildMetaData() {
			
		}
	}
}
