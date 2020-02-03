using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDataUpdateShell<out T> : IDisposable {
		T		Item { get; }
		void	Update();
	}

    public interface ITrackUpdateShell : IDataUpdateShell<DbTrack> {
        void    UpdateTrackAndAlbum();
    }
}
