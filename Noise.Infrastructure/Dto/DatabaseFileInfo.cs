namespace Noise.Infrastructure.Dto {
	public class DatabaseFileInfo {
		public	string		LogicalName { get; }
		public	string		PhysicalName { get; }
		public	string		FileType { get; }
		public	long		FileSize { get; }

		public DatabaseFileInfo( string logicalName, string physicalName, string fileType, long fileSize ) {
			LogicalName = logicalName;
			PhysicalName = physicalName;
			FileType = fileType;
			FileSize = fileSize;
		}
	}
}
