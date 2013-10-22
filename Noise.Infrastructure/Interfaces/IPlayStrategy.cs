using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public enum ePlayStrategy {
		Next,
		Random,
		TwoFers,
		FeaturedArtists,
		NewReleases
	}

    public interface IPlayStrategy {
        ePlayStrategy   StrategyId {  get; }
        string          DisplayName {  get; }
        string          StrategyDescription {  get; }
        bool            RequiresParameters {  get; }

        bool            Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters );

		PlayQueueTrack	NextTrack();
	}
}
