using System.Configuration;

namespace Composite.Layout.Configuration {
	public static class LayoutConfigurationManager {
		private static readonly ILayoutManager _LayoutManager;

		static LayoutConfigurationManager() {
			var _Provider = ConfigurationManager.GetSection( "layoutProvider" ) as LayoutProviderBase;

			if( _Provider != null ) {
				_LayoutManager = _Provider.LayoutManager;
			}
		}

		public static ILayoutManager LayoutManager {
			get { return _LayoutManager; }
		}
	}
}