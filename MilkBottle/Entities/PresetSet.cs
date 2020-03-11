using System;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Set: {" + nameof( Name ) + "}")]
    class PresetSet : EntityBase {
        public  string  Name { get; }

        public PresetSet( string name ) :
            base( ObjectId.NewObjectId()) {
            Name = name ?? String.Empty;
        }

        [BsonCtorAttribute]
        public PresetSet( ObjectId id, string name ) :
            base( id ) {
            Name = name ?? String.Empty;
        }
    }
}
