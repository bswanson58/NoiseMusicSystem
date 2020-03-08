using System;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Tag: {" + nameof( Name ) + "}")]
    class PresetTag : EntityBase {
        public  string  Name {  get; }

        public PresetTag( string name ) :
            base( ObjectId.NewObjectId()) {
            Name = name ?? String.Empty;
        }

        [BsonCtor]
        public PresetTag( ObjectId id, string name ) :
            base( id ) {
            Name = name ?? String.Empty;
        }

        public PresetTag WithName( string name ) {
            return new PresetTag( this.Id, name );
        }
    }
}
