#region Using Directives

using System.Collections.Generic;
using Microsoft.Practices.Unity;

#endregion

namespace Composite.Layout
{
    public interface ILayoutManager
    {
        string ShellName { get; set; }
        List<ILayout> Layouts { get; set; }
        ILayout CurrentLayout { get; }
        bool IsInitialized { get; }
        void Initialize(IUnityContainer container);
        void LoadLayout();
        void LoadLayout(string layoutName);
    }
}