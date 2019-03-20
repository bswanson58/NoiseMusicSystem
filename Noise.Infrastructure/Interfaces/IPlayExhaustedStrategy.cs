namespace Noise.Infrastructure.Interfaces {
	public enum ePlayExhaustedStrategy {
		Stop = 1,
		Replay = 2,
		PlayList = 3,
		PlayArtist = 4,
		PlaySimilar = 5,
		PlayFavorites = 6,
		PlayStream = 7,
		PlayGenre = 8,
		PlayArtistGenre = 9,
		PlayCategory = 10,
		SeldomPlayedArtists = 11,
        PlayFavoritesPlus = 12,
        PlayUserTags = 13
	}

	public interface IPlayExhaustedStrategy {
		ePlayExhaustedStrategy	StrategyId { get; }
        string					StrategyName {  get; }
		string					StrategyDescription { get; }
        string					ConfiguredDescription {  get; }
        bool					RequiresParameters {  get; }
		string					ParameterName { get; }
		IPlayStrategyParameters Parameters {  get; }

        bool					Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters );
		bool					QueueTracks();
	}
}
