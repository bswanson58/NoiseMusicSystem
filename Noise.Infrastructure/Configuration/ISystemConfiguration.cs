using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	public interface ISystemConfiguration {
		T		RetrieveConfiguration<T>( string sectionName );

		void	Save( ConfigurationSection section );
	}
}
