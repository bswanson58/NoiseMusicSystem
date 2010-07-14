using System.Configuration;

namespace Noise.Infrastructure.Configuration {
	// The ConfigurationElementCollection<t> provides a simple generic implementation of ConfigurationElementCollection.
	[ConfigurationCollection( typeof( ConfigurationElement ) )]
	public class ConfigurationElementCollection<T> : ConfigurationElementCollection where T : ConfigurationElement, new() {
		protected override ConfigurationElement CreateNewElement() {
			return new T();
		}

		protected override object GetElementKey( ConfigurationElement element ) {
			return((element).ToString());
		}

		public void Add( T element ) {
			BaseAdd( element );
		}

		public T this[int idx] {
			get { return (T)BaseGet( idx ); }
		}

		public T this[object key] {
			get { return((T)( BaseGet( key ))); }
		}

		public void Remove( T element ) {
			BaseRemove( GetElementKey( element ));
		}

		public T GetItemAt( int index ) {
			return((T)( BaseGet( index )));
		}

		public T GetItemByKey( int key ) {
			return((T)( BaseGet((object)key )));
		}

		public override bool IsReadOnly() {
			return( false );
		}
	}
}
