using Noise.Infrastructure.Dto;

namespace Noise.Core.FileProcessor {
	public interface IStorageFileProcessor {
		void	Process( DatabaseChangeSummary summary );
		void	Stop();
	}

	internal enum ePipelineState {
		Stopped,
		DetermineFileState,
		BuildAudioFile,
		BuildInfoFile,
		BuildArtworkFile,
		BuildUnknownFile
	}
	public enum ePipelineTrigger {
		DetermineFileType,
		FileTypeIsAudio,
		FileTypeIsInfo,
		FileTypeIsArtwork,
		FileTypeIsUnknown,
		Completed,
		Undetermined,
		Unexpected
	}
}
