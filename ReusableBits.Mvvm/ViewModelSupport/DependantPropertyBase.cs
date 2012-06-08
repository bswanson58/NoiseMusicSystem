using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReusableBits.Mvvm.ViewModelSupport {
	public class DependantPropertyBase : PropertyChangeBase {
		private readonly IDictionary<string, List<string>>	mPropertyMap;

		[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true )]
		protected class DependsUponAttribute : Attribute {
			public string DependencyName { get; private set; }

			public bool VerifyStaticExistence { get; set; }

			public DependsUponAttribute( string propertyName ) {
				DependencyName = propertyName;
			}
		}

		public DependantPropertyBase() {
			mPropertyMap = MapDependencies<DependsUponAttribute>( () => GetType().GetProperties());

			VerifyDependancies();
		}

		protected override void RaisePropertyChanged( string propertyName ) {
			base.RaisePropertyChanged( propertyName );

			if( mPropertyMap.ContainsKey( propertyName )) {
				mPropertyMap[propertyName].ToList().ForEach( RaisePropertyChanged );
			}
		}

		protected static IDictionary<string, List<string>> MapDependencies<T>( Func<IEnumerable<MemberInfo>> getInfo ) where T : DependsUponAttribute {
			var uniqueList = getInfo().GroupBy( p => p.Name ).Select( g => g.First());
			var dependencyMap = uniqueList.ToDictionary(
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
			var methods = GetType().GetMethods().AsEnumerable<MemberInfo>();
			var properties = GetType().GetProperties();

			var propertyNames = methods.Union( properties )
				.SelectMany( method => method.GetCustomAttributes( typeof( DependsUponAttribute ), true ).Cast<DependsUponAttribute>() )
				.Select( attribute => attribute.DependencyName ).AsEnumerable().ToList();

			propertyNames.ForEach( VerifyDependancy );
		}

		private void VerifyDependancy( string propertyName ) {
			var property = GetType().GetProperty( propertyName );
			if( property == null ) {
				throw new ArgumentException( "DependsUpon Property Does Not Exist: " + propertyName );
			}
		}
	}
}
