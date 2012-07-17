using Caliburn.Micro;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace ReusableBits.Mvvm.CaliburnSupport {
	public class UnityEventSubscriber : UnityContainerExtension {
		private readonly IEventAggregator	mEventAggregator;

		public UnityEventSubscriber( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;
		}

		protected override void Initialize() {
			Context.Strategies.Add( new EventSubscriberStrategy( mEventAggregator ), UnityBuildStage.PostInitialization );
		}

		private class EventSubscriberStrategy : BuilderStrategy {
			private readonly IEventAggregator	mEventAggregator;

			public EventSubscriberStrategy( IEventAggregator eventAggregator ) {
				mEventAggregator = eventAggregator;
			}

			public override void PostBuildUp( IBuilderContext context ) {
				if(( context.Existing != null ) &&
				   ( context.Existing is IHandle )) {
					mEventAggregator.Subscribe( context.Existing );
				}
			}
		}
	}
}
