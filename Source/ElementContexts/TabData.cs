using Blazoop.ExternalDeps.Classes.Management;

namespace Blazoop.Source.ElementContexts
{
    public class TabData
    {
        public string TabGroup { get; set; }
        public TabContext TabContext { get; set; }
        public ElementContext Content { get; set; }

        public void SelectTab(bool selected)
        {
            TabContext?.SelectTab(selected);
        }
    }
}