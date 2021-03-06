﻿using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Models;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.Librarian.ViewModels {
	public class BackupDatabaseViewModel : AutomaticCommandBase,
										   IHandle<Events.SystemInitialized>, IHandle<Events.LibraryListChanged> {
		private readonly IEventAggregator							mEventAggregator;
		private readonly ILibraryConfiguration						mLibraryConfiguration;
		private readonly ILibrarian									mLibrarian;
		private readonly BindableCollection<LibraryConfiguration>	mLibraries; 
		private LibraryConfiguration								mCurrentLibrary;

		public BackupDatabaseViewModel( IEventAggregator eventAggregator, ILibrarian librarian, ILibraryConfiguration libraryConfiguration ) {
			mEventAggregator = eventAggregator;
			mLibrarian = librarian;
			mLibraryConfiguration = libraryConfiguration;

			mLibraries = new BindableCollection<LibraryConfiguration>();
			ProgressActive = false;

			mEventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized message ) {
			LoadLibraries();
		}

		public void Handle( Events.LibraryListChanged args ) {
			LoadLibraries();
		}

		private void LoadLibraries() {
			mLibraries.Clear();
			mLibraries.AddRange( mLibraryConfiguration.Libraries );

			CurrentLibrary = mLibraries.FirstOrDefault();
			CanEdit = mLibraries.Any();
		}

		public BindableCollection<LibraryConfiguration> LibraryList => mLibraries;

        public LibraryConfiguration CurrentLibrary {
			get => ( mCurrentLibrary );
            set {
				mCurrentLibrary = value;

				RaisePropertyChanged( () => CurrentLibrary );
			}
		}

		public void Execute_BackupLibrary() {
			if( mCurrentLibrary != null ) {
				mEventAggregator.PublishOnUIThread( new ProgressEvent( 0.0, true ));

				mLibrarian.BackupLibrary( mCurrentLibrary, OnBackupProgress );
			}
		}

		[DependsUpon( "CurrentLibrary" )]
		[DependsUpon( "ProgressActive" )]
		public bool CanExecute_BackupLibrary() {
			return(( mCurrentLibrary != null ) &&
				   (!ProgressActive ));
		}

		private void OnBackupProgress( LibrarianProgressReport args ) {
			ProgressPhase = args.CurrentPhase;
			ProgressItem = args.CurrentItem;
			Progress = args.Progress;
			ProgressActive = !args.Completed;

			mEventAggregator.PublishOnUIThread( new ProgressEvent( (double)args.Progress / 1000, !args.Completed ));
		}

		public string ProgressPhase {
			get {  return( Get( () => ProgressPhase )); }
			set {  Set( () => ProgressPhase, value ); }
		}

		public string ProgressItem {
			get {  return( Get( () => ProgressItem )); }
			set {  Set( () => ProgressItem, value ); }
		}

		public int Progress {
			get {  return( Get( () => Progress )); }
			set {  Set( () => Progress, value ); }
		}

		public bool ProgressActive {
			get {  return( Get( () => ProgressActive )); }
			set {
				Set( () => ProgressActive, value );
				CanEdit = !ProgressActive;
			}
		}

		public bool CanEdit {
			get {  return( Get( () => CanEdit )); }
			set {  Set( () => CanEdit, value ); }
		}
	}
}
