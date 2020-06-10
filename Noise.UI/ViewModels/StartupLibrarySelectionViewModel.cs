using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Prism.Commands;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class StartupLibrarySelectionViewModel : AutomaticPropertyBase {
		private readonly IEventAggregator							mEventAggregator;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly IPreferences								mPreferences;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries;
		public	bool												AlwaysOpenLastUsedLibrary { get; set; }
        public	BindableCollection<LibraryConfiguration>			LibraryList => mLibraries;

		public	DelegateCommand										CreateLibrary { get; }
		public	DelegateCommand										Exit { get; }

		public StartupLibrarySelectionViewModel( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration, IPreferences preferences ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;
			mPreferences = preferences;

			mLibraries = new BindableCollection<LibraryConfiguration>();

			CreateLibrary = new DelegateCommand( OnCreateLibrary );
			Exit = new DelegateCommand( OnExit );

			LoadLibraries();
		}

        public LibraryConfiguration SelectedLibrary {
			get{ return( Get( () => SelectedLibrary )); }
			set {
				Set( () => SelectedLibrary, value );
				OpenLibrary( value );
			}
		}

		public bool IsLoading {
			get{ return( Get( () => IsLoading )); }
			set{ Set( () => IsLoading, value ); }
		}

		[DependsUpon("IsLoading")]
		public bool IsNotLoading => !IsLoading;

        private async void OpenLibrary( LibraryConfiguration configuration ) {
			IsLoading = true;

			await mLibraryConfiguration.AsyncOpen( configuration );
			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));

			if( AlwaysOpenLastUsedLibrary ) {
				var preferences = mPreferences.Load<NoiseCorePreferences>();

				preferences.LoadLastLibraryOnStartup = AlwaysOpenLastUsedLibrary;
				mPreferences.Save( preferences );
			}
		}

		private void LoadLibraries() {
			mLibraries.IsNotifying = false;
			mLibraries.Clear();
			mLibraries.AddRange( from library in mLibraryConfiguration.Libraries orderby library.LibraryName select library );
			mLibraries.IsNotifying = true;
			mLibraries.Refresh();
		}

		private void OnCreateLibrary() {
			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.LibraryCreationLayout ));
		}

		private void OnExit() {
			mEventAggregator.PublishOnUIThread( new Events.WindowLayoutRequest( Constants.ExploreLayout ));
		}
	}
}
