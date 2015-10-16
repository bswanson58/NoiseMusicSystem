using Caliburn.Micro;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.TenFoot.Ui.ViewModels {
	public class ConfigurationViewModel : Screen {
		private readonly IPreferences	mPreferences;
		private	bool					mAllowInternet;
		private	bool					mEnableRemote;

		public ConfigurationViewModel( IPreferences preferences ) {
			mPreferences = preferences;
		}

		protected override void OnActivate() {
			base.OnActivate();

			DisplayName = "Configuration";

			var preferences = mPreferences.Load<NoiseCorePreferences>();

			EnableRemote = preferences.EnableRemoteAccess;
			AllowInternet = preferences.HasNetworkAccess;
		}

		private void UpdateConfiguration() {
			var preferences = mPreferences.Load<NoiseCorePreferences>();

			preferences.EnableRemoteAccess = EnableRemote;
			preferences.HasNetworkAccess = AllowInternet;

			mPreferences.Save( preferences );
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
