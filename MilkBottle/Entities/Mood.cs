
using System;
using System.Diagnostics;
using LiteDB;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Mood: {" + nameof( Name ) + "}")]
    class Mood : EntityBase {
        public  string  Identity { get; }
        public  string  Name {  get; }

        public Mood( string name ) :
            base( ObjectId.NewObjectId()) {
            Identity = ObjectId.NewObjectId().ToString();
            Name = name ?? String.Empty;
        }

        [BsonCtor]
        public Mood( ObjectId id, string identity, string name ) :
            base( id ) {
            Identity = identity ?? String.Empty;
            Name = name ?? String.Empty;
        }

        public Mood WithName( string name ) {
            return new Mood( Id, Identity, name );
        }
    }
}
