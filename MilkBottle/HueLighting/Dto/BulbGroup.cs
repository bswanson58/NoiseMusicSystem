using System.Collections.Generic;
using System.Diagnostics;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Dto {
    [DebuggerDisplay("Group: {" + nameof( Name ) + "}")]
    public class BulbGroup {
        public  string      Name { get; }
        public  GroupType   GroupType { get; }
        public  List<Bulb>  Bulbs { get; }

        public BulbGroup( string name, GroupType? groupType, IEnumerable<Bulb> bulbs ) {
            Name = name;
            GroupType = groupType ?? GroupType.Room;
            Bulbs = new List<Bulb>( bulbs );
        }
    }
}
