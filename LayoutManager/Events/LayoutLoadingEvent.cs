#region Using Directives

using Microsoft.Practices.Prism.Events;

#endregion

namespace Composite.Layout.Events
{
    public class LayoutLoadingEvent : CompositePresentationEvent<ILayout>
    {
    }
}