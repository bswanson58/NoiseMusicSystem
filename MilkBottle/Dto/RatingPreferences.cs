namespace MilkBottle.Dto {
    class RatingPreferences {
        public  int     ArtistRating { get; set; }
        public  int     AlbumRating { get; set; }
        public  int     TrackRating { get; set; }
        public  int     ArtistGenreRating { get; set; }
        public  int     TagsRating { get; set; }
        public  int     PublishedYearRating { get; set; }
        public  int     FavoritesRating { get; set; }
        public  int     TimeOfDayRating { get; set; }
        public  int     MoodRating { get; set; }
        public  int     PreferenceBoost { get; set; }

        public RatingPreferences() {
            TrackRating = 8;
            AlbumRating = 7;
            ArtistRating = 6;
            ArtistGenreRating = 5;
            TagsRating = 5;
            PublishedYearRating = 5;
            FavoritesRating = 5;

            TimeOfDayRating = 5;

            MoodRating = 5;

            PreferenceBoost = 3;
        }
    }
}
