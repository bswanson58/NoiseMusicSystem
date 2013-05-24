using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class StartupLibrarySelectionViewModel : AutomaticCommandBase {
		private readonly IEventAggregator							mEventAggregator;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries;
		public	bool												AlwaysOpenLastUsedLibrary { get; set; }

		public StartupLibrarySelectionViewModel( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;

			mLibraries = new BindableCollection<LibraryConfiguration>();
			LoadLibraries();
		}

		public BindableCollection<LibraryConfiguration> LibraryList {
			get { return ( mLibraries ); }
		} 

		public LibraryConfiguration SelectedLibrary {
			get{ return( null ); }
			set {
				mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
				mLibraryConfiguration.Open( value );

				if( AlwaysOpenLastUsedLibrary ) {
					var expConfig = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

					if( expConfig != null ) {
						expConfig.LoadLastLibraryOnStartup = AlwaysOpenLastUsedLibrary;

						NoiseSystemConfiguration.Current.Save( expConfig );
					}
				}
			}
		}

		private void LoadLibraries() {
			mLibraries.IsNotifying = false;
			mLibraries.Clear();
			mLibraries.AddRange( from library in mLibraryConfiguration.Libraries orderby library.LibraryName select library );
			mLibraries.IsNotifying = true;
			mLibraries.Refresh();
		}

		public void Execute_CreateLibrary() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.LibraryCreationLayout ));
		}

		public void Execute_Exit() {
			mEventAggregator.Publish( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
		}
	}
}
