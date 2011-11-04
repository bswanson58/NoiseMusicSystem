namespace DiscogsConnect
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ArtistResponse : DiscogsResponse
    {
        [JsonProperty("artist")]
        public Artist Artist { get; set; }
    }

    public class Artist : ValueObject<Artist>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("realname")]
        public string RealName { get; set; }

        [JsonProperty("namevariations")]
        public List<string> NameVariations { get; set; }

        [JsonProperty("aliases")]
        public List<string> Aliases { get; set; }

        [JsonProperty("urls")]
        public List<string> Urls { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("releases")]
        public List<Release> Releases { get; set; }

        [JsonProperty("members")]
        public List<string> BandMembers { get; set; }

        public class Release : ValueObject<Release>
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("thumb")]
            public string Thumb { get; set; }

            [JsonProperty("main_release")]
            public int MainRelease { get; set; }

            [JsonProperty("role")]
            public string Role { get; set; }

            [JsonProperty("year")]
            public int Year { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }
        }
    }
}
