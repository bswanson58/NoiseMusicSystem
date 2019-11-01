namespace ArchiveLoader.Dto {
    class DisplayedProcessItem {
        public  string      Name { get; }
        public  string      FileName { get; }

        public DisplayedProcessItem( ProcessItem item ) {
            Name = item.Name;
            FileName = item.FileName;
        }
    }
}
