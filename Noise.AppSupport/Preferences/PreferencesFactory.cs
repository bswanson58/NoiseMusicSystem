using Noise.Infrastructure.Interfaces;
using Unity;

namespace Noise.AppSupport.Preferences {
	public static class PreferencesFactory<T> where T : new() {
		public static T CreatePreferences( IUnityContainer container ) {
			var preferences = container.Resolve<IPreferences>();

			return( preferences.Load<T>());
		}
	}
}
