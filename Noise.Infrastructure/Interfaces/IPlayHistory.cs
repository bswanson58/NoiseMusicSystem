using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayHistory {
		bool						Initialize();

		void						TrackPlayCompleted( PlayQueueTrack track );

		IEnumerable<DbPlayHistory>	PlayHistory { get; }
	}
}
