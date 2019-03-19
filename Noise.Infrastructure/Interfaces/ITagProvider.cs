using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagProvider {
		void						AddTag( DbTag tag );
        IDataUpdateShell<DbTag>     GetTagForUpdate( long dbid );

		IDataProviderList<DbTag>	GetTagList( eTagGroup forGroup );
	}
}
