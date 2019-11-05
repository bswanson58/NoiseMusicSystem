using ArchiveLoader.Dto;

namespace ArchiveLoader.Interfaces {
    public interface IProcessQueue {
        bool    CanAddProcessItem();
        void    AddProcessItem( ProcessHandler handler );
    }
}
