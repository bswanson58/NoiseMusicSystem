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
        public  bool            IsDuplicate { get; }
        public  int             Rating { get; }
        public  List<PresetTag> Tags { get; set; }
        public  PresetLibrary   Library { get; set; }

        public Preset( string name, string location, PresetLibrary library ) :
            this( ObjectId.NewObjectId(), name, location, false, false, PresetRating.UnRatedValue, library ) { }

        public Preset( ObjectId id, string name, string location, bool isDuplicate, bool isFavorite, int rating, PresetLibrary library ) :
            base( id ) {
            Name = ( name ?? String.Empty ).Trim();
            Location = ( location ?? String.Empty ).Trim();
            IsFavorite = isFavorite;
            IsDuplicate = isDuplicate;
            Rating = rating;
            Library = library;
            Tags = new List<PresetTag>();
        }

        [BsonCtorAttribute]
        public Preset( ObjectId id, string name, string location, bool isDuplicate, bool isFavorite, int rating ) :
            base( id ) {
            Name = ( name ?? String.Empty ).Trim();
            Location = ( location ?? String.Empty ).Trim();
            IsFavorite = isFavorite;
            IsDuplicate = isDuplicate;
            Rating = rating;
            Library = PresetLibrary.Default();
            Tags = new List<PresetTag>();
        }

        private Preset( Preset clone, bool isFavorite ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = isFavorite;
            IsDuplicate = clone.IsDuplicate;
            Rating = clone.Rating;
            Library = clone.Library;
            Tags = new List<PresetTag>( clone.Tags );
        }

        private Preset( Preset clone, PresetRating rating ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = clone.IsFavorite;
            IsDuplicate = clone.IsDuplicate;
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

        public Preset WithTags( IEnumerable<PresetTag> tagList ) {
            Tags.Clear();
            Tags.AddRange( tagList );

            return this;
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

        public Preset WithoutTag( PresetTag tag ) {
            var currentTag = Tags.FirstOrDefault( t => t.Id.Equals( tag.Id ));

            if(currentTag != null ) {
                Tags.Remove( currentTag );
            }

            return this;
        }

        public Preset WithDuplicate( bool isDuplicate ) {
            return new Preset( Id, Name, Location, isDuplicate, IsFavorite, Rating, Library ) { Tags = Tags };
        }
    }
}
