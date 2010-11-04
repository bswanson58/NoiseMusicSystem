using Quartz;

namespace Noise.Service.LibraryService {
	internal class LibraryUpdateJob : IJob {
		public void Execute( JobExecutionContext context ) {
			if( context != null ) {
				var	service = context.Trigger.JobDataMap[LibraryServiceImpl.cNoiseLibraryUpdate] as LibraryServiceImpl;

				if( service != null ) {
					service.UpdateLibrary();
				}
			}
		}
	}
}
