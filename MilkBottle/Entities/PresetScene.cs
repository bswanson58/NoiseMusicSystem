using System;
using System.Diagnostics;
using LiteDB;

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
            SourceId = ObjectId.Empty;
            PresetCycle = PresetCycling.CountPerScene;
            PresetDuration = 1;
            OverlapPresets = false;
            OverlapDuration = 0;
        }

        public PresetScene( ObjectId id, string name, SceneSource sceneSource, ObjectId sourceId, PresetCycling presetCycle, int presetDuration,
            bool overlapPresets, int overlapDuration ) :
            base( id ) {
            Name = name;
            SceneSource = sceneSource;
            SourceId = sourceId;
            PresetCycle = presetCycle;
            PresetDuration = presetDuration;
            OverlapPresets = overlapPresets;
            OverlapDuration = overlapDuration;
        }

        [BsonCtor]
        public PresetScene( ObjectId id, string name, int sceneSource, ObjectId sourceId, int presetCycle, int presetDuration,
                            bool overlapPresets, int overlapDuration ) :
                base( id ) {
            Name = name;
            SceneSource = (SceneSource)sceneSource;
            SourceId = sourceId;
            PresetCycle = (PresetCycling)presetCycle;
            PresetDuration = presetDuration;
            OverlapPresets = overlapPresets;
            OverlapDuration = overlapDuration;
        }

        public PresetScene WithName( string name ) {
            return new PresetScene( Id, name, SceneSource, SourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration );
        }

        public PresetScene WithSource( SceneSource source, ObjectId sourceId ) {
            return new PresetScene( Id, Name, source, sourceId, PresetCycle, PresetDuration, OverlapPresets, OverlapDuration );
        }

        public PresetScene WithCycle( PresetCycling cycling, int duration ) {
            return new PresetScene( Id, Name, SceneSource, SourceId, cycling, duration, OverlapPresets, OverlapDuration );
        }

        public PresetScene WithOverlap( bool overlap, int overlapDuration ) {
            return new PresetScene( Id, Name, SceneSource, SourceId, PresetCycle, PresetDuration, overlap, overlapDuration );
        }
    }
}
