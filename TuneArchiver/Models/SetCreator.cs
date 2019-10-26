using System;
using System.Collections.Generic;
using System.Linq;
using TuneArchiver.Interfaces;

namespace TuneArchiver.Models {
    class SetCreator : ISetCreator {
        private const long  cDvd9Size = 4692251770;

        public IEnumerable<Album> GetBestAlbumSet( List<Album> albumList ) {
            var retValue = new List<Album>();
            var totalSetSize = albumList.Sum( album => album.Size );

            if( totalSetSize > cDvd9Size ) {
                var powerSet = Subsets( albumList );
                var selectedSize = 0L;

                foreach ( var set in powerSet ) {
                    var setSize = set.Sum(album => album.Size);

                    if(( setSize > selectedSize ) &&
                       ( setSize <= cDvd9Size )) {
                        retValue.Clear();
                        retValue.AddRange( set );

                        selectedSize = setSize;

                        if( selectedSize > ( cDvd9Size * 0.98 )) {
                            break;
                        }
                    }
                }
            }
            else {
                retValue.Clear();
                retValue.AddRange( albumList );
            }

            return retValue;
        }

        public static IEnumerable<IList<T>> Subsets<T>( IList<T> list) {
            var max = (long)Math.Pow( 2, list.Count );

            for( var count = 0; count < max; count++ ) {
                var subset = new List<T>();
                uint rs = 0;

                while( rs < list.Count ) {
                    if(( count & ( 1u << (int)rs )) > 0 ) {
                        subset.Add( list[(int)rs]);
                    }
                    rs++;
                }

                yield return subset;
            }
        }
    }
}
