using System;

namespace Noise.Infrastructure.Dto {
	public class LibraryBackup {
		public DateTime		BackupDate { get; private set; }
		public string		BackupPath { get; private set; }

		public LibraryBackup( DateTime backupDate, string backupDirectory ) {
			BackupDate = backupDate;
			BackupPath = backupDirectory;
		}
	}
}
