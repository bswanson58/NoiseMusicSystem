using Caliburn.Micro;
using Unity.Builder;
using Unity.Extension;
using Unity.Strategies;

namespace ReusableBits.Mvvm.CaliburnSupport {
	public class UnityEventSubscriber : UnityContainerExtension {
		protected readonly IEventAggregator	mEventAggregator;

		public UnityEventSubscriber( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
		}

		protected override void Initialize() {
			Context.Strategies.Add( new EventSubscriberStrategy( mEventAggregator ), UnityBuildStage.PostInitialization );
		}

		protected class EventSubscriberStrategy : BuilderStrategy {
			private readonly IEventAggregator	mEventAggregator;

			public EventSubscriberStrategy( IEventAggregator eventAggregator ) {
				mEventAggregator = eventAggregator;
			}

            public override void PostBuildUp( ref BuilderContext context ) {
				if(( context.Existing is IHandle ) &&
				   (!( context.Existing is IDisableAutoEventSubscribe ))) {
					mEventAggregator.Subscribe( context.Existing );
				}
			}
		}
	}
}
