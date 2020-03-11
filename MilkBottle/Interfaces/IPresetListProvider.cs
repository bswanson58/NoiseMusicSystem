using System.Collections.Generic;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPresetListProvider {
        IEnumerable<PresetList> GetLists();
    }
}
