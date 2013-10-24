using System.Linq;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.PlayStrategies {
	internal class PlayStrategySingle : IPlayStrategy {
        private IPlayQueue  mPlayQueue;

	    public ePlayStrategy StrategyId {
            get {  return( ePlayStrategy.Next ); }
	    }
	    public string DisplayName {
            get {  return( "Normal" ); }
	    }

	    public string StrategyDescription {
            get {  return( "in sequential order" ); }
	    }

	    public bool RequiresParameters {
            get {  return( false ); }
	    }

		public IPlayStrategyParameters Parameters {
			get {  return( null ); }
		}

	    public bool Initialize( IPlayQueue queueMgr, IPlayStrategyParameters parameters ) {
            mPlayQueue = queueMgr;

            return( true );
	    }

	    public PlayQueueTrack NextTrack() {
			return( mPlayQueue.PlayList.FirstOrDefault( track => ( !track.IsPlaying ) && ( !track.HasPlayed )));
	    }
	}
}
