using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;

namespace Blazoop.Source.ElementContexts
{
    public class ViewDropdownContext : ElementContext, INodeElement
    {
        public StyleOperator StyleOperator { get; }
        public WindowingService WindowingService { get; }
        public LinkMember ElementNode { get; }

        public ToolbarOperator ToolbarOperator { get; }
        
        public ViewDropdownContext(IRootElement nodeBase) : base($"ViewBar_{nodeBase.NodeBase.Id}_")
        {
            cssClass = "window-container-toolbar";
            
            ToolbarOperator = nodeBase.ServiceData.OperationManager.GetOperation<ToolbarOperator>();
            WindowingService = nodeBase.ServiceData.OperationManager.GetOperation<WindowingService>();

            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            Add("node", ElementNode = new LinkMember(this));
            
            var TestMenu = ToolbarOperator.CreateMenu("File", StyleOperator);
            ElementNode.Add(TestMenu.ElementNode);
            
            ToolbarOperator.CreateItem(TestMenu, "item1");
            ToolbarOperator.CreateItem(TestMenu, "item2");
            
            var TestMenu2 = ToolbarOperator.CreateMenu("View", StyleOperator);
            
            ElementNode.Add(TestMenu2.ElementNode);
            
            TestMenu2.AddEvent("onmouseenter", (a) =>
            {
                foreach (var linkMember in ToolbarOperator.Labels[TestMenu2])
                {
                    linkMember.ElementNode.Pop();
                }
                ToolbarOperator.Labels[TestMenu2].Clear();
                
                foreach ((string key, var tabs) in WindowingService.TabService.TabGroupMap)
                {
                    
                    var hold = ToolbarOperator.CreateItem(TestMenu2, key);
                    hold.cssClass += " vslim";
                    hold.SurrogateReference?.ChangeState();
                    
                    tabs.Group.ForEach(e =>
                    {
                        ToolbarOperator.CreateItem(TestMenu2, e.Label);
                        Console.WriteLine(e.Label);
                    });
                    
                    ToolbarOperator.OpenMenu(TestMenu2);
                    TestMenu2.SurrogateReference?.ChangeState();
                }
            });
            
            WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOperator, this, 
                ("display","grid"),
                ("grid-auto-rows", "100%"),
                ("grid-auto-columns", "minmax(80px, min-content)"),
                ("align-content","center"),
                ("grid-template-areas",$"\"{ToolbarOperator.GetJoinedMenus()}\""));
            
        }
    }
}