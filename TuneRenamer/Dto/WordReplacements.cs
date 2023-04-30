using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TuneRenamer.Dto {
    public class WordReplacement {
        public  string  Word { get; set; }
        public  string  Replacement { get; set; }

        public WordReplacement() {
            Word = String.Empty;
            Replacement = String.Empty;
        }

        [JsonConstructor]
        public WordReplacement( string word, string replacement ) {
            Word = word;
            Replacement = replacement;
        }
    }

    public class WordReplacements {
        public List<WordReplacement>    ReplacementList { get; }

        public WordReplacements() {
            ReplacementList = new List<WordReplacement>();
        }
    }
}
