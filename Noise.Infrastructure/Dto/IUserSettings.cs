using System;

namespace Noise.Infrastructure.Dto {
	public interface IUserSettings {
		bool	IsFavorite { get; set; }
		string	Genre { get; set; }
		Int16	Rating { get; set; }
		bool	IsUserRating { get; }
	}
}
