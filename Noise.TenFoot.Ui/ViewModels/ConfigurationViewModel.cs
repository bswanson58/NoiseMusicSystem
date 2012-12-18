using Caliburn.Micro;
using Noise.Infrastructure.Configuration;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ConfigurationViewModel : Screen {
		private	bool	mAllowInternet;
		private	bool	mEnableRemote;

		protected override void OnActivate() {
			base.OnActivate();

			DisplayName = "Configuration";

			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				EnableRemote = configuration.EnableRemoteAccess;
				AllowInternet = configuration.HasNetworkAccess;
			}
		}

		private void UpdateConfiguration() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration != null ) {
				configuration.EnableRemoteAccess = EnableRemote;
				configuration.HasNetworkAccess = AllowInternet;

				NoiseSystemConfiguration.Current.Save( configuration );
			}
		}

		public bool AllowInternet {
			get{ return( mAllowInternet ); }
			set {
				mAllowInternet = value;

				NotifyOfPropertyChange( () => AllowInternet );
			}
		}

		public bool EnableRemote {
			get{ return( mEnableRemote ); }
			set {
				mEnableRemote = value;

				NotifyOfPropertyChange( () => EnableRemote );
			}
		}

		public void Close() {
			TryClose( true );

			UpdateConfiguration();
		}

		public void Cancel() {
			TryClose( false );
		}
	}
}
