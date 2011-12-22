using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagProvider {
		void						AddTag( DbTag tag );
		DataProviderList<DbTag>		GetTagList( eTagGroup forGroup );
	}
}
