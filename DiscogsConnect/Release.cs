namespace DiscogsConnect
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ReleaseResponse : DiscogsResponse
    {
        [JsonProperty("release")]
        public Release Release { get; set; }
    }

    public class Release : ValueObject<Release>
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("master_id")]
        public int MasterId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("released")]
        public string Released { get; set; }

        [JsonProperty("released_formatted")]
        public string ReleasedFormatted { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("styles")]
        public List<string> Styles { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("labels")]
        public List<Label> Labels { get; set; }

        [JsonProperty("videos")]
        public List<Video> Videos { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("tracklist")]
        public List<Track> Tracks { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("extraartists")]
        public List<Artist> ExtraArtists { get; set; }

        public class Label : ValueObject<Label>
        {
            [JsonProperty("catno")]
            public string Catno { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class Artist : ValueObject<Artist>
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("tracks")]
            public string Tracks { get; set; }

            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("anv")]
            public string Anv { get; set; }

            [JsonProperty("join")]
            public string Join { get; set; }
        }
    }    
}
