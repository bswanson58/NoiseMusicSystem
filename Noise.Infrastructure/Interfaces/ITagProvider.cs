using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface ITagProvider {
		void						AddTag( DbTag tag );
        void                        DeleteTag( DbTag tag );
        DbTag                       GetTag( long dbid );
        IDataUpdateShell<DbTag>     GetTagForUpdate( long dbid );

		IDataProviderList<DbTag>	GetTagList( eTagGroup forGroup );
	}
}
