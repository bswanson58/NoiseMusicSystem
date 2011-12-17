using System.Collections.Generic;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayHistory {
		void						TrackPlayCompleted( PlayQueueTrack track );

		IEnumerable<DbPlayHistory>	PlayHistory { get; }
	}
}
