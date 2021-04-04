using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;
using Blazoop.Source.Properties.Vector;

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

            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            
            ToolbarOperator = nodeBase.ServiceData.OperationManager.GetOperation<ToolbarOperator>();
            ToolbarOperator.StyleOp = StyleOperator;
            
            WindowingService = nodeBase.ServiceData.OperationManager.GetOperation<WindowingService>();

            Add("node", ElementNode = new LinkMember(this));
            
            var TestMenu = ToolbarOperator.CreateMenu("File", StyleOperator);
            ElementNode.Add(TestMenu.ElementNode);
            
            var item1 = ToolbarOperator.CreateItem(TestMenu, "item1");
                item1.AddEvent("onmousedown", o => Console.WriteLine("clicked on the file button"));
            
            ToolbarOperator.CreateItem(TestMenu, "item2");
            
            var WindowMenu = ToolbarOperator.CreateMenu("Window", StyleOperator);
            
            ElementNode.Add(WindowMenu.ElementNode);
            
            WindowingService.TabService.OnTabMove += () =>
            {
                SetupTabItems(WindowMenu);
            };
            
            var ViewMenu = ToolbarOperator.CreateMenu("View", StyleOperator);
            
            ElementNode.Add(ViewMenu.ElementNode);
            
            WindowingService.TabService.OnTabMove += () =>
            {
                SetupTabItems(ViewMenu);
            };
            
            WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOperator, this, 
                ("display","grid"),
                ("grid-auto-rows", "100%"),
                ("grid-auto-columns", "minmax(80px, min-content)"),
                ("align-content","center"),
                ("grid-template-areas",$"\"{ToolbarOperator.GetJoinedMenus()}\""));
        }

        public void CreateTabItem(MenuPart menu, TabData tabData)
        {
            var tabItem = ToolbarOperator.CreateItem(menu, tabData.Label);
            tabItem.WithAttribute("draggable", out AttributeString drag);
            drag.Value = "true";

            tabItem.PreventDefaults.Add("onfocus");
            tabItem.PreventDefaults.Add("ondragover");
            tabItem.PreventDefaults.Add("ondrop");
            tabItem.StopPropagations.Add("ondragover");
            tabItem.StopPropagations.Add("ondrop");

            tabItem.AddEvent("ondragstart", o =>
            {
                ToolbarOperator.StartItemDrag(tabData, tabItem);
            });

            tabItem.AddEvent("ondrop", o =>
            {
                ToolbarOperator.DroppedTabItem(menu, WindowingService, tabData, tabItem);
                ToolbarOperator.EndItemDrag();   
            });

            tabItem.AddEvent("onmouseenter", (o =>
            {
                if (tabData.TabGroup is TabService.UNJOINED)
                    return;
                
                tabData.TabContext.Hover(true);
            }));

            tabItem.AddEvent("onmouseleave", (o =>
            {
                if (tabData.TabGroup is TabService.UNJOINED)
                    return;

                tabData.TabContext.Hover(false);
                WindowingService.UpdateTabs(tabData.TabGroup);
            }));

            tabItem.AddEvent("onclick", (o =>
            {
                WindowContext window = null;
                
                if (tabData.TabGroup is TabService.UNJOINED)
                {
                    Console.WriteLine("here");
                    window = WindowingService.CreateWindow;
                    WindowingService.AddTabToWindow(window, tabData);
                    window.Transform.Position = new Position().Subtract<Position>(WindowingService.ContainerContext.SliderTransform.Position);
                }
                else
                {
                    window = WindowingService.TabService.ObjectTabMap.FirstOrDefault(item => item.Value == tabData.TabGroup).Key as WindowContext;
                    if (window is null) return;
                    WindowingService.ContainerContext.SliderTransform.Position = new Position().Subtract<Position>(window.Transform.Position);
                }
                
                WindowingService.SelectTab(tabData);
                WindowingService.UpdateTabs(tabData.TabGroup);
                WindowingService.WindowToFront(window);
                WindowingService.ContainerContext.Slider.SurrogateReference?.ChangeState();
            }));
        }

        public void SetupTabItems(MenuPart menu)
        {
            foreach (var linkMember in ToolbarOperator.Labels[menu])
            {
                linkMember.ElementNode.Pop();
            }
            ToolbarOperator.Labels[menu].Clear();

            foreach ((string key, var tabs) in WindowingService.TabService.TabGroupMap)
            {
                string label = key;
                if (key is TabService.UNJOINED) label = "None";
                
                var viewTabItem = ToolbarOperator.CreateItem(menu, label);
                //.SetHtml(label);
                
                viewTabItem.cssClass += " vslim";
                viewTabItem.SurrogateReference?.ChangeState();

                tabs.Group.ForEach(e =>
                {
                    CreateTabItem(menu, e);
                });
            }

            menu.SurrogateReference?.ChangeState();
        }
    }
}