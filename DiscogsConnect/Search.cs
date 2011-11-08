namespace DiscogsConnect
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class SearchResponse : DiscogsResponse
    {
        [JsonProperty("search")]
        public Search Search { get; set; }
    }

    public class Search : ValueObject<Search>
    {        
        [JsonProperty("searchresults")]
        public SearchResults Result { get; set; }
                
        public class SearchResults : ValueObject<SearchResults>
        {
            [JsonProperty("numResults")]
            public int NumResults { get; set; }

            [JsonProperty("start")]
            public int Start { get; set; }

            [JsonProperty("end")]
            public int End { get; set; }

            [JsonProperty("results")]
            public List<SearchResult> Results { get; set; }           
        }

        public class SearchResult : ValueObject<SearchResult>
        {
            [JsonProperty("thumb")]
            public string Thumb { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("type")]
            [JsonConverter(typeof(StringEnumConverter))]
            public SearchResultType Type { get; set; }

            [JsonProperty("uri")]
            public string Uri { get; set; }

            [JsonProperty("summary")]
            public string Summary { get; set; }            
        }

        public enum SearchResultType
        {
            Release,
            Master,
            Artist,
            Label
        }
    }  
}
