using System.Collections.Generic;
using System.Threading.Tasks;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayHistory {
		Task						TrackPlayCompleted( PlayQueueTrack track );

		IEnumerable<DbPlayHistory>	PlayHistory { get; }
	}
}
