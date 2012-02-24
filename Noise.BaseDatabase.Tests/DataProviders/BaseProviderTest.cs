namespace Noise.BaseDatabase.Tests.DataProviders {
	public abstract class BaseProviderTest<TEntity> where TEntity : class {
		protected abstract TEntity	CreateSut();
	}
}
