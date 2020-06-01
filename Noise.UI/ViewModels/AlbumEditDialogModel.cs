using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Logging;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class UiEditChild : PropertyChangeBase { }

    [DebuggerDisplay("Track = {" + nameof( Name ) + "}")]
    public class UiTrackEdit : UiEditChild {
		public  DbTrack				Track { get; }

		public	string				Name { get; set; }
		public	string				TrackIndex => $"{Track.TrackNumber:D2} -";
		public	bool				WasEdited => !Track.Name.Equals( Name );

		public UiTrackEdit( DbTrack track ) {
			Track = track;

			Name = Track.Name;
        }
    }

	public class UiVolumeBase : UiEditChild {
		public	string				VolumeKey {  get; }

		protected UiVolumeBase( string volumeKey ) {
			VolumeKey = volumeKey;
        }
	}

    [DebuggerDisplay("Volume = {" + nameof( Name ) + "}")]
    public class UiVolumeEdit : UiVolumeBase {
		private readonly Action<UiVolumeEdit>		mDeleteVolume;
		private readonly Action<UiVolumeEdit>		mChangeVolumeName;
		private string								mVolumeName;

		public	ObservableCollection<UiTrackEdit>	Tracks { get; }
		public	DelegateCommand						DeleteVolume { get; }

		public UiVolumeEdit( string key, string volumeName, IEnumerable<UiTrackEdit> tracks, Action<UiVolumeEdit> deleteVolume, Action<UiVolumeEdit> changeVolumeName ) :
            base( key ) {
			Tracks = new ObservableCollection<UiTrackEdit>( tracks );
			mDeleteVolume = deleteVolume;
			mChangeVolumeName = changeVolumeName;
			mVolumeName = volumeName;

			DeleteVolume = new DelegateCommand( OnDeleteVolume );
        }

        public string Name {
			get => mVolumeName;
			set {
				mVolumeName = value;

				mChangeVolumeName?.Invoke( this );
            }
        }

		private void OnDeleteVolume() {
			mDeleteVolume?.Invoke( this );
        }
    }

    [DebuggerDisplay("Album = {" + nameof( Name ) + "}")]
	public class UiAlbumEdit : UiVolumeBase {
		public	DbAlbum								Album { get; }

		public	ObservableCollection<UiEditChild>	Children { get; }
		public	string								Name { get; set; }
		public	bool								WasEdited => !Album.Name.Equals( Name );

		public UiAlbumEdit( DbAlbum album ) :
			base( String.Empty) {
			Album = album;

			Name = Album.Name;
			Children = new ObservableCollection<UiEditChild>();
		}
    }

	internal class VolumeInfo {
		public	string			VolumeName { get; set; }
		public	List<DbTrack>	Tracks { get; }

		public VolumeInfo( string volumeName ) {
			Tracks = new List<DbTrack>();

			VolumeName = volumeName;
		}

		public VolumeInfo( string volumeName, DbTrack track ) :
			this( volumeName ) {
			Tracks.Add( track );
        }
    }

	public class AlbumEditDialogModel : PropertyChangeBase, IDialogAware, IDropTarget, IDataErrorInfo {
		public	const string							cAlbumIdParameter = "albumId";

		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;
		private readonly IUiLog							mLog;
		private readonly IEventAggregator				mEventAggregator;
		private readonly List<DbTrack>					mTrackList;
		private readonly Dictionary<string, VolumeInfo>	mVolumeAssociations;
		private long									mAlbumId;
		private TaskHandler								mDatabaseTaskHandler;
		private string									mNewVolumeName;

        public  DbAlbum									Album { get; private set; }
		public	ObservableCollection<UiEditChild>		EditList { get; }
        public	string									PublishedDate { get; set; }

        public  string									Title { get; }
        public  DelegateCommand							Ok { get; }
        public  DelegateCommand							Cancel { get; }
		public	DelegateCommand							CreateVolume { get; }
        public  event Action<IDialogResult>				RequestClose;

		public AlbumEditDialogModel( IAlbumProvider albumProvider, ITrackProvider trackProvider, IUiLog log, IEventAggregator eventAggregator ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mLog = log;
			mEventAggregator = eventAggregator;

			mTrackList = new List<DbTrack>();
			mVolumeAssociations = new Dictionary<string, VolumeInfo>();
			EditList = new ObservableCollection<UiEditChild>();
			mNewVolumeName = String.Empty;
			PublishedDate = String.Empty;

			Title = "Album Editor";

			Ok = new DelegateCommand( OnOk );
			Cancel = new DelegateCommand( OnCancel );
			CreateVolume = new DelegateCommand( OnCreateVolume, CanCreateVolume );
		}

        public void OnDialogOpened( IDialogParameters parameters ) {
			mAlbumId = parameters.GetValue<long>( cAlbumIdParameter );

			if( mAlbumId != Constants.cDatabaseNullOid ) {
                LoadData( mAlbumId );
            }
        }

        internal TaskHandler DatabaseTaskHandler {
            get {
                if( mDatabaseTaskHandler == null ) {
                    Execute.OnUIThread( () => mDatabaseTaskHandler = new TaskHandler());
                }

                return( mDatabaseTaskHandler );
            }

            set => mDatabaseTaskHandler = value;
        }

        public string NewVolumeName {
			get => mNewVolumeName;
			set {
				mNewVolumeName = value;

				RaisePropertyChanged( () => NewVolumeName );
				CreateVolume.RaiseCanExecuteChanged();
            }
        }

        private void LoadData( long albumId ) {
			mTrackList.Clear();

			DatabaseTaskHandler.StartTask( () => {
				    Album = mAlbumProvider.GetAlbum( albumId );

					if( Album != null ) {
					    using( var list = mTrackProvider.GetTrackList( Album )) {
							mTrackList.AddRange( list.List );
                        }
				    }
                }, 
                () => {
					BuildVolumeList();
                    BuildUiList();

					if( Album.PublishedYear > Constants.cVariousYears  ) {
                        PublishedDate = Album.PublishedYear.ToString();

                        RaisePropertyChanged( () => PublishedDate );
                    }
                }, 
                ex => mLog.LogException( "AlbumEditDialog:LoadData", ex ));
        }

		private void BuildVolumeList() {
			foreach( var track in mTrackList ) {
				if( mVolumeAssociations.ContainsKey( track.VolumeName )) {
					mVolumeAssociations[track.VolumeName].Tracks.Add( track );
				}
				else {
					mVolumeAssociations.Add( track.VolumeName, new VolumeInfo( track.VolumeName, track ));
                }
			}

			if(!mVolumeAssociations.ContainsKey( String.Empty )) {
				mVolumeAssociations.Add( String.Empty, new VolumeInfo( String.Empty ));
            }
        }

		private void BuildUiList() {
			EditList.Clear();

			if(( Album != null ) &&
               ( mTrackList.Any())) {
			    var album = new UiAlbumEdit( Album );
                var volumeList = new List<UiVolumeEdit>();

				mVolumeAssociations.ForEach( volume => {
					if( String.IsNullOrWhiteSpace( volume.Key )) {
						album.Children.AddRange( from t in volume.Value.Tracks orderby t.TrackNumber select new UiTrackEdit( t ));
                    }
					else {
                        volumeList.Add( new UiVolumeEdit( volume.Key, volume.Value.VolumeName,
                                                          from t in volume.Value.Tracks orderby t.TrackNumber select new UiTrackEdit( t ), 
                                                          OnDeleteVolume, OnChangeVolumeName ));
                    }
                });

				album.Children.AddRange( from v in volumeList orderby v.Name select v );
				EditList.Add( album );
			}
        }

		private void OnChangeVolumeName( UiVolumeEdit volume ) {
			if( mVolumeAssociations.ContainsKey( volume.VolumeKey )) {
				mVolumeAssociations[volume.VolumeKey].VolumeName = volume.Name;
            }
        }

		private void OnDeleteVolume( UiVolumeEdit volume ) {
			if( mVolumeAssociations.ContainsKey( volume.Name )) {
				var albumTracks = mVolumeAssociations[String.Empty].Tracks;

                albumTracks?.AddRange( mVolumeAssociations[volume.Name].Tracks );
				mVolumeAssociations.Remove( volume.Name );
            }

			BuildUiList();
		}

		private bool UpdateData() {
			var retValue = false;

            if( EditList.FirstOrDefault( i => i is UiAlbumEdit ) is UiAlbumEdit album ) {
				var volumes = album.Children.Where( i => i is UiVolumeEdit );

				foreach( var v in volumes ) {
					if( v is UiVolumeEdit volume ) {
						retValue |= UpdateTracks( volume.Tracks, volume.Name );
					}
                }

				retValue |= UpdateTracks( from c in album.Children where c is UiTrackEdit select c as UiTrackEdit, String.Empty );

                retValue |= UpdateAlbum( album );
            }

			mEventAggregator.PublishOnUIThread( new Events.LibraryBackupPressure( 3, "AlbumEditDialog" ));

			return retValue;
        }

		private bool UpdateAlbum( UiAlbumEdit albumEdit ) {
			var retValue = false;

			if(( albumEdit.WasEdited ) ||
			   (!albumEdit.Album.PublishedYear.ToString().Equals( PublishedDate ))) {
                using( var updater = mAlbumProvider.GetAlbumForUpdate( albumEdit.Album.DbId )) {
                    updater.Item.Name = albumEdit.Name;

                    if( Int32.TryParse( PublishedDate, out var date )) {
                        updater.Item.PublishedYear = date;
                    }

                    updater.Update();
					retValue = true;
                }

                mEventAggregator.PublishOnUIThread( new Events.AlbumUserUpdate( albumEdit.Album.DbId ));
            }

			return retValue;
        }

		private bool UpdateTracks( IEnumerable<UiTrackEdit> tracks, string volumeName ) {
			var retValue = false;

			tracks.ForEach( track => {
				if(( track.WasEdited ) ||
				   (!track.Track.VolumeName.Equals( volumeName ))) {
                    using( var updater = mTrackProvider.GetTrackForUpdate( track.Track.DbId )) {
                        updater.Item.Name = track.Name;
						updater.Item.VolumeName = volumeName;

						updater.Update();

						retValue = true;
                    }

                    mEventAggregator.PublishOnUIThread( new Events.TrackUserUpdate( mTrackProvider.GetTrack( track.Track.DbId )));
                }
            });

			return retValue;
        }

		private void OnCreateVolume() {
			if((!String.IsNullOrWhiteSpace( NewVolumeName )) &&
               (!mVolumeAssociations.ContainsKey( NewVolumeName ))) {
				mVolumeAssociations.Add( NewVolumeName, new VolumeInfo( NewVolumeName ));

				BuildUiList();
				CreateVolume.RaiseCanExecuteChanged();
			}
        }

		private bool CanCreateVolume() {
			return !String.IsNullOrWhiteSpace( NewVolumeName ) && !mVolumeAssociations.ContainsKey( NewVolumeName );
        }

        public void DragOver( DropInfo dropInfo ) {
			if(( dropInfo.Data is UiTrackEdit ) &&
			   ( dropInfo.TargetItem is UiVolumeEdit || dropInfo.TargetItem is UiAlbumEdit )) {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop( DropInfo dropInfo ) {
			if(( dropInfo.Data is UiTrackEdit track ) &&
			   ( dropInfo.TargetItem is UiVolumeBase volume )) {
                var sourceKey = FindVolumeOfTrack( track );
				var dbTrack = mVolumeAssociations[sourceKey].Tracks.FirstOrDefault( t => t.DbId.Equals( track.Track.DbId ));

				if( dbTrack != null ) {
					mVolumeAssociations[sourceKey].Tracks.Remove( dbTrack );
                }

				if( mVolumeAssociations.ContainsKey( volume.VolumeKey )) {
					mVolumeAssociations[volume.VolumeKey].Tracks.Add( dbTrack );
                }

				BuildUiList();
            }
        }

		private string FindVolumeOfTrack( UiTrackEdit track ) {
			var retValue = string.Empty;

			if( track != null ) {
                foreach( var key in mVolumeAssociations ) {
                    if( key.Value.Tracks.FirstOrDefault( i => i.DbId == track.Track.DbId) != null ) {
                        retValue = key.Key;

                        break;
                    }
                }
			}

			return retValue;
        }

        public string this[ string columnName ] {
			get {
				var retValue = String.Empty;

                switch( columnName ) {
                    case nameof( PublishedDate ):
						if(!String.IsNullOrWhiteSpace( PublishedDate )) {
							if( Int32.TryParse( PublishedDate, out var date )) {
								if(( date != Constants.cUnknownYear ) &&
								   ( date != Constants.cVariousYears )) {
									if(( date <= 1900 ) ||
                                       ( date >= 2100 )) {
                                        retValue = "Date out of range.";
									}
                                }
                            }
							else {
								retValue = "Date has invalid characters.";
                            }
                        }
						break;
                }

				return retValue;
            }
        }

        public string Error => null;

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
			if( UpdateData()) {
				var parameters = new DialogParameters{{ cAlbumIdParameter, mAlbumId }};

                RaiseRequestClose( new DialogResult( ButtonResult.OK, parameters ));
            }
			else {
				RaiseRequestClose( new DialogResult( ButtonResult.Abort ));
            }
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
