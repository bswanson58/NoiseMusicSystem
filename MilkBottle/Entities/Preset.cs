using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Preset: {" + nameof( Name ) + "}")]
    class Preset : EntityBase {
        public  string  Name { get; }
        public  string  Location { get; }

        public Preset( string name, string location ) :
            this( ObjectId.NewObjectId(), name, location ) { }

        [BsonCtorAttribute]
        public Preset( ObjectId id, string name, string location ) :
            base( id ) {
            Name = name;
            Location = location;
        }
    }
}
