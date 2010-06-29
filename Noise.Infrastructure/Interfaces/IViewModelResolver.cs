namespace Noise.Infrastructure.Interfaces {
	public interface IViewModelResolver {
		object	Resolve( string viewModelName );
	}
}
