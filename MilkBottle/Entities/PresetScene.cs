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

        [BsonCtor]
        public PresetScene( ObjectId sceneId, string name, SceneSource source, ObjectId sourceId, PresetCycling cycling, int presetDuration,
                            bool overlapPresets, int overlapDuration ) :
                base( sceneId ) {
            Name = name;
            SceneSource = source;
            SourceId = sourceId;
            PresetCycle = cycling;
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
