using Quartz;

namespace Noise.ServiceImpl.LibraryUpdate {
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
