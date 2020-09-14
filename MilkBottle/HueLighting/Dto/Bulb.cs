using System;
using System.Diagnostics;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Dto {
    public class BulbLocation {
        public  double  X { get; }
        public  double  Y { get; }
        public  double  Z { get; }

        public BulbLocation() { }

        public BulbLocation( LightLocation location ) {
            X = location.X;
            Y = location.Y;
            Z = location.Z;
        }
    }

    [DebuggerDisplay("Bulb: {" + nameof( DebugString ) + "}")]
    public class Bulb {
        public  String          Id { get; set; }
        public  String          Name { get; set; }
        public  bool            Available { get; set; }
        public  BulbLocation    Location { get; }

        public  string      DebugString => $"{Name} ({Id})";

        public Bulb() {
            Id = String.Empty;
            Name = String.Empty;
            Available = false;
            Location = new BulbLocation();
        }

        public Bulb( string id, string name, bool available ) {
            Id = id;
            Name = name;
            Available = available;
            Location = new BulbLocation();
        }

        public Bulb( string id, string name, bool available, LightLocation location ) {
            Id = id;
            Name = name;
            Available = available;
            Location = new BulbLocation( location );
        }
    }
}
