using System.Threading.Tasks;
using Caliburn.Micro;

namespace ReusableBits.Mvvm.CaliburnSupport {
	/// <summary>
	/// Extensions for <see cref="IEventAggregator"/>.
	/// </summary>
	public static class EventAggregatorExtensions {
		/// <summary>
		///   Publishes a message on the current thread (synchrone).
		/// </summary>
		/// <param name="eventAggregator">The event aggregator.</param>
		/// <param name = "message">The message instance.</param>
		public static void PublishOnCurrentThread(this IEventAggregator eventAggregator, object message) {
			eventAggregator.Publish(message, action => action());
		}

		/// <summary>
		///   Publishes a message on a background thread (async).
		/// </summary>
		/// <param name="eventAggregator">The event aggregator.</param>
		/// <param name = "message">The message instance.</param>
		public static void PublishOnBackgroundThread(this IEventAggregator eventAggregator, object message) {
	#if !SILVERLIGHT || SL5 || WP8
			eventAggregator.Publish(message, action => Task.Factory.StartNew(action));
	#else
			eventAggregator.Publish(message, action => System.Threading.ThreadPool.QueueUserWorkItem(state => action()));
	#endif
		}
	}
}
