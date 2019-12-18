using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
	public class UiEditChild : AutomaticCommandBase { }

    public class UiTrackEdit : UiEditChild {
		public  DbTrack				Track { get; }

		public	string				Name {  get; set; }
		public	string				TrackIndex => $"{Track.TrackNumber:D2} -";
		public	bool				WasEdited => !Track.Name.Equals( Name );

		public UiTrackEdit( DbTrack track ) {
			Track = track;

			Name = Track.Name;
        }
    }

    public class UiVolumeEdit : UiEditChild {
		private readonly string						mVolumeName;
		private readonly Action<UiVolumeEdit>		mDeleteVolume;

		public	ObservableCollection<UiTrackEdit>	Tracks { get; }
		public	string								Name { get; set; }

		public UiVolumeEdit( string volumeName, IEnumerable<UiTrackEdit> tracks, Action<UiVolumeEdit> deleteVolume ) {
			Tracks = new ObservableCollection<UiTrackEdit>( tracks );
			mDeleteVolume = deleteVolume;

			mVolumeName = volumeName;
			Name = volumeName;
        }

		public void Execute_DeleteVolume() {
			mDeleteVolume?.Invoke( this );
        }
    }

	public class UiAlbumEdit : UiEditChild {
		public	DbAlbum								Album { get; }

		public	ObservableCollection<UiEditChild>	Children { get; }
		public	string								Name { get; set; }
		public	bool								WasEdited => !Album.Name.Equals( Name );

		public UiAlbumEdit( DbAlbum album ) {
			Album = album;

			Name = Album.Name;
			Children = new ObservableCollection<UiEditChild>();
		}
    }

	public class AlbumEditDialogModel : DialogModelBase {
		private readonly IAlbumProvider					mAlbumProvider;
		private readonly ITrackProvider					mTrackProvider;
		private readonly IUiLog							mLog;
		private readonly IEventAggregator				mEventAggregator;
		private readonly List<DbTrack>					mTrackList;
		private readonly Dictionary<string, List<DbTrack>>	mVolumeAssociations;
		private TaskHandler								mDatabaseTaskHandler;

        public  DbAlbum									Album { get; private set; }
		public	ObservableCollection<UiEditChild>		EditList { get; }
        public	string									PublishedDate => Album.PublishedYear.ToString();

		public AlbumEditDialogModel( IAlbumProvider albumProvider, ITrackProvider trackProvider, IUiLog log, IEventAggregator eventAggregator, long albumId ) {
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
			mLog = log;
			mEventAggregator = eventAggregator;

			mTrackList = new List<DbTrack>();
			mVolumeAssociations = new Dictionary<string, List<DbTrack>>();
			EditList = new ObservableCollection<UiEditChild>();

			LoadData( albumId );
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
                }, 
                ex => mLog.LogException( "AlbumEditDialog:LoadData", ex ));
        }

		private void BuildVolumeList() {
			foreach( var track in mTrackList ) {
				if( mVolumeAssociations.ContainsKey( track.VolumeName )) {
					mVolumeAssociations[track.VolumeName].Add( track );
				}
				else {
					mVolumeAssociations.Add( track.VolumeName, new List<DbTrack>{ track });
                }
			}
        }

		private void BuildUiList() {
			EditList.Clear();

			if(( Album != null ) &&
               ( mTrackList.Any())) {
			    var album = new UiAlbumEdit( Album );

				mVolumeAssociations.ForEach( volume => {
					if( String.IsNullOrWhiteSpace( volume.Key )) {
						album.Children.AddRange( from t in volume.Value orderby t.TrackNumber select new UiTrackEdit( t ));
                    }
					else {
                        album.Children.Add( new UiVolumeEdit( volume.Key, from t in volume.Value orderby t.TrackNumber select new UiTrackEdit( t ), OnDeleteVolume ));
                    }
                });

				EditList.Add( album );
			}
        }

		private void OnDeleteVolume( UiVolumeEdit volume ) {
			if( mVolumeAssociations.ContainsKey( volume.Name )) {
				if(!mVolumeAssociations.ContainsKey( String.Empty )) {
					mVolumeAssociations.Add( String.Empty, new List<DbTrack>());
				}

				var albumTracks = mVolumeAssociations[String.Empty];

                albumTracks?.AddRange( mVolumeAssociations[volume.Name]);
				mVolumeAssociations.Remove( volume.Name );
            }

			BuildUiList();
		}

		public bool UpdateData() {
			var retValue = false;

            if( EditList.FirstOrDefault( i => i is UiAlbumEdit ) is UiAlbumEdit album ) {
				var volumes = album.Children.Where( i => i is UiVolumeEdit );

				foreach( var v in volumes ) {
					if( v is UiVolumeEdit volume ) {
						UpdateTracks( volume.Tracks, volume.Name );
					}
                }

				UpdateTracks( from c in album.Children where c is UiTrackEdit select c as UiTrackEdit, String.Empty );

                retValue |= UpdateAlbum( album );
            }

			return retValue;
        }

		private bool UpdateAlbum( UiAlbumEdit albumEdit ) {
			var retValue = false;

			if( albumEdit.WasEdited ) {
                using( var updater = mAlbumProvider.GetAlbumForUpdate( albumEdit.Album.DbId )) {
                    updater.Item.Name = albumEdit.Name;

                    updater.Update();
					retValue = true;
                }

                mEventAggregator.PublishOnUIThread( new Events.AlbumUserUpdate( albumEdit.Album.DbId ));
            }

			return retValue;
        }

		private void UpdateTracks( IEnumerable<UiTrackEdit> tracks, string volumeName ) {
			tracks.ForEach( track => {
				if( track.WasEdited ) {
                    using( var updater = mTrackProvider.GetTrackForUpdate( track.Track.DbId )) {
                        updater.Item.Name = track.Name;
						updater.Item.VolumeName = volumeName;

						updater.UpdateTrackAndAlbum();
                    }

                    mEventAggregator.PublishOnUIThread( new Events.TrackUserUpdate( mTrackProvider.GetTrack( track.Track.DbId )));
                }
            });
        }
	}
}
