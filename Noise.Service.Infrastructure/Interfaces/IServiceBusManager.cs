using NServiceBus;

namespace Noise.Service.Infrastructure.Interfaces {
	public interface IServiceBusManager {
		bool	InitializeServer();
		bool	Initialize( string serverName );

		void	Publish( IMessage message );
	}
}
