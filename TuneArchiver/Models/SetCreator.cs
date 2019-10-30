using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TuneArchiver.Interfaces;

namespace TuneArchiver.Models {
    class SetCreatorProgress {
        public  long    TotalIterations { get; }
        public  long    CurrentIteration { get; }
        public  long    ArchiveSize { get; }
        public  long    CurrentSetSize { get; }

        public SetCreatorProgress( long totalIterations, long currentIteration, long archiveSize, long currentSetSize ) {
            TotalIterations = totalIterations;
            CurrentIteration = currentIteration;
            ArchiveSize = archiveSize;
            CurrentSetSize = currentSetSize;
        }
    }

    class SetCreator : ISetCreator {
        private const long  cDvd9Size = 4692251770;

        async Task<IEnumerable<Album>> ISetCreator.GetBestAlbumSet( IList<Album> albumList, IProgress<SetCreatorProgress> progressReporter, CancellationTokenSource cancellation ) {
            var random = new Random( DateTime.Now.Millisecond );
            var limitedList = albumList.OrderBy( a => random.Next( Int32.MaxValue)).Take( 150 ).ToList();

            return await Task.Run(() => GetBestAlbumSet( limitedList, progressReporter, cancellation ));
        }

        private IEnumerable<Album> GetBestAlbumSet( IList<Album> albumList, IProgress<SetCreatorProgress> progressReporter, CancellationTokenSource cancellation ) {
            var retValue = new List<Album>();
            var totalSetSize = albumList.Sum( album => album.Size );

            if( totalSetSize > cDvd9Size ) {
                var powerSet = Subsets( albumList );
                var maxIterations = (long)Math.Pow( 2, albumList.Count );
                var iteration = 0;
                var selectedSize = 0L;

                foreach ( var set in powerSet ) {
                    var setSize = set.Sum( album => album.Size );

                    if(( setSize > selectedSize ) &&
                       ( setSize <= cDvd9Size )) {
                        retValue.Clear();
                        retValue.AddRange( set );

                        selectedSize = setSize;

                        if( selectedSize > ( cDvd9Size * 0.995 )) {
                            break;
                        }
                    }

                    iteration++;

                    if( cancellation.IsCancellationRequested ) {
                        break;
                    }

                    if(( maxIterations < 64000 ) ||
                       ( iteration % 1000 == 0 )) {
                        progressReporter.Report( new SetCreatorProgress( maxIterations, iteration, cDvd9Size, selectedSize ));
                    }
                }

                // issue a final report.
                progressReporter.Report( new SetCreatorProgress( maxIterations, iteration, cDvd9Size, selectedSize ));
            }
            else {
                retValue.Clear();
                retValue.AddRange( albumList );

                progressReporter.Report( new SetCreatorProgress( 0, 0, cDvd9Size, totalSetSize ));
            }

            return retValue;
        }

        public static IEnumerable<IList<T>> Subsets<T>( IList<T> list) {
            var max = Math.Pow( 2, list.Count );

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
