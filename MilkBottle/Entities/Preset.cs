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
        public  List<string>    Categories { get; set; }
        public  PresetLibrary   ParentLibrary { get; set; }

        public  string          PrimaryCategory => Categories.Any() ? Categories[0] : String.Empty;

        public Preset( string name, string location, PresetLibrary parentLibrary ) :
            this( ObjectId.NewObjectId(), name, location, false, false, PresetRating.UnRatedValue, parentLibrary ) { }

        public Preset( ObjectId id, string name, string location, bool isDuplicate, bool isFavorite, int rating, PresetLibrary parentLibrary ) :
            base( id ) {
            Name = ( name ?? String.Empty ).Trim();
            Location = ( location ?? String.Empty ).Trim();
            IsFavorite = isFavorite;
            IsDuplicate = isDuplicate;
            Rating = rating;
            ParentLibrary = parentLibrary;
            Tags = new List<PresetTag>();
            Categories = new List<string>();
        }

        [BsonCtorAttribute]
        public Preset( ObjectId id, string name, string location, bool isDuplicate, bool isFavorite, int rating ) :
            base( id ) {
            Name = ( name ?? String.Empty ).Trim();
            Location = ( location ?? String.Empty ).Trim();
            IsFavorite = isFavorite;
            IsDuplicate = isDuplicate;
            Rating = rating;
            ParentLibrary = PresetLibrary.Default();
            Tags = new List<PresetTag>();
            Categories = new List<string>();
        }

        private Preset( Preset clone, bool isFavorite ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = isFavorite;
            IsDuplicate = clone.IsDuplicate;
            Rating = clone.Rating;
            ParentLibrary = clone.ParentLibrary;
            Tags = new List<PresetTag>( clone.Tags );
            Categories = new List<string>( clone.Categories );
        }

        private Preset( Preset clone, PresetRating rating ) :
            base( clone.Id ) {
            Name = clone.Name;
            Location = clone.Location;
            IsFavorite = clone.IsFavorite;
            IsDuplicate = clone.IsDuplicate;
            Rating = rating;
            ParentLibrary = clone.ParentLibrary;
            Tags = new List<PresetTag>( clone.Tags );
            Categories = new List<string>( clone.Categories );
        }

        public Preset WithFavorite( bool isFavorite ) {
            return new Preset( this, isFavorite );
        }

        public Preset WithCategories( IEnumerable<string> categories ) {
            Categories.Clear();
            Categories.AddRange( categories );

            return this;
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
            return new Preset( Id, Name, Location, isDuplicate, IsFavorite, Rating, ParentLibrary ) { Tags = Tags, Categories = Categories };
        }
    }
}
