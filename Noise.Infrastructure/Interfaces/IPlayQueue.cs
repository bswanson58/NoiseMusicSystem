using System;
using System.Collections.Generic;
using DynamicData;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayQueue {
		void			Add( DbTrack track );
		void			Add( IEnumerable<DbTrack> trackList );
		void			Add( DbAlbum album );
		void			Add( DbAlbum album, string volumeName );
		void			Add( DbArtist artist );
		void			Add( DbInternetStream stream );
		void			StrategyAdd( DbTrack track );
		void			StrategyAdd( DbTrack track, PlayQueueTrack afterTrack );
		void			RemoveTrack( PlayQueueTrack track );
		bool			RemoveTrack( long trackId );
		void			RemovePlayedTracks();
		void			ClearQueue();

		PlayQueueTrack	PlayNextTrack();
		bool            CanPlayNextTrack();
		PlayQueueTrack	PlayPreviousTrack();
		bool            CanPlayPreviousTrack();
		void			StopPlay();
		void			StopAtEndOfTrack();
		bool			CanStopAtEndOfTrack();
		void			ReplayQueue();
		void			ContinuePlayFromTrack( PlayQueueTrack track );
		bool			ContinuePlayFromTrack( long trackId );
		bool			ReplayTrack( long trackId );
        void            PromoteTrackFromStrategy( PlayQueueTrack track );

		PlayQueueTrack	NextTrack { get; }
		PlayQueueTrack	PreviousTrack { get; }
		PlayQueueTrack	PlayingTrack { get; set; }
		int				PlayingTrackReplayCount { get; set; }
		bool			IsQueueEmpty { get; }
		bool			IsTrackQueued( DbTrack track );
		int				UnplayedTrackCount { get; }
		int				PlayedTrackCount { get; }
		bool			StrategyRequestsQueued { get; }
		int				IndexOfNextInsert { get; }

		void			ReorderQueueItem( int fromIndex, int toIndex );

		IPlayStrategy			        PlayStrategy { get; }
		void					        SetPlayStrategy( ePlayStrategy strategyId, IPlayStrategyParameters parameters );

        IStrategyDescription   	        PlayExhaustedStrategy { get; }
        ExhaustedStrategySpecification  ExhaustedPlayStrategy { get; set; }

		void			StartPlayStrategy();
		bool			CanStartPlayStrategy { get; }

		bool			DeletedPlayedTracks { get; set; }
		int				MaximumPlayedTracks { get; set; }

		IEnumerable<PlayQueueTrack>				PlayList { get; }
        IObservable<IChangeSet<PlayQueueTrack>>	PlayQueue { get; }
        ISourceList<PlayQueueTrack>				PlaySource { get; }
	}
}
