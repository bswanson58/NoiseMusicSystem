namespace MilkBottle.Infrastructure.Interfaces {
    public interface IPreferences {
        T		Load<T>() where T : new();
        T		Load<T>( string path ) where T : new();
        void	Save<T>( T preferences );
        void	Save<T>( T preferences, string path );
    }
}
