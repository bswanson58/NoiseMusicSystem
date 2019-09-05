using System;
using System.Collections.Generic;
using System.Linq;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Interfaces;

namespace Noise.UI.Support {
    class PrefixedNameHandler : IPrefixedNameHandler {
        private readonly IPreferences   mPreferences;
        private readonly List<string>	mSortPrefixes;

        public  bool                    ArePrefixesEnabled { get; private set; }

        public PrefixedNameHandler( IPreferences preferences ) {
            mPreferences = preferences;

            mSortPrefixes = new List<string>();

            InitializePrefixes();
        }

        private void InitializePrefixes() {
            var configuration = mPreferences.Load<UserInterfacePreferences>();
            if( configuration != null ) {
                ArePrefixesEnabled = configuration.EnableSortPrefixes;

                if( ArePrefixesEnabled ) {
                    mSortPrefixes.AddRange(from p in configuration.SortPrefixes.Split('|') select p.Trim());
                }
            }
        }

        public string FormatPrefixedName( string name ) {
            var retValue = name;

            if( ArePrefixesEnabled ) {
                foreach( string prefix in mSortPrefixes ) {
                    if( name.StartsWith( prefix + " ", StringComparison.CurrentCultureIgnoreCase )) {
                        retValue = "(" + name.Insert( prefix.Length, ")" );

                        break;
                    }
                }
            }

            return retValue;
        }

        public string FormatSortingName( string name ) {
            var retValue = name;

            if( ArePrefixesEnabled ) {
                foreach( string prefix in mSortPrefixes ) {
                    if( name.StartsWith( prefix + " ", StringComparison.CurrentCultureIgnoreCase )) {
                        retValue = name.Remove( 0, prefix.Length ).Trim();

                        break;
                    }
                }
            }

            return retValue;
        }
    }
}
