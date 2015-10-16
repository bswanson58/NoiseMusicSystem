using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport.Preferences {
	public static class PreferencesFactory<T> where T : new() {
		public static T CreatePreferences( IUnityContainer container ) {
			var preferences = container.Resolve<IPreferences>();

			return( preferences.Load<T>());
		}
	}
}
