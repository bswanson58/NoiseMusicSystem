using System;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Library: {" + nameof( Name ) + "}")]
    class PresetLibrary : EntityBase {
        public  string  Name { get; }
        public  string  Location { get; }

        public PresetLibrary( string name, string location ) :
            this( ObjectId.NewObjectId(), name, location ) {
            Name = name ?? String.Empty;
            Location = location ?? String.Empty;
        }

        [BsonCtorAttribute]
        public PresetLibrary( ObjectId id, string name, string location ) :
            base( id ) {
            Name = name ?? String.Empty;
            Location = location ?? String.Empty;
        }

        public static PresetLibrary Default() {
            return new PresetLibrary( String.Empty, String.Empty );
        }
    }
}
