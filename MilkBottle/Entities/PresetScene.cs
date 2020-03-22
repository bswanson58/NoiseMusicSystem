using System;
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
        public  string              Name { get; }
        public SceneSource          SceneSource { get; }
        public PresetListType       SourceListType { get; }
        public ObjectId             SourceId { get; }
        public PresetCycling        PresetCycle { get; }
        public int                  PresetDuration { get; }
        public bool                 OverlapPresets { get; }
        public int                  OverlapDuration {  get; }

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
        }

        public PresetScene( ObjectId id, string name, SceneSource sceneSource, PresetListType listType, ObjectId sourceId, PresetCycling presetCycle, int presetDuration,
            bool overlapPresets, int overlapDuration ) :
            base( id ) {
            Name = name;
            SceneSource = sceneSource;
            SourceListType = listType;
            SourceId = sourceId;
            PresetCycle = presetCycle;
            PresetDuration = presetDuration;
            OverlapPresets = overlapPresets;
            OverlapDuration = overlapDuration;
        }

        [BsonCtor]
        public PresetScene( ObjectId id, string name, int sceneSource, int sourceListType, ObjectId sourceId, int presetCycle, int presetDuration,
                            bool overlapPresets, int overlapDuration ) :
                base( id ) {
            Name = name;
            SceneSource = (SceneSource)sceneSource;
            SourceListType = (PresetListType)sourceListType;
            SourceId = sourceId;
            PresetCycle = (PresetCycling)presetCycle;
            PresetDuration = presetDuration;
            OverlapPresets = overlapPresets;
            OverlapDuration = overlapDuration;
        }

        public PresetScene WithName( string name ) {
            return new PresetScene( Id, name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration );
        }

        public PresetScene WithSource( SceneSource source, PresetListType sourceListType, ObjectId sourceId ) {
            return new PresetScene( Id, Name, source, sourceListType, sourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration );
        }

        public PresetScene WithCycle( PresetCycling cycling, int duration ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, cycling, duration, OverlapPresets, OverlapDuration );
        }

        public PresetScene WithOverlap( bool overlap, int overlapDuration ) {
            return new PresetScene( Id, Name, SceneSource, SourceListType, SourceId, PresetCycle, PresetDuration, overlap, overlapDuration );
        }
    }
}
