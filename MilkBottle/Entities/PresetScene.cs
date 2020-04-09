using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiteDB;
using MilkBottle.Dto;

namespace MilkBottle.Entities {
    enum SceneSource {
        SinglePreset = 1,
        PresetList = 2
    }

    enum PresetCycling {
        CountPerScene = 1,
        Duration = 2
    }

    [DebuggerDisplay("{" + nameof( DebugDisplay ) + "}")]
    class PresetScene : EntityBase {
        public  const char          cValueSeparator = '|';

        public  string              Name { get; }
        public  SceneSource         SceneSource { get; }
        public  PresetListType      SourceListType { get; }
        public  ObjectId            SourceId { get; }
        public  PresetCycling       PresetCycle { get; }
        public  int                 PresetDuration { get; }
        public  bool                OverlapPresets { get; }
        public  int                 OverlapDuration {  get; }
        public  string              ArtistNames { get; }
        public  string              AlbumNames { get; }
        public  string              TrackNames { get; }
        public  string              Genres { get; }
        public  string              Tags { get; }
        public  string              Years { get; }
        public  string              Hours { get; }
        public  bool                IsFavoriteTrack { get; }
        public  bool                IsFavoriteAlbum { get; }
        public  bool                IsFavoriteArtist { get; }
        public  List<Mood>          Moods { get; }

        public  string              DebugDisplay => $"Scene: {Name}";

        public PresetScene( string name ) :
            base( ObjectId.NewObjectId()) {
            Name = name ?? String.Empty;
            SceneSource = SceneSource.SinglePreset;
            SourceListType = PresetListType.Library;
            SourceId = ObjectId.Empty;
            PresetCycle = PresetCycling.CountPerScene;
            PresetDuration = 1;
            OverlapPresets = false;
            OverlapDuration = 0;
            ArtistNames = String.Empty;
            AlbumNames = String.Empty;
            TrackNames = String.Empty;
            Genres = String.Empty;
            Tags = String.Empty;
            Years = String.Empty;
            Hours = String.Empty;
            IsFavoriteArtist = false;
            IsFavoriteAlbum = false;
            IsFavoriteTrack = false;
            Moods = new List<Mood>();
        }

        public PresetScene( ObjectId id, string name, SceneSource sceneSource, PresetListType listType, ObjectId sourceId, PresetCycling presetCycle, int presetDuration,
                            bool overlapPresets, int overlapDuration, string artistNames, string albumNames, string trackNames, string genres, string tags, string years, string hours,
                            bool isFavoriteTrack, bool isFavoriteAlbum, bool isFavoriteArtist, List<Mood> moods ) :
            base( id ) {
            Name = name ?? String.Empty;
            SceneSource = sceneSource;
            SourceListType = listType;
            SourceId = sourceId;
            PresetCycle = presetCycle;
            PresetDuration = presetDuration;
            OverlapPresets = overlapPresets;
            OverlapDuration = overlapDuration;
            ArtistNames = artistNames ?? String.Empty;
            AlbumNames = albumNames ?? String.Empty;
            TrackNames = trackNames ?? String.Empty;
            Genres = genres ?? String.Empty;
            Tags = tags ?? String.Empty;
            Years = years ?? String.Empty;
            Hours = hours ?? String.Empty;
            IsFavoriteTrack = isFavoriteTrack;
            IsFavoriteAlbum = isFavoriteAlbum;
            IsFavoriteArtist = isFavoriteArtist;
            Moods = moods ?? new List<Mood>();
        }

        [BsonCtor]
        public PresetScene( ObjectId id, string name, int sceneSource, int sourceListType, ObjectId sourceId, int presetCycle, int presetDuration, bool overlapPresets,
                            int overlapDuration, string artistNames, string albumNames, string trackNames, string genres, string tags, string years, string hours,
                            bool isFavoriteTrack, bool isFavoriteAlbum, bool isFavoriteArtist, List<Mood> moods ) :
                base( id ) {
            Name = name ?? String.Empty;
            SceneSource = (SceneSource)sceneSource;
            SourceListType = (PresetListType)sourceListType;
            SourceId = sourceId;
            PresetCycle = (PresetCycling)presetCycle;
            PresetDuration = presetDuration;
            OverlapPresets = overlapPresets;
            OverlapDuration = overlapDuration;
            ArtistNames = artistNames ?? String.Empty;
            AlbumNames = albumNames ?? String.Empty;
            TrackNames = trackNames ?? String.Empty;
            Genres = genres ?? String.Empty;
            Tags = tags ?? String.Empty;
            Years = years ?? String.Empty;
            Hours = hours ?? String.Empty;
            IsFavoriteTrack = isFavoriteTrack;
            IsFavoriteAlbum = isFavoriteAlbum;
            IsFavoriteArtist = isFavoriteArtist;
            Moods = moods ?? new List<Mood>();
        }

        public PresetScene WithName( string name ) {
            return new PresetScene( Id, name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                                    ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithSource( SceneSource source, PresetListType sourceListType, ObjectId sourceId ) {
            return new PresetScene( Id, Name, source, sourceListType, sourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                                    ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithCycle( PresetCycling cycling, int duration ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, cycling, duration, OverlapPresets, OverlapDuration,
                                    ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithOverlap( bool overlap, int overlapDuration ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, overlap, overlapDuration,
                                    ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithArtists( string artistNames ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                                    artistNames, AlbumNames, TrackNames, Genres, Tags,Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithAlbums( string albumNames ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, albumNames, TrackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithTracks( string trackNames ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, trackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithGenres( string genreNames ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, TrackNames, genreNames, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithTags( string tags ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, TrackNames, Genres, tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithYears( string years ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, TrackNames, Genres, Tags, years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithHours( string hours ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, Moods );
        }

        public PresetScene WithFavorites( bool isFavoriteTrack, bool isFavoriteAlbum, bool isFavoriteArtist ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, Hours, isFavoriteTrack, isFavoriteAlbum, isFavoriteArtist, Moods );
        }

        public PresetScene WithMoods( List<Mood> moods ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration,
                ArtistNames, AlbumNames, TrackNames, Genres, Tags, Years, Hours, IsFavoriteTrack, IsFavoriteAlbum, IsFavoriteArtist, moods );
        }
    }
}
