using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiteDB;
using MilkBottle.Types;

namespace MilkBottle.Entities {
    [DebuggerDisplay("Preset: {" + nameof( Name ) + "}")]
    class Preset : EntityBase {
        public  string          Name { get; }
        public  string          Location { get; }
        public  bool            IsFavorite { get; }
        public  int             Rating { get; }
        public  List<PresetTag> Tags { get; set; }
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
            Tags = new List<PresetTag>();
        }

        [BsonCtorAttribute]
        public Preset( ObjectId id, string name, string location, bool isFavorite, int rating ) :
            base( id ) {
            Name = name ?? String.Empty;
            Location = location ?? String.Empty;
            IsFavorite = isFavorite;
            Rating = rating;
            Library = new PresetLibrary( String.Empty, String.Empty );
            Tags = new List<PresetTag>();
        }

        private Preset( Preset clone, bool isFavorite ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = isFavorite;
            Rating = clone.Rating;
            Library = clone.Library;
            Tags = new List<PresetTag>( clone.Tags );
        }

        private Preset( Preset clone, PresetRating rating ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = clone.IsFavorite;
            Rating = rating;
            Library = clone.Library;
            Tags = new List<PresetTag>( clone.Tags );
        }

        public Preset WithFavorite( bool isFavorite ) {
            return new Preset( this, isFavorite );
        }

        public Preset WithRating( PresetRating rating ) {
            return new Preset( this, rating );
        }

        public Preset WithTagState( PresetTag tag, bool added ) {
            var currentTag = Tags.FirstOrDefault( t => t.Id.Equals( tag.Id ));

            if( added ) {
                if( currentTag == null ) {
                    Tags.Add( tag );
                }
            }
            else {
                if( currentTag != null ) {
                    Tags.Remove( currentTag );
                }
            }

            return this;
        }
    }
}
