using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IGenreProvider {
		void						AddGenre( DbGenre genre );

		DataProviderList<DbGenre>	GetGenreList();
	}
}
