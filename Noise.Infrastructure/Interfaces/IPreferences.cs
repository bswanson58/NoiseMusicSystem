namespace Noise.Infrastructure.Interfaces {
	public interface IPreferences {
		T		Load<T>() where T : new();
		void	Save<T>( T preferences );
	}
}
