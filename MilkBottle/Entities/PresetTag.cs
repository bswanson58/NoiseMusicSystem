using System;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Tag: {" + nameof( Name ) + "}")]
    class PresetTag : EntityBase {
        public  string  Identity { get; }
        public  string  Name {  get; }

        public PresetTag( string name ) :
            base( ObjectId.NewObjectId()) {
            Identity = ObjectId.NewObjectId().ToString();
            Name = name ?? String.Empty;
        }

        [BsonCtor]
        public PresetTag( ObjectId id, string identity, string name ) :
            base( id ) {
            Identity = identity ?? String.Empty;
            Name = name ?? String.Empty;
        }

        public PresetTag WithName( string name ) {
            return new PresetTag( Id, Identity, name );
        }
    }
}
