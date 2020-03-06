using System;
using System.Diagnostics;
using LiteDB;
using MilkBottle.Types;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Preset: {" + nameof( Name ) + "}")]
    class Preset : EntityBase {
        public  string          Name { get; }
        public  string          Location { get; }
        public  bool            IsFavorite { get; }
        public  int             Rating { get; }
        public  PresetLibrary   Library { get; set; }

        public Preset( string name, string location, PresetLibrary library ) :
            this( ObjectId.NewObjectId(), name, location, false, PresetRating.UnRatedValue, library ) { }

        public Preset( ObjectId id, string name, string location, bool isFavorite, int rating, PresetLibrary library ) :
            base( id ) {
            Name = name ?? String.Empty;
            Location = location ?? String.Empty;
            IsFavorite = isFavorite;
            Rating = rating;
            Library = library;
        }

        [BsonCtorAttribute]
        public Preset( ObjectId id, string name, string location, bool isFavorite, int rating ) :
            base( id ) {
            Name = name ?? String.Empty;
            Location = location ?? String.Empty;
            IsFavorite = isFavorite;
            Rating = rating;
            Library = new PresetLibrary( String.Empty, String.Empty );
        }

        private Preset( Preset clone, bool isFavorite ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = isFavorite;
            Rating = clone.Rating;
            Library = clone.Library;
        }

        private Preset( Preset clone, PresetRating rating ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = clone.IsFavorite;
            Rating = rating;
            Library = clone.Library;
        }

        public Preset WithFavorite( bool isFavorite ) {
            return new Preset( this, isFavorite );
        }

        public Preset WithRating( PresetRating rating ) {
            return new Preset( this, rating );
        }
    }
}
