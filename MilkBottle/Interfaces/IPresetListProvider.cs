using System.Collections.Generic;
using LiteDB;
using MilkBottle.Dto;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetListProvider {
        IEnumerable<PresetList> GetLists();
        IEnumerable<Preset>     GetPresets( PresetSet forSet );
        IEnumerable<Preset>     GetPresets( PresetListType ofType, ObjectId id );
    }
}
