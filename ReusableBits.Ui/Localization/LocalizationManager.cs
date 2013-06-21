﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Reflection;
using System.Windows;
using System.ComponentModel;

namespace ReusableBits.Ui.Localization {
	// from: http://www.codeproject.com/Articles/249369/Advanced-WPF-Localization

	/// <summary>
	/// Provides methods to control the localization of an application.
	/// </summary>
	/// <remarks>
	/// This type is thread-safe. YES YOU CAN set localized values from a thread
	/// other than the UI thread. Multiple UI threads are also supported.
	/// </remarks>
	public static class LocalizationManager {
		/// <summary>
		/// Specifies the number of localized values that can be added before purging of dead values
		/// is performed.
		/// </summary>
		private	const int				cPurgeLimit = 100;
		private static readonly object	DefaultResourceManagerSyncRoot = new object();
		private static ResourceManager	mDefaultResourceManager;
		private static bool				mDefaultResourceManagerSet;

		/// <summary>
		/// The default resource manager to use.
		/// </summary>
		public static ResourceManager DefaultResourceManager {
			get {
				if( false == mDefaultResourceManagerSet ) {
					lock( DefaultResourceManagerSyncRoot ) {
						if( false == mDefaultResourceManagerSet ) {
							mDefaultResourceManager = GetDefaultResourceManager();

							mDefaultResourceManagerSet = true;
						}
					}
				}

				return mDefaultResourceManager;
			}
			set {
				lock( DefaultResourceManagerSyncRoot ) {
					mDefaultResourceManager = value;

					mDefaultResourceManagerSet = true;
				}
			}
		}

		/// <summary>
		/// Updates all localized values.
		/// </summary>
		/// <remarks>
		/// This method must be called when the culture of the UI thread changes.
		/// </remarks>
		public static void UpdateValues() {
			var localizedValues = LocalizedValues;

			// Lock the instance because a new value can be added by another thread
			lock( localizedValues ) {
				foreach( LocalizedValue item in localizedValues.Values ) {
					item.UpdateValue();
				}
			}
		}

		/// <summary>
		/// Contains the localized values.
		/// </summary>
		private static readonly Dictionary<LocalizedProperty, LocalizedValue> LocalizedValues = new Dictionary<LocalizedProperty, LocalizedValue>();

		/// <summary>
		/// The number of localized values added after the last purge.
		/// </summary>
		private static int mLocalizedValuesPurgeCount;

		/// <summary>
		/// Adds a new localized value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
		public static void AddLocalizedValue( LocalizedValue value ) {
			InternalAddLocalizedValue( value );

			// Update the value initially
			value.UpdateValue();
		}

		/// <summary>
		/// Adds a new localized value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
		internal static void InternalAddLocalizedValue( LocalizedValue value ) {
			if( value == null ) {
				throw new ArgumentNullException( "value" );
			}

			if( mLocalizedValuesPurgeCount > cPurgeLimit ) {
				// Remove localized values the owners of which have been garbage collected

				lock( LocalizedValues ) {
					List<LocalizedProperty> keys = null;

					foreach( var item in LocalizedValues.Keys ) {
						if( false == item.IsAlive ) {
							if( keys == null ) {
								keys = new List<LocalizedProperty>();
							}

							keys.Add( item );
						}
					}

					if( keys != null ) {
						foreach( var key in keys ) {
							LocalizedValues.Remove( key );
						}
					}

					mLocalizedValuesPurgeCount = 0;
				}
			}

			lock( LocalizedValues ) {
				var count = LocalizedValues.Count;

				LocalizedValues[value.Property] = value;

				if( count < LocalizedValues.Count ) {
					// A new value has been added

					mLocalizedValuesPurgeCount++;
				}
			}
		}

		/// <summary>
		/// Removes any localized value associated with the specified property.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
		/// <remarks>
		/// This method remvoes the property from the list of properties updated automatically when the
		/// current culture changes. The current value of the property remains untouched.
		/// </remarks>
		public static void RemoveLocalizedValue( LocalizedProperty property ) {
			if( property == null ) {
				throw new ArgumentNullException( "property" );
			}

			lock( LocalizedValues ) {
				LocalizedValues.Remove( property );
			}
		}

