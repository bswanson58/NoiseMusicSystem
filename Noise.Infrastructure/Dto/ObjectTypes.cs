using System;

namespace Noise.Infrastructure.Dto {
	[Flags]
	public enum ObjectTypes {
		Artists = 1,
		Albums = 2,
		Tracks = 4,
		Favorites = 8,
		Ratings = 16,
		Streams = 32,
		Playlists = 64
	}
}
