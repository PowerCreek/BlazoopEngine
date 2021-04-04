using System;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.Operations;

namespace Blazoop.Source.ElementContexts
{
    public class TabData
    {
        public string Label { get; set; }
        public string TabGroup { get; set; }
        public TabContext TabContext { get; set; }
        public TabContentContext Content { get; set; }

        public void SelectTab(bool selected)
        {
            TabContext?.SelectTab(selected);
            Content?.Show(selected);
        }

        public void DisconnectTab()
        {
            TabContext?.ElementNode.Pop();
            Content?.ElementNode.Pop();
            (TabContext?.ElementNode?.Parent?.Parent.Value as ElementContext)?.SurrogateReference?.ChangeState();
            (Content?.ElementNode?.Parent?.Parent.Value as ElementContext)?.SurrogateReference?.ChangeState();
        }

        private int iteration = 0;
        
        public void ConnectToWindow(WindowContext context)
        {
            context.TabSection.ElementNode.Add(TabContext?.ElementNode);
            context.ContentPane.ElementNode.Add(Content?.ElementNode);
            context.TabSection.SurrogateReference?.ChangeState();
            context.ContentPane.SurrogateReference?.ChangeState();
        }
    }
}