		private static readonly List<ResourceManagerData> ResourceManagers = new List<ResourceManagerData>();

		internal static ResourceManager LoadResourceManager( Type type ) {
			if( type == null ) {
				throw new ArgumentNullException( "type" );
			}

			lock( ResourceManagers ) {
				var result = ResourceManagers.Find( x => x.Type == type );

				if( result == null ) {
					result = new ResourceManagerData { Type = type, Manager = new ResourceManager( type ) };

					ResourceManagers.Add( result );
				}

				return result.Manager;
			}
		}

		internal static ResourceManager LoadResourceManager( string assemblyName, string baseName ) {
			if( string.IsNullOrEmpty( assemblyName ) ) {
				throw new ArgumentNullException( "assemblyName" );
			}

			if( string.IsNullOrEmpty( baseName ) ) {
				throw new ArgumentNullException( "baseName" );
			}

			var assembly = AppDomain.CurrentDomain.Load( assemblyName );

			lock( ResourceManagers ) {
				var result = ResourceManagers.Find( x => x.Assembly == assembly && string.Equals( x.BaseName, baseName, StringComparison.CurrentCultureIgnoreCase ) );

				if( result == null ) {
					result = new ResourceManagerData { Assembly = assembly, BaseName = baseName, Manager = new ResourceManager( baseName, assembly )};

					ResourceManagers.Add( result );
				}

				return result.Manager;
			}
		}

		/// <summary>
		/// Tries to find a suitable <see cref="ResourceManager"/> to use by default.
		/// </summary>
		/// <returns>
		/// A <see cref="ResourceManager"/> or <c>null</c> if suitable <see cref="ResourceManager"/>
		/// was not found.
		/// </returns>
		static ResourceManager GetDefaultResourceManager() {
			// The assembly in which to look for resources
			Assembly assembly;

			if( DesignerProperties.GetIsInDesignMode( new DependencyObject() ) ) {
				// Design mode

				// Try to find the main assembly of the application

				var assemblies = AppDomain.CurrentDomain.GetAssemblies();

				assembly = null;

				// A .dll assembly
				Assembly libraryAssembly = null;

				foreach( var item in assemblies ) {
					if( item.GlobalAssemblyCache ) {
						continue;
					}

					// The name of the assembly
					var assemblyName = item.GetName().Name;

					if( assemblyName.IndexOf( "Microsoft", StringComparison.InvariantCultureIgnoreCase ) >= 0
						|| assemblyName.IndexOf( "Interop", StringComparison.InvariantCultureIgnoreCase ) >= 0 ) {
						// Avoid Microsoft and interoperability assemblies loaded by Visual Studio

						continue;
					}

					if( string.Equals( assemblyName, "Blend", StringComparison.InvariantCultureIgnoreCase ) ) {
						// Avoid "Blend.exe" of the Expression Blend

						continue;
					}

					var assemblyCompanyAttribute = (AssemblyCompanyAttribute)item.GetCustomAttributes( typeof( AssemblyCompanyAttribute ), false ).FirstOrDefault();

					if( assemblyCompanyAttribute != null && assemblyCompanyAttribute.Company.IndexOf( "Microsoft", StringComparison.InvariantCultureIgnoreCase ) >= 0 ) {
						// Avoid Microsoft assemblies loaded by Visual Studio

						continue;
					}

					var resourceType = item.GetType( assemblyName + ".Properties.Resources", false );

					if( resourceType != null ) {
						// The assembly has default resources

						if( item.EntryPoint != null ) {
							// Check if the assembly contains WPF application (e.g. MyApplication.App class
							// that derives from System.Windows.Application)

							var applicationType = item.GetType( assemblyName + ".App", false );

							if( applicationType != null && typeof( Application ).IsAssignableFrom( applicationType )) {
								// The assembly is valid

								assembly = item;

								break;
							}
						}
						else {
							if( libraryAssembly == null ) {
								libraryAssembly = item;
							}
						}
					}
				}

				if( assembly == null ) {
					// The project must be a library project so use the first assembly that has
					// default resources and does not belong to Microsoft

					assembly = libraryAssembly;
				}
			}
			else {
				if( Application.Current != null && Application.Current.GetType() != typeof( Application ) ) {
					// The assembly of the current WPF application
					assembly = Application.Current.GetType().Assembly;
				}
				else {
					// The entry assembly of the application
					assembly = Assembly.GetEntryAssembly();
				}
			}

			if( assembly != null ) {
				try {
					return new ResourceManager( assembly.GetName().Name + ".Properties.Resources", assembly );
				}
				catch( MissingManifestResourceException ) {
					// The resoures cannot be found in the manifest of the assembly
				}
			}

			return null;
		}

