using System;
using System.Configuration;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Configuration {
	public class ExplorerConfiguration : ConfigurationSection {
		public	const string		SectionName = "explorerConfiguration";

		private const string	cEnableLibraryExplorerProperty				= "enableLibraryExplorer";
		private const string	cEnableBackgroundContentExplorerProperty	= "enableBackgroundContentExplorer";
		private const string	cEnableLibraryChangeUpdateProperty			= "enableLibraryChangeUpdates";
		private const string	cEnableUpdateReadOnlyProperty				= "enableUpdateReadOnly";
		private const string	cMinimizeToTrayProperty						= "minimizeToTray";
		private const string	cDisplayPlayTimeElapsedProperty				= "displayPlayTimeElapsed";
		private const string	cPlayExhaustedStrategyProperty				= "playExhaustedStrategy";
		private const string	cPlayExhaustedItemProperty					= "playExhaustedItem";
		private const string	cPlayStrategyProperty						= "playStrategy";
		private const string	cNewAdditionsHorizonDaysProperty			= "newAdditionsHorizonDays";
		private const string	cNewAdditionsHorizonCountProperty			= "newAdditionsHorizonCount";
		private const string	cEnableSortPrefixesProperty					= "enableSortPrefixes";
		private const string	cSortPrefixesProperty						= "sortPrefixes";

		[ConfigurationPropertyAttribute( cEnableLibraryExplorerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableLibraryExplorer {
			get { return ((bool)( base[cEnableLibraryExplorerProperty] ) ); }
			set { base[cEnableLibraryExplorerProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cEnableBackgroundContentExplorerProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = "false" )]
		public bool EnableBackgroundContentExplorer {
			get { return ((bool)( base[cEnableBackgroundContentExplorerProperty] ) ); }
			set { base[cEnableBackgroundContentExplorerProperty] = value; }
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

		[ConfigurationPropertyAttribute( cPlayExhaustedItemProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = Constants.cDatabaseNullOid )]
		public long PlayExhaustedItem {
			get { return ((long)( base[cPlayExhaustedItemProperty] ) ); }
			set { base[cPlayExhaustedItemProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cPlayStrategyProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = ePlayStrategy.Next )]
		public ePlayStrategy PlayStrategy {
			get { return ((ePlayStrategy)( base[cPlayStrategyProperty] ) ); }
			set { base[cPlayStrategyProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cNewAdditionsHorizonDaysProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = (UInt16)7 )]
		public UInt16 NewAdditionsHorizonDays {
			get { return ((UInt16)( base[cNewAdditionsHorizonDaysProperty] ) ); }
			set { base[cNewAdditionsHorizonDaysProperty] = value; }
		}

		[ConfigurationPropertyAttribute( cNewAdditionsHorizonCountProperty, IsRequired = false, IsKey = false, IsDefaultCollection = false, DefaultValue = (UInt32)250 )]
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
	}
}
