namespace ArchiveLoader.Dto {
    class DisplayedProcessItem {
        public  string      Key {  get; }
        public  string      Name { get; }
        public  string      FileName { get; }

        public DisplayedProcessItem( ProcessItem item ) {
            Key = item.Key;
            Name = item.Name;
            FileName = item.FileName;
        }
    }
}
