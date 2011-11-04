namespace DiscogsConnect
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class MasterReleaseResponse : DiscogsResponse
    {
        [JsonProperty("master")]
        public MasterRelease Master { get; set; }
    }

    public class MasterRelease : ValueObject<MasterRelease>
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("main_release")]
        public int MainRelease { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("styles")]
        public List<string> Styles { get; set; }

        [JsonProperty("genres")]
        public List<string> Genres { get; set; }

        [JsonProperty("videos")]
        public List<Video> Videos { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("tracklist")]
        public List<Track> Tracks { get; set; }

        [JsonProperty("versions")]
        public List<Version> Versions { get; set; }

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

        public class Version : ValueObject<Version>
        {
            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("thumb")]
            public string Thumb { get; set; }

            [JsonProperty("format")]
            public string Format { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("released")]
            public string Released { get; set; }

            [JsonProperty("catno")]
            public string Catno { get; set; }

            [JsonProperty("id")]
            public int Id { get; set; }
        }
    }  
}
