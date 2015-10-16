namespace Noise.Infrastructure.Dto {
	public class DatabaseFileInfo {
		public	string		LogicalName { get; private set; }
		public	string		PhysicalName { get; private set; }
		public	string		FileType { get; private set; }
		public	long		FileSize { get; private set; }

		public DatabaseFileInfo( string logicalName, string physicalName, string fileType, long fileSize ) {
			LogicalName = logicalName;
			PhysicalName = physicalName;
			FileType = fileType;
			FileSize = fileSize;
		}
	}
}
