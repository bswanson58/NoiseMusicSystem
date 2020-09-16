using System.Collections.Generic;
using System.Linq;

namespace MilkBottle.Entities {
    class PresetSidecar {
        public  bool            IsFavorite { get; set; }
        public  List<string>    Tags { get; set; }
        public  int             Rating { get; set; }

        public PresetSidecar() {
            IsFavorite = false;
            Rating = 0;
            Tags = new List<string>();
        }

        public PresetSidecar( Preset fromPreset ) {
            IsFavorite = fromPreset.IsFavorite;
            Rating = fromPreset.Rating;
            Tags = new List<string>( from t in fromPreset.Tags select t.Name );
        }
    }
}
