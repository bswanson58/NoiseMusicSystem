using System;
using System.Threading;
using System.Threading.Tasks;

namespace HueLighting.Support {
    internal static class Repeat {
        public static Task Interval( TimeSpan pollInterval, Action action, CancellationToken token ) =>
            // We don't use Observable.Interval:
            // If we block, the values start bunching up behind each other.
            Task.Run( () => {
                while( true ) {
                    if( token.WaitCancellationRequested( pollInterval )) {
                        break;
                    }

                    action();
                }
            }, token );

        public static Task<bool> IntervalWhile( TimeSpan pollInterval, Func<Task<bool>> action, CancellationToken token ) =>
            Task.Run( async () => {
                var result = false;

                do {
                    if( token.WaitCancellationRequested( pollInterval )) {
                        break;
                    }

                    result = await action();
                }
                while( result );

                return result;
            }, token );
    }

    static class CancellationTokenExtensions {
        public static bool WaitCancellationRequested( this CancellationToken token, TimeSpan timeout ) =>
            token.WaitHandle.WaitOne( timeout );
    }
}
