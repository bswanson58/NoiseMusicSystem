using System;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Library: {" + nameof( Name ) + "}")]
    class PresetLibrary : EntityBase {
        public  ObjectId    Parent { get; }
        public  string      Name { get; }
        public  string      Location { get; }

        public PresetLibrary( string name, string location, PresetLibrary parent ) :
            this( ObjectId.NewObjectId(), name, location, parent.Id ) {
            Name = ( name ?? String.Empty ).Trim();
            Location = ( location ?? String.Empty ).Trim();
            Parent = parent.Id;
        }

        [BsonCtorAttribute]
        public PresetLibrary( ObjectId id, string name, string location, ObjectId parent ) :
            base( id ) {
            Name = ( name ?? String.Empty ).Trim();
            Location = ( location ?? String.Empty ).Trim();
            Parent = parent;
        }

        public static PresetLibrary Default() {
            return new PresetLibrary( ObjectId.Empty, String.Empty, String.Empty, ObjectId.Empty );
        }

        public static PresetLibrary RootLibrary( string location ) {
            return new PresetLibrary( ObjectId.Empty, "Root Library", location, ObjectId.Empty );
        }
    }
}
