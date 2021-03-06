﻿using Caliburn.Micro;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Noise.Infrastructure;
using Noise.UI.Dto;
using Noise.UI.Logging;
using ReusableBits;

namespace Noise.UI.ViewModels {
    class TagAssociationViewModel : PropertyChangedBase, IHandle<Events.UserTagsChanged> {
        private readonly IEventAggregator       mEventAggregator;
        private readonly IUiLog                 mLog;
        private readonly IUserTagManager        mTagManager;
        private readonly IArtistProvider        mArtistProvider;
        private readonly IAlbumProvider         mAlbumProvider;
        private readonly ITrackProvider         mTrackProvider;
        private readonly IPlayCommand           mPlayCommand;
        private readonly ISelectionState        mSelectionState;
        private readonly IPlayingItemHandler    mPlayingItemHandler;
        private DbTag                           mCurrentTag;
        private UiTagAssociation                mCurrentAssociation;
        private TaskHandler<IEnumerable<UiTagAssociation>>  mRetrievalTask;

        public  ObservableCollection<UiTagAssociation>  Associations { get; }
        public  string                                  HeaderText => Associations.Any() ? $" {Associations.Count} tracks tagged with '{mCurrentTag.Name}'" : " Tagged Items ";

        public TagAssociationViewModel( IUserTagManager tagManager, IEventAggregator eventAggregator, ISelectionState selectionState, IPlayCommand playCommand,
                                        IArtistProvider artistProvider, IAlbumProvider albumProvider, ITrackProvider trackProvider, IPlayingItemHandler playingItemHandler, IUiLog log ) {
            mEventAggregator = eventAggregator;
            mTagManager = tagManager;
            mSelectionState = selectionState;
            mArtistProvider = artistProvider;
            mAlbumProvider = albumProvider;
            mTrackProvider = trackProvider;
            mPlayCommand = playCommand;
            mPlayingItemHandler = playingItemHandler;
            mLog = log;

            Associations = new ObservableCollection<UiTagAssociation>();

            mSelectionState.CurrentTagChanged.Subscribe( OnTagChanged );
            mPlayingItemHandler.StartHandler( Associations );
            mEventAggregator.Subscribe( this );
        }

        private void OnTagChanged( DbTag tag ) {
            mCurrentTag = tag;

            UpdateAssociations();
        }

        public void Handle( Events.UserTagsChanged args ) {
            UpdateAssociations();
        }

        public UiTagAssociation SelectedAssociation {
            get => mCurrentAssociation;
            set {
                mCurrentAssociation = value;

                OnAssociationSelected();
            }
        }

        private void OnAssociationSelected() {
            if( mCurrentAssociation != null ) {
                mEventAggregator.PublishOnUIThread( new Events.AlbumFocusRequested( mCurrentAssociation.Artist.DbId, mCurrentAssociation.Album.DbId ));
            }
        }

        private void UpdateAssociations() {
            Associations.Clear();

            RetrievalTaskHandler.StartTask( LoadAssociations, SetAssociations, ex => mLog.LogException( "Loading Tag Associations", ex ));
        }

        internal TaskHandler<IEnumerable<UiTagAssociation>> RetrievalTaskHandler {
            get {
                if( mRetrievalTask == null ) {
                    Execute.OnUIThread( () => mRetrievalTask = new TaskHandler<IEnumerable<UiTagAssociation>>());
                }

                return ( mRetrievalTask );
            }
            set => mRetrievalTask = value;
        }

        private IEnumerable<UiTagAssociation> LoadAssociations() {
            var retValue = new List<UiTagAssociation>();

            if( mCurrentTag != null ) {
                var associations = mTagManager.GetAssociations( mCurrentTag.DbId );

                foreach( var association in associations ) {
                    var track = mTrackProvider.GetTrack( association.ArtistId );
                    var artist = mArtistProvider.GetArtist( track.Artist );
                    var album = mAlbumProvider.GetAlbum( track.Album );

                    retValue.Add( new UiTagAssociation( association, artist, album, track, OnAssociationPlay, OnAssociationDelete ));
                }
            }

            return retValue;
        }

        private void SetAssociations( IEnumerable<UiTagAssociation> list ) {
            Associations.AddRange( from a in list orderby a.Track.Name select a );
            mPlayingItemHandler.UpdateList();

            NotifyOfPropertyChange( () => HeaderText );
        }

        private void OnAssociationPlay( UiTagAssociation tag ) {
            mPlayCommand.Play( tag.Track );
        }

        private void OnAssociationDelete( UiTagAssociation tag ) {
            mTagManager.DeleteAssociation( tag.Association );
        }
    }
}
