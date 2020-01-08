using System;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Interfaces {
	public interface ISelectionState {
		DbArtist                    CurrentArtist { get; }
		DbAlbum                     CurrentAlbum { get; }
        DbTag                       CurrentTag { get; }
        PlayingItem					CurrentlyPlayingItem { get; }
        void                        SetCurrentAlbumVolume( string volumeName );

		IObservable<DbArtist>       CurrentArtistChanged { get; }
		IObservable<DbAlbum>        CurrentAlbumChanged { get; }
        IObservable<string>         CurrentAlbumVolumeChanged { get; }
        IObservable<DbTag>          CurrentTagChanged { get; }
        IObservable<PlayingItem>    PlayingTrackChanged { get; }
	}
}
