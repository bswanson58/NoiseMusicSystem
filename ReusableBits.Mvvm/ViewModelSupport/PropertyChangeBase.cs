using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ReusableBits.Mvvm.ViewModelSupport {
	internal static partial class Extensions {
		public static void Each<T>( this IEnumerable<T> items, Action<T> action ) {
			foreach( var item in items )
				action( item );
		}

		public static void Raise( this PropertyChangedEventHandler eventHandler, object source, string propertyName ) {
			var handlers = eventHandler;
			if( handlers != null )
				handlers( source, new PropertyChangedEventArgs( propertyName ) );
		}

		public static void Raise( this EventHandler eventHandler, object source ) {
			var handlers = eventHandler;
			if( handlers != null )
				handlers( source, EventArgs.Empty );
		}
	}

	public class PropertyChangeBase : INotifyPropertyChanged {
		private readonly IDictionary<string, List<string>>	mPropertyMap;

		public event PropertyChangedEventHandler PropertyChanged;

		[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true )]
		protected class DependsUponAttribute : Attribute {
			public string DependencyName { get; private set; }

			public bool VerifyStaticExistence { get; set; }

			public DependsUponAttribute( string propertyName ) {
				DependencyName = propertyName;
			}
		}

		public PropertyChangeBase() {
			mPropertyMap = MapDependencies<DependsUponAttribute>( () => GetType().GetProperties());

			VerifyDependancies();
		}

		protected void RaisePropertyChanged<TProperty>( Expression<Func<TProperty>> property ) {
			RaisePropertyChanged( PropertyName( property ) );
		}

		protected virtual void RaisePropertyChanged( string name ) {
			PropertyChanged.Raise( this, name );

			if( mPropertyMap.ContainsKey( name )) {
				mPropertyMap[name].Each( RaisePropertyChanged );
			}
		}

		protected static string PropertyName<T>( Expression<Func<T>> expression ) {
			var memberExpression = expression.Body as MemberExpression;

			if( memberExpression == null ) {
				throw new ArgumentException( "expression must be a property expression" );
			}

			return memberExpression.Member.Name;
		}

		protected static IDictionary<string, List<string>> MapDependencies<T>( Func<IEnumerable<MemberInfo>> getInfo ) where T : DependsUponAttribute {
			var dependencyMap = getInfo().ToDictionary(
						p => p.Name,
						p => p.GetCustomAttributes( typeof( T ), true )
							  .Cast<T>()
							  .Select( a => a.DependencyName )
							  .ToList() );

			return Invert( dependencyMap );
		}

		private static IDictionary<string, List<string>> Invert( IDictionary<string, List<string>> map ) {
			var flattened = from key in map.Keys
							from value in map[key]
							select new { Key = key, Value = value };

			var uniqueValues = flattened.ToList().Select( x => x.Value ).Distinct();

			return uniqueValues.ToDictionary(
						x => x,
						x => ( from item in flattened
							   where item.Value == x
							   select item.Key ).ToList() );
		}

		private void VerifyDependancies() {
			var methods = GetType().GetMethods().Cast<MemberInfo>();
			var properties = GetType().GetProperties();

			var propertyNames = methods.Union( properties )
				.SelectMany( method => method.GetCustomAttributes( typeof( DependsUponAttribute ), true ).Cast<DependsUponAttribute>() )
				.Where( attribute => attribute.VerifyStaticExistence )
				.Select( attribute => attribute.DependencyName );

			propertyNames.Each( VerifyDependancy );
		}

		private void VerifyDependancy( string propertyName ) {
			var property = GetType().GetProperty( propertyName );
			if( property == null )
				throw new ArgumentException( "DependsUpon Property Does Not Exist: " + propertyName );
		}
	}
}
