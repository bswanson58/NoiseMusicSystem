using System.Collections.Generic;
using MilkBottle.Dto;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetListProvider {
        IEnumerable<PresetList> GetLists();
        IEnumerable<Preset>     GetPresets( PresetSet forSet );
    }
}
