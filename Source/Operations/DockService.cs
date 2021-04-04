using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.ElementContexts;

namespace Blazoop.Source.Operations
{
    public class DockService
    {
        public (LinkMember link, ElementContext context) CreateDock(string id)
        {
            ElementContext context = new ElementContext($"{id}_DockZone");
            LinkMember hold = new(context);

            return (hold, context);
        }
    }
}