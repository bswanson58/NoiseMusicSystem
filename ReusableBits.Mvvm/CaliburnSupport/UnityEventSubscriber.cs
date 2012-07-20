using Caliburn.Micro;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

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

			public override void PostBuildUp( IBuilderContext context ) {
				if(( context.Existing is IHandle ) &&
				   (!( context.Existing is IDisableAutoEventSubscribe ))) {
					mEventAggregator.Subscribe( context.Existing );
				}
			}
		}
	}
}
