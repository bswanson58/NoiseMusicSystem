﻿using System.Collections.Generic;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	public class ContextType {
		public string	ContextName { get; private set; }
		public bool		IsAlbumContext { get; private set; }

		public ContextType( string name, bool isAlbumContext ) {
			ContextName = name;
			IsAlbumContext = isAlbumContext;
		}
	}

	public class PlaybackContextDialogManager : DialogModelBase {
		private readonly IAudioController		mAudioController;
		private readonly IPlaybackContextWriter	mContextWriter;
		private DbAlbum							mCurentAlbum;
		private	DbTrack							mCurrentTrack;
		private ScPlayContext					mTrackContext;
		private ScPlayContext					mAlbumContext;
		private bool							mIsAlbumContext;
		private readonly List<ContextType>		mContextTypes;
		private readonly ContextType			mAlbumContextType;
		private readonly ContextType			mTrackContextType;
		private ContextType						mCurrentContext;

		public PlaybackContextDialogManager( IAudioController audioController, IPlaybackContextWriter contextWriter ) {
			mAudioController = audioController;
			mContextWriter = contextWriter;

			mTrackContextType = new ContextType( "Track", false );
			mAlbumContextType = new ContextType( "Album", true );
			mContextTypes = new List<ContextType>{ mAlbumContextType, mTrackContextType };
		}

		public void SetTrack( DbAlbum album, DbTrack track ) {
			mCurentAlbum = album;
			mCurrentTrack = track;

			mTrackContext = mContextWriter.GetTrackContext( mCurrentTrack ) ?? new ScPlayContext();
			mAlbumContext = mContextWriter.GetAlbumContext( mCurrentTrack ) ?? new ScPlayContext();

			CurrentContext = mTrackContext.HasContext ? mTrackContextType : mAlbumContextType;
		}

		public ScPlayContext PlaybackContext {
			get { return( mIsAlbumContext ? mAlbumContext : mTrackContext ); }
		}

		public void UpdatePlaybackContext() {
			if( mIsAlbumContext ) {
				mAlbumContext.PanPosition = mAlbumContext.PanPositionValid ? mAudioController.PanPosition : 0.0;
				mAlbumContext.PlaySpeed = mAlbumContext.PlaySpeedValid ? mAudioController.PlaySpeed : 0.0;
				mAlbumContext.PreampVolume = mAlbumContext.PreampVolumeValid ? mAudioController.PreampVolume : 0.0;

				mContextWriter.SaveAlbumContext( mCurrentTrack, mAlbumContext );
			}
			else {
				mTrackContext.PanPosition = mTrackContext.PanPositionValid ? mAudioController.PanPosition : 0.0;
				mTrackContext.PlaySpeed = mTrackContext.PlaySpeedValid ? mAudioController.PlaySpeed : 0.0;
				mTrackContext.PreampVolume = mTrackContext.PreampVolumeValid ? mAudioController.PreampVolume : 0.0;

				mContextWriter.SaveTrackContext( mCurrentTrack, mTrackContext );
			}
		}

		public IList<ContextType> ContextTypes {
			get { return(mContextTypes); }
		}

		public ContextType CurrentContext {
			get { return( mCurrentContext ); }
			set {
				mCurrentContext = value;
				mIsAlbumContext = mCurrentContext.IsAlbumContext;
				
				RaisePropertyChanged( () => CurrentContext );
				RaisePropertyChanged( () => PlaybackContext );
				RaisePropertyChanged( () => ContextDescription );
			}
		}

		public string ContextDescription {
			get {
				return( mIsAlbumContext ? mCurentAlbum.Name : mCurrentTrack.Name );
			}
		}
	}
}
