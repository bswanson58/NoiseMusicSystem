namespace Noise.Infrastructure.Interfaces {
	public enum ePlayExhaustedStrategy {
		Stop,
		Replay,
		PlayList,
		PlayArtist,
		PlaySimilar,
		PlayFavorites,
		PlayStream,
		PlayGenre,
		PlayArtistGenre,
		PlayCategory,
		SeldomPlayedArtists
	}

	public interface IPlayExhaustedStrategy {
		ePlayExhaustedStrategy	StrategyId { get; }
        string					DisplayName {  get; }
        string					StrategyDescription {  get; }
        bool					RequiresParameters {  get; }

        bool					Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters );

		bool					QueueTracks();
	}
}
