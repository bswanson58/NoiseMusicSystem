using System.IO;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class ExportDatabaseViewModel : AutomaticCommandBase,
										   IHandle<Events.SystemInitialized> {
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibrarian									mLibrarian;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries; 
		private LibraryConfiguration								mCurrentLibrary;

		public ExportDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, ILibraryConfiguration libraryConfiguration ) {
			mLibrarian = librarian;
			mLibraryConfiguration = libraryConfiguration;

			mLibraries = new BindableCollection<LibraryConfiguration>();

			ExportPath = @"D:\Exported Library";

			eventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized message ) {
			mLibraries.Clear();
			mLibraries.AddRange( mLibraryConfiguration.Libraries );

			CurrentLibrary = mLibraries.FirstOrDefault();
		}

		public BindableCollection<LibraryConfiguration> LibraryList {
			get {  return( mLibraries ); }
		}

		public LibraryConfiguration CurrentLibrary {
			get {  return( mCurrentLibrary ); }
			set {
				mCurrentLibrary = value;

				RaisePropertyChanged( () => CurrentLibrary );
			}
		}

		public string ExportPath {
			get {  return( Get( () => ExportPath )); }
			set {  Set( () => ExportPath, value ); }
		}

		public void Execute_ExportLibrary() {
			if(( mCurrentLibrary != null ) &&
			   ( Directory.Exists( ExportPath ))) {
				mLibrarian.ExportLibrary( mCurrentLibrary, ExportPath );
			}
		}

		[DependsUpon( "CurrentLibrary" )]
		[DependsUpon( "ExportPath" )]
		public bool CanExecute_ExportLibrary() {
			return(( mCurrentLibrary != null ) &&
				   ( Directory.Exists( ExportPath )));
		}
	}
}
