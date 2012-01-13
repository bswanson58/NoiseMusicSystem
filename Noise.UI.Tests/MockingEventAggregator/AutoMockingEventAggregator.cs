using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Events;
using Rhino.Mocks;

namespace Noise.UI.Tests.MockingEventAggregator {
	public class AutoMockingEventAggregator : IEventAggregator {
		private readonly IDictionary<Type, EventBase> mEvents = new Dictionary<Type, EventBase>();

		public virtual TEvent GetEvent<TEvent>() where TEvent : EventBase, new() {
			return StubEvent<TEvent>();
		}

		private TEvent StubEvent<TEvent>() where TEvent : EventBase {
			var eventType = typeof( TEvent );
			if( mEvents.ContainsKey( eventType ) )
				return (TEvent)mEvents[eventType];

			var @event = (TEvent)Activator.CreateInstance( eventType );
			OverrideDispatcherFacade( @event );
			mEvents[eventType] = @event;

			return @event;
		}

		/// <summary>
		/// Use Reflection to set the uiDispatcher  in CompositePresentationEvent so that mEvents that are Published 
		/// with ThreadOptions.UIThread will work in Unit tests. Without this the CompositePresentationEvent tries to get the UI dispatcher
		/// from Application.Current which isn't available in tests
		/// </summary>
		/// <typeparam name="TEvent"></typeparam>
		/// <param name="event"></param>
		private static void OverrideDispatcherFacade<TEvent>( TEvent @event ) {
			@event.SetDispatcher( MockRepository.GeneratePartialMock<TestDispatcherFacade>() );
		}

		public class TestDispatcherFacade : IDispatcherFacade {
			public virtual void BeginInvoke( Delegate method, object arg ) {
				Dispatcher.CurrentDispatcher.Invoke( method, arg );
			}
		}

	}
}
