using System;

namespace Noise.Infrastructure.Dto {
	public class LibraryBackup {
		public DateTime		BackupDate { get; }
		public string		BackupPath { get; }

		public LibraryBackup( DateTime backupDate, string backupDirectory ) {
			BackupDate = backupDate;
			BackupPath = backupDirectory;
		}
	}
}
