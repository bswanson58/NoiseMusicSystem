using System;
using Noise.Infrastructure.Dto;

namespace Noise.UI.Interfaces {
	public interface ISelectionState {
		DbArtist    CurrentArtist { get; }
		DbAlbum     CurrentAlbum { get; }
        DbTag       CurrentTag { get; }

		IObservable<DbArtist>   CurrentArtistChanged { get; }
		IObservable<DbAlbum>    CurrentAlbumChanged { get; }
        IObservable<DbTag>      CurrentTagChanged { get; }
	}
}
