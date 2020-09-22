using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetImageHandler {
        byte[]  GetPresetImage( Preset preset );
    }
}
