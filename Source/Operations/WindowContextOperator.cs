using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Blazoop.Source.ElementContexts;
using Microsoft.JSInterop;

namespace Blazoop.Source.Operations
{
    public class WindowContextOperator : OperationBase
    {
        
        public IJSRuntime JsRuntime { get; }

        public IServiceData ServiceData { get; set; }
        public WindowingService WindowingService { get; set; }
        
        public WindowContextOperator(IJSRuntime jsRuntime)
        {
            JsRuntime = jsRuntime;
        }

        public void DoThing()
        {
            var hold = WindowingService.CreateWindow;

            for (int i = 0; i < 4; i++)
            {
                var tab = WindowingService.CreateTab<TabData>();
                tab.Label = $"test{i}";
                tab.TabContext = new TabContext(WindowingService.ContainerContext.NodeBase, tab) { };
                tab.TabContext.SetHtml(tab.Label);
                tab.Content = new InfoBoxContent(WindowingService.ContainerContext.NodeBase);
                tab.Content.SetHtml($"TAB {i}");
                WindowingService.AddTabToWindow(hold, tab);
            }
        }

        public void Register(IServiceData serviceData)
        {
            ServiceData = serviceData;
            WindowingService = ServiceData.OperationManager.GetOperation<WindowingService>();
        }
    }
}