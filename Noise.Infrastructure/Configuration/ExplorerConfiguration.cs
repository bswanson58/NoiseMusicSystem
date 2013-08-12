using System;
using System.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Configuration {
	public class ExplorerConfiguration : ConfigurationSection {
		public	const string		SectionName = "explorerConfiguration";

		private const string	cEnableLibraryExplorerProperty				= "enableLibraryExplorer";
		private const string	cEnableLibraryChangeUpdateProperty			= "enableLibraryChangeUpdates";
		private const string	cEnableUpdateReadOnlyProperty				= "enableUpdateReadOnly";
		private const string	cEnableGlobalHotkeysProperty				= "enableGlobalHotkeys";
		private const string	cMinimizeToTrayProperty						= "minimizeToTray";
		private const string	cDisplayPlayTimeElapsedProperty				= "displayPlayTimeElapsed";
		private const string	cPlayExhaustedStrategyProperty				= "playExhaustedStrategy";
		private const string	cPlayExhaustedParametersProperty			= "playExhaustedParameters";
		private const string	cPlayStrategyProperty						= "playStrategy";
		private const string	cPlayStrategyParametersProperty				= "playStrategyParameters";
		private const string	cNewAdditionsHorizonDaysProperty			= "newAdditionsHorizonDays";
		private const string	cNewAdditionsHorizonCountProperty			= "newAdditionsHorizonCount";
		private const string	cEnableSortPrefixesProperty					= "enableSortPrefixes";
		private const string	cSortPrefixesProperty						= "sortPrefixes";
		private const string	cHasNetworkAccess							= "hasNetworkAccess";
		private const string	cEnableRemoteAccess							= "enableRemoteAccess";
		private const string	cEnablePlaybackLibraryFocus					= "enablePlaybackLibraryFocus";
		private const string	cEnablePlaybackScrobbling					= "enablePlaybackScrobbling";
		private const string	cLastLibraryUsedProperty					= "lastLibraryUsed";
		private const string	cLoadLastLibraryOnStartup					= "loadLastUsedLibrary";

		[ConfigurationPropertyAttribute( cEnableLibraryExplorerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableLibraryExplorer {
			get { return ((bool)( base[cEnableLibraryExplorerProperty] ) ); }
			set { base[cEnableLibraryExplorerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableLibraryChangeUpdateProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableLibraryChangeUpdates {
			get { return ((bool)( base[cEnableLibraryChangeUpdateProperty] ) ); }
			set { base[cEnableLibraryChangeUpdateProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableUpdateReadOnlyProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool EnableReadOnlyUpdates {
			get { return ((bool)( base[cEnableUpdateReadOnlyProperty] ) ); }
			set { base[cEnableUpdateReadOnlyProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableGlobalHotkeysProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableGlobalHotkeys {
			get { return ((bool)( base[cEnableGlobalHotkeysProperty] ) ); }
			set { base[cEnableGlobalHotkeysProperty] = value; }
		}
		[ConfigurationPropertyAttribute( cMinimizeToTrayProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool MinimizeToTray {
			get { return ((bool)( base[cMinimizeToTrayProperty] ) ); }
			set { base[cMinimizeToTrayProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cDisplayPlayTimeElapsedProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool DisplayPlayTimeElapsed {
			get { return ((bool)( base[cDisplayPlayTimeElapsedProperty] ) ); }
			set { base[cDisplayPlayTimeElapsedProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPlayExhaustedStrategyProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = ePlayExhaustedStrategy.Stop )]
		public ePlayExhaustedStrategy PlayExhaustedStrategy {
			get { return ((ePlayExhaustedStrategy)( base[cPlayExhaustedStrategyProperty] ) ); }
			set { base[cPlayExhaustedStrategyProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPlayExhaustedParametersProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string PlayExhaustedParameters {
			get { return ((string)( base[cPlayExhaustedParametersProperty] ) ); }
			set { base[cPlayExhaustedParametersProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPlayStrategyProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = ePlayStrategy.Next )]
		public ePlayStrategy PlayStrategy {
			get { return ((ePlayStrategy)( base[cPlayStrategyProperty] ) ); }
			set { base[cPlayStrategyProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPlayStrategyParametersProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string PlayStrategyParameters {
			get { return ( (string)( base[cPlayStrategyParametersProperty] ) ); }
			set { base[cPlayStrategyParametersProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cNewAdditionsHorizonDaysProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = (UInt16)90 )]
		public UInt16 NewAdditionsHorizonDays {
			get { return ((UInt16)( base[cNewAdditionsHorizonDaysProperty] ) ); }
			set { base[cNewAdditionsHorizonDaysProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cNewAdditionsHorizonCountProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = (UInt32)500 )]
		public UInt32 NewAdditionsHorizonCount {
			get { return ((UInt32)( base[cNewAdditionsHorizonCountProperty] ) ); }
			set { base[cNewAdditionsHorizonCountProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableSortPrefixesProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableSortPrefixes {
			get { return ((bool)( base[cEnableSortPrefixesProperty] ) ); }
			set { base[cEnableSortPrefixesProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cSortPrefixesProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "" )]
		public string SortPrefixes {
			get { return ((string)( base[cSortPrefixesProperty] ) ); }
			set { base[cSortPrefixesProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cHasNetworkAccess, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool HasNetworkAccess {
			get { return ((bool)( base[cHasNetworkAccess] ) ); }
			set { base[cHasNetworkAccess] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableRemoteAccess, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableRemoteAccess {
			get { return ((bool)( base[cEnableRemoteAccess] ) ); }
			set { base[cEnableRemoteAccess] = value; }
		}

		[ConfigurationPropertyAttribute( cEnablePlaybackLibraryFocus, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool EnablePlaybackLibraryFocus {
			get { return ((bool)( base[cEnablePlaybackLibraryFocus] ) ); }
			set { base[cEnablePlaybackLibraryFocus] = value; }
		}

		[ConfigurationPropertyAttribute( cEnablePlaybackScrobbling, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnablePlaybackScrobbling {
			get { return ( (bool)( base[cEnablePlaybackScrobbling] ) ); }
			set { base[cEnablePlaybackScrobbling] = value; }
		}

		[ConfigurationPropertyAttribute( cLastLibraryUsedProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = 0L )]
		public long LastLibraryUsed {
			get { return ((long)( base[cLastLibraryUsedProperty] ) ); }
			set { base[cLastLibraryUsedProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cLoadLastLibraryOnStartup, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "true" )]
		public bool LoadLastLibraryOnStartup {
			get { return ((bool)( base[cLoadLastLibraryOnStartup] ) ); }
			set { base[cLoadLastLibraryOnStartup] = value; }
		}

	}
}
