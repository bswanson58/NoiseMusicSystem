using System.Collections.Generic;

namespace TuneRenamer.Dto {
    public class WordReplacement {
        public  string  Word { get; set; }
        public  string  Replacement { get; set; }
    }

    public class WordReplacements {
        public List<WordReplacement>    ReplacementList { get; }

        public WordReplacements() {
            ReplacementList = new List<WordReplacement>();
        }
    }
}
