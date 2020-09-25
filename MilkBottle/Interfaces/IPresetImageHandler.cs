using System.Threading.Tasks;
using MilkBottle.Entities;

namespace MilkBottle.Interfaces {
    interface IPresetImageHandler {
        byte[]          GetPresetImage( Preset preset );
        Task<byte[]>    CapturePresetImage( Preset preset );
    }
}
