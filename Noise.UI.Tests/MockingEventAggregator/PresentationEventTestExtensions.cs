using System;
using System.Reflection;
using Microsoft.Practices.Prism.Events;
using Rhino.Mocks;

namespace Noise.UI.Tests.MockingEventAggregator {
	public static class PresentationEventTestExtensions {
		public static void SetDispatcher<TEvent>( this TEvent me, IDispatcherFacade facade ) {
			GetDispatcherField<TEvent>().SetValue( me, facade );
		}

		private static FieldInfo GetDispatcherField<TEvent>() {
			return typeof( TEvent ).GetPrivateField( "uiDispatcher" );
		}

		private static IDispatcherFacade GetDispatcher<TEvent, TPayload>( this TEvent me ) where TEvent : CompositePresentationEvent<TPayload> {
			return (IDispatcherFacade)GetDispatcherField<TEvent>().GetValue( me );
		}

		public static void AssertSubscriptionWasOnUiThread<TEvent, TPayload>( this TEvent me, TPayload arg ) where TEvent : CompositePresentationEvent<TPayload> {
			me.GetDispatcher<TEvent, TPayload>().AssertWasCalled( x => x.BeginInvoke( Arg<Delegate>.Is.Anything, Arg<TPayload>.Is.Equal( arg ) ), o => o.Repeat.AtLeastOnce() );
		}
	}
}