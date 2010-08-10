using System;

namespace Noise.Infrastructure.Dto {
	public interface IUserSettings {
		bool	IsFavorite { get; set; }
		Int16	Rating { get; set; }
		string	Genre { get; set; }
	}
}
