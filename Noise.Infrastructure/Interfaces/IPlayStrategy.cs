using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum ePlayStrategy {
		Next = 1,
		Random = 2,
		TwoFers = 3,
		FeaturedArtists = 4,
		NewReleases = 5
	}

    public interface IPlayStrategy {
        ePlayStrategy			StrategyId { get; }
        string					StrategyName { get; }
		string					StrategyDescription { get; }
        string					ConfiguredDescription { get; }
        bool					RequiresParameters { get; }
		string					ParameterName { get; }
		IPlayStrategyParameters Parameters { get; }

        bool					Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters );
		PlayQueueTrack			NextTrack();
	}
}
