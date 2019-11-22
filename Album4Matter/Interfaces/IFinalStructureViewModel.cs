using System;
using Album4Matter.Dto;

namespace Album4Matter.Interfaces {
    public interface IFinalStructureViewModel {
        void    SetTargetLayout( TargetAlbumLayout layout, Action<TargetItem> onRemoveItem );
    }
}
