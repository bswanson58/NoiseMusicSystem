using System;
using CommonServiceLocator;
using Noise.AppSupport.Logging;
using Noise.AppSupport.Preferences;
using Noise.AppSupport.Support;
using Noise.AudioSupport;
using Noise.Guide.Browser;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;
using Noise.Infrastructure.RemoteHost;
using Noise.Metadata;
using Prism.Logging;
using Prism.Modularity;
using Prism.Unity;
using Unity;

namespace Noise.AppSupport {
	public enum ApplicationUsage {
		Desktop,
		TenFootUi,
		Server,
		Librarian
	}

	public class IocConfiguration {
		private readonly IUnityContainer	mContainer;

		public IocConfiguration() :
			this( new UnityContainer()) {
		}

		public IocConfiguration( IUnityContainer container ) {
			mContainer = container;
		}

		public IUnityContainer Container => mContainer;

        public bool InitializeIoc( ApplicationUsage appUsage ) {
			mContainer.RegisterSingleton<IPlatformLog, SeriLogAdapter>();
			mContainer.RegisterSingleton<IFileWriter, JsonObjectWriter>();
			mContainer.RegisterSingleton<IApplicationLog, ApplicationLogger>();
			mContainer.RegisterSingleton<INoiseLog, NoiseLogger>();
			mContainer.RegisterSingleton<IIoc, IocProvider>();

			mContainer.RegisterFactory<AudioPreferences>( PreferencesFactory<AudioPreferences>.CreatePreferences );
			mContainer.RegisterFactory<LoggingPreferences>( PreferencesFactory<LoggingPreferences>.CreatePreferences );
			mContainer.RegisterFactory<NoiseCorePreferences>( PreferencesFactory<NoiseCorePreferences>.CreatePreferences );
			mContainer.RegisterFactory<UserInterfacePreferences>( PreferencesFactory<UserInterfacePreferences>.CreatePreferences );

			switch( appUsage ) {
				case ApplicationUsage.Desktop:
				case ApplicationUsage.Librarian:
					mContainer.RegisterInstance( new RemoteHostConfiguration( 6503, "Noise Desktop System" ));
					mContainer.RegisterInstance<INoiseEnvironment>( new NoiseEnvironment( Constants.DesktopPreferencesDirectory ));
					mContainer.RegisterSingleton<IPreferences, PreferencesManager>();

					break;

				case ApplicationUsage.Server:
					mContainer.RegisterInstance( new RemoteHostConfiguration( 6503, "Noise Headless Service" ));
					mContainer.RegisterInstance<INoiseEnvironment>( new NoiseEnvironment( Constants.HeadlessPreferencesDirectory ));
					mContainer.RegisterSingleton<IPreferences, HeadlessPreferences>();

					InitializeUnity();

					break;

				case ApplicationUsage.TenFootUi:
					mContainer.RegisterInstance( new RemoteHostConfiguration( 6503, "Noise TenFoot System" ));
					mContainer.RegisterInstance<INoiseEnvironment>( new NoiseEnvironment( Constants.TenFootPreferencesDirectory ));
					mContainer.RegisterSingleton<IPreferences, PreferencesManager>();

					break;
			}

			if( appUsage == ApplicationUsage.Desktop ) {
				var guideInitialization = mContainer.Resolve<CefInitialization>();

				guideInitialization.Initialize();
            }

			return true;
		}

		private void InitializeUnity() {
			try {

				mContainer.RegisterInstance<ILoggerFacade>( new TextLogger());

				var catalog = new ModuleCatalog();

				catalog.AddModule( typeof( Core.NoiseCoreModule ))
						.AddModule( typeof( EntityFrameworkDatabase.EntityFrameworkDatabaseModule ))
						.AddModule( typeof( AudioSupportModule ))
						.AddModule( typeof( BlobStorage.BlobStorageModule ))
						.AddModule( typeof( NoiseMetadataModule ))
						.AddModule( typeof( RemoteHost.RemoteHostModule ));
				mContainer.RegisterInstance<IModuleCatalog>( catalog );

				mContainer.RegisterSingleton<IServiceLocator, UnityServiceLocatorAdapter>();
				mContainer.RegisterSingleton<IModuleInitializer, ModuleInitializer>();
				mContainer.RegisterSingleton<IModuleManager, ModuleManager>();

				ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ActivationException));
				ExceptionExtensions.RegisterFrameworkExceptionType( typeof( ResolutionFailedException));

				ServiceLocator.SetLocatorProvider( () => mContainer.Resolve<IServiceLocator>());

				mContainer.Resolve<IModuleManager>().Run();
			}
			catch( Exception ex ) {
				if( mContainer != null ) {
					var logger = mContainer.Resolve<ILoggerFacade>();

					logger.Log( $"Failed to initialize Unity: {ex.Message}", Category.Exception, Priority.High );
				}
			}
		}
	}
}