		class ResourceManagerData {
			public Type Type { get; set; }

			public Assembly Assembly { get; set; }

			public string BaseName { get; set; }

			public ResourceManager Manager { get; set; }
		}

		/// <summary>
		/// Returns an object that can ube used to localize the specified dependency property.
		/// </summary>
		/// <param name="obj">The object that owns the property.</param>
		/// <param name="property">The property to localize.</param>
		/// <returns>A localized property.</returns>
		public static LocalizedProperty Property( this DependencyObject obj, DependencyProperty property ) {
			return new LocalizedDependencyProperty( obj, property );
		}

		/// <summary>
		/// Returns an object that can ube used to localize the specified non-dependency property.
		/// </summary>
		/// <param name="obj">The object that owns the property.</param>
		/// <param name="property">The property to localize.</param>
		/// <returns>A localized property.</returns>
		public static LocalizedProperty Property( this DependencyObject obj, PropertyInfo property ) {
			return new LocalizedNonDependencyProperty( obj, property );
		}

		/// <summary>
		/// Returns an object that can ube used to localize the specified non-dependency property.
		/// </summary>
		/// <param name="obj">The object that owns the property.</param>
		/// <param name="propertyName">The name of the property to localize.</param>
		/// <returns>
		/// A localized property.
		/// </returns>
		public static LocalizedProperty Property( this DependencyObject obj, string propertyName ) {
			if( obj == null ) {
				throw new ArgumentNullException( "obj" );
			}

			if( string.IsNullOrEmpty( propertyName ) ) {
				throw new ArgumentNullException( "propertyName" );
			}

			var property = obj.GetType().GetProperty( propertyName );

			if( property == null ) {
				throw new ArgumentOutOfRangeException( "propertyName", null, string.Format( "Property not found in type '{0}'.", obj.GetType()));
			}

			return new LocalizedNonDependencyProperty( obj, property );
		}

		/// <summary>
		/// Assigns a resource value to the specified localized property.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="resourceKey">The resource key.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="resourceKey"/> is null or empty.</exception>
		public static void SetResourceValue( this LocalizedProperty property, string resourceKey ) {
			AddLocalizedValue( new ResourceLocalizedValue( property, resourceKey ) );
		}

		/// <summary>
		/// Assigns a formated value to the specified property.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="formatString">The format string.</param>
		/// <param name="args">The args.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="formatString"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
		public static void SetFormattedValue( this LocalizedProperty property, string formatString, params object[] args ) {
			AddLocalizedValue( new FormattedLocalizedValue( property, formatString, args ) );
		}

		/// <summary>
		/// Assigns a formated value to the specified property. The format string is kept 
		/// in resources.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="resourceKey">The resource key.</param>
		/// <param name="args">The args.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="resourceKey"/> is null or empty.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
		public static void SetResourceFormattedValue( this LocalizedProperty property, string resourceKey, params object[] args ) {
			AddLocalizedValue( new ResourceFormattedLocalizedValue( property, resourceKey, args ) );
		}

		/// <summary>
		/// Assigns a value that is produced by a callback.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <param name="method">The callback method.</param>
		/// <param name="parameter">The parameter to pass to the callback method.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is null.</exception>
		/// <remarks>
		/// The method will be called once to set the initial value of the property and each
		/// time the current culture changes.
		/// </remarks>
		public static void SetCallbackValue( this LocalizedProperty property, LocalizationCallback method, object parameter ) {
			AddLocalizedValue( new MethodLocalizedValue( property, method, parameter ) );
		}

		/// <summary>
		/// Stops localizing the property. Does not clear the current value of the property.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <exception cref="ArgumentNullException"><paramref name="property"/> is null.</exception>
		public static void Clear( this LocalizedProperty property ) {
			RemoveLocalizedValue( property );
		}
	}
}
