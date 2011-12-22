using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IPlayListProvider {
		DataProviderList<DbPlayList>	GetPlayLists();
	}
}
