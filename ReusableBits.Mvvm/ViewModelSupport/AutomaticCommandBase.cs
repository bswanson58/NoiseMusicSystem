using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.Prism.Commands;

namespace ReusableBits.Mvvm.ViewModelSupport {
    internal static partial class Extensions {
        public static string StripLeft( this string value, int length ) {
            return value.Substring(length, value.Length - length);
        }
    }

	public class AutomaticCommandBase : PropertyChangeBase {
		private const string cExecutePrefix = "Execute_";
		private const string cCanExecutePrefix = "CanExecute_";

		private readonly Dictionary<string, object>			mValues;
		private readonly IDictionary<string, List<string>>	mMethodMap;
		private readonly IDictionary<string, List<string>>	mCommandMap;

		protected AutomaticCommandBase() {
			mValues = new Dictionary<string, object>();
			mMethodMap = MapDependencies<DependsUponAttribute>( () => GetType().GetMethods().Cast<MemberInfo>().Where( method => !method.Name.StartsWith( cCanExecutePrefix )));
			mCommandMap = MapDependencies<DependsUponAttribute>( () => GetType().GetMethods().Cast<MemberInfo>().Where( method => method.Name.StartsWith( cCanExecutePrefix )));

			CreateCommands();
		}

		private void CreateCommands() {
			CommandNames.ToList().ForEach( name => Set( name, new DelegateCommand<object>( x => ExecuteCommand( name, x ), x => CanExecuteCommand( name, x ))));
		}

		private IEnumerable<string> CommandNames {
			get {
				return from method in GetType().GetMethods()
					   where method.Name.StartsWith( cExecutePrefix )
					   select method.Name.StripLeft( cExecutePrefix.Length );
			}
		}

		private void ExecuteCommand( string name, object parameter ) {
			var methodInfo = GetType().GetMethod( cExecutePrefix + name );
			if( methodInfo == null ) return;

			methodInfo.Invoke( this, methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null );
		}

		private bool CanExecuteCommand( string name, object parameter ) {
			var methodInfo = GetType().GetMethod( cCanExecutePrefix + name );
			if( methodInfo == null ) return true;

			return (bool)methodInfo.Invoke( this, methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null );
		}

		protected void RaiseCanExecuteChangedEvent( string canExecuteName ) {
			var commandName = canExecuteName.StripLeft( cCanExecutePrefix.Length );
			var command = Get<DelegateCommand<object>>( commandName );
			if( command == null )
				return;

			command.RaiseCanExecuteChanged();
		}

		protected override void RaisePropertyChanged( string name ) {
			base.RaisePropertyChanged( name );

			ExecuteDependentMethods( name );
			FireChangesOnDependentCommands( name );
		}

		private void ExecuteDependentMethods( string name ) {
			if( mMethodMap.ContainsKey( name ) )
				mMethodMap[name].ToList().ForEach( ExecuteMethod );
		}

		private void ExecuteMethod( string name ) {
			var memberInfo = GetType().GetMethod( name );
			if( memberInfo == null )
				return;

			memberInfo.Invoke( this, null );
		}
		private void FireChangesOnDependentCommands( string name ) {
			if( mCommandMap.ContainsKey( name ) )
				mCommandMap[name].ToList().ForEach( RaiseCanExecuteChangedEvent );
		}

		protected T Get<T>( string name ) {
			return Get( name, default( T ) );
		}

		protected T Get<T>( string name, T defaultValue ) {
			if( mValues.ContainsKey( name ) ) {
				return (T)mValues[name];
			}

			return defaultValue;
		}

		protected T Get<T>( string name, Func<T> initialValue ) {
			if( mValues.ContainsKey( name ) ) {
				return (T)mValues[name];
			}

			Set( name, initialValue() );
			return Get<T>( name );
		}

		protected T Get<T>( Expression<Func<T>> expression ) {
			return Get<T>( PropertyName( expression ) );
		}

		protected T Get<T>( Expression<Func<T>> expression, T defaultValue ) {
			return Get( PropertyName( expression ), defaultValue );
		}

		protected T Get<T>( Expression<Func<T>> expression, Func<T> initialValue ) {
			return Get( PropertyName( expression ), initialValue );
		}

		public void Set<T>( string name, T value ) {
			if( mValues.ContainsKey( name ) ) {
				if( mValues[name] == null && value == null )
					return;

				if( mValues[name] != null && mValues[name].Equals( value ) )
					return;

				mValues[name] = value;
			}
			else {
				mValues.Add( name, value );
			}

			RaisePropertyChanged( name );
		}

	}
}
