using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.Composite.Presentation.Commands;

namespace Noise.Infrastructure.Support {
	public class ViewModelBase : DynamicObject, INotifyPropertyChanged {
		private readonly Dictionary<string, object>			mValues;
		private readonly IDictionary<string, List<string>>	mPropertyMap;
		private readonly IDictionary<string, List<string>>	mMethodMap;
		private readonly IDictionary<string, List<string>>	mCommandMap;
		private readonly Dispatcher							mDispatcher;

		[AttributeUsage( AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true )]
		protected class DependsUponAttribute : Attribute {
			public string DependencyName { get; private set; }

			public bool VerifyStaticExistence { get; set; }

			public DependsUponAttribute( string propertyName ) {
				DependencyName = propertyName;
			}
		}

		private static readonly bool mIsInDesignMode;

		static ViewModelBase() {
			// Thanks to Laurent Bugnion for this detection code:
			// http://geekswithblogs.net/lbugnion/archive/2009/09/05/detecting-design-time-mode-in-wpf-and-silverlight.aspx
#if SILVERLIGHT
			_isInDesignMode = DesignerProperties.IsInDesignTool;
#else
			var prop = DesignerProperties.IsInDesignModeProperty;
			mIsInDesignMode
					= (bool)DependencyPropertyDescriptor
					.FromProperty( prop, typeof( FrameworkElement ) )
					.Metadata.DefaultValue;
#endif
		}

		public bool IsInDesignMode {
			get {
				return mIsInDesignMode;
			}
		}

		public ViewModelBase() :
			this( Dispatcher.CurrentDispatcher ) {
		}

		public ViewModelBase( Dispatcher dispatcher ) {
			mDispatcher = dispatcher;
			mValues = new Dictionary<string, object>();

			mPropertyMap = MapDependencies<DependsUponAttribute>( () => GetType().GetProperties());
			mMethodMap = MapDependencies<DependsUponAttribute>( () => GetType().GetMethods().Cast<MemberInfo>().Where( method => !method.Name.StartsWith( cCanExecutePrefix ) ) );
			mCommandMap = MapDependencies<DependsUponAttribute>( () => GetType().GetMethods().Cast<MemberInfo>().Where( method => method.Name.StartsWith( cCanExecutePrefix ) ) );

			CreateCommands();
			VerifyDependancies();
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

		protected void RaisePropertyChanged<TProperty>( Expression<Func<TProperty>> property ) {
			RaisePropertyChanged( PropertyName( property ) );
		}

		protected void RaisePropertyChanged( string name ) {
			PropertyChanged.Raise( this, name );
#if SILVERLIGHT
			PropertyChanged.Raise(this, "");
#endif

			if( mPropertyMap.ContainsKey( name ) )
				mPropertyMap[name].Each( RaisePropertyChanged );

			ExecuteDependentMethods( name );
			FireChangesOnDependentCommands( name );
		}

		private void ExecuteDependentMethods( string name ) {
			if( mMethodMap.ContainsKey( name ) )
				mMethodMap[name].Each( ExecuteMethod );
		}

		private void FireChangesOnDependentCommands( string name ) {
			if( mCommandMap.ContainsKey( name ) )
				mCommandMap[name].Each( RaiseCanExecuteChangedEvent );
		}

		protected void Set<T>( Expression<Func<T>> expression, T value ) {
			Set( PropertyName( expression ), value );
		}

		private static string PropertyName<T>( Expression<Func<T>> expression ) {
			var memberExpression = expression.Body as MemberExpression;

			if( memberExpression == null )
				throw new ArgumentException( "expression must be a property expression" );

			return memberExpression.Member.Name;
		}

		public override bool TryGetMember( GetMemberBinder binder, out object result ) {
			result = Get<object>( binder.Name );

			if( result != null )
				return true;

			return base.TryGetMember( binder, out result );
		}

		public override bool TrySetMember( SetMemberBinder binder, object value ) {
			var result = base.TrySetMember( binder, value );
			if( result )
				return true;

			Set( binder.Name, value );
			return true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void CreateCommands() {
			CommandNames.Each( name => Set( name, new DelegateCommand<object>( x => ExecuteCommand( name, x ), x => CanExecuteCommand( name, x ) ) ) );
		}

		private const string cExecutePrefix = "Execute_";
		private const string cCanExecutePrefix = "CanExecute_";

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

#if SILVERLIGHT
		public object this[string key]
		{
			get { return Get<object>(key);}
			set { Set(key, value); }
		}
#endif

		private static IDictionary<string, List<string>> MapDependencies<T>( Func<IEnumerable<MemberInfo>> getInfo ) where T : DependsUponAttribute {
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

			var uniqueValues = flattened.Select( x => x.Value ).Distinct();

			return uniqueValues.ToDictionary(
						x => x,
						x => ( from item in flattened
							   where item.Value == x
							   select item.Key ).ToList() );
		}

		private void ExecuteMethod( string name ) {
			var memberInfo = GetType().GetMethod( name );
			if( memberInfo == null )
				return;

			memberInfo.Invoke( this, null );
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

		public bool CheckAccess() {
			return( mDispatcher == Dispatcher.CurrentDispatcher );
		}

		public void Invoke( Action action ) {
			mDispatcher.Invoke( action );
		}

		public void BeginInvoke( Action action ) {
			mDispatcher.BeginInvoke( action );
		}
	}
}
