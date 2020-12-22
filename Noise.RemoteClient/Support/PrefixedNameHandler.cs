using System;
using System.Collections.Generic;
using System.Linq;
using Noise.RemoteClient.Interfaces;
using Xamarin.Essentials.Interfaces;

namespace Noise.RemoteClient.Support {
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
            ArePrefixesEnabled = mPreferences.Get( PreferenceNames.UseSortPrefixes, true );

            var prefixes = mPreferences.Get( PreferenceNames.SortPrefixes, "the" );

            mSortPrefixes.AddRange(from p in prefixes.Split('|') select p.Trim());
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
