using System;

namespace HueLighting.Dto {
    public class Bulb {
        public  String      Id { get; set; }
        public  String      Name { get; set; }
        public  bool        Available { get; set; }

        public Bulb() {
            Id = String.Empty;
            Name = String.Empty;
            Available = false;
        }

        public Bulb( string id, string name, bool available ) {
            Id = id;
            Name = name;
            Available = available;
        }
    }
}
