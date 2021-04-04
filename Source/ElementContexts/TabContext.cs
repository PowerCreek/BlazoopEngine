using System;
using System.Linq;
using Blazoop.ExternalDeps.Classes;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;
using Microsoft.AspNetCore.Components.Web;

namespace Blazoop.Source.ElementContexts
{
    public class TabContext : ElementContext, INodeElement
    {
        
        public TabData TabData { get; }
        
        public LinkMember ElementNode { get; }
        
        public WindowingService WindowingService { get; init; }
        public StyleOperator StyleOperator { get; init; }
        
        public TabContext(IRootElement nodeBase, TabData tabData) : base($"Tabs{nodeBase.NodeBase.Id}")
        {
            TabData = tabData;
            
            WindowingService = nodeBase.ServiceData.OperationManager.GetOperation<WindowingService>();
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            Add("node", ElementNode = new LinkMember(this));

            cssClass = "window-tab";
            
            WithAttribute("style", out StyleContext tabsStyle);
            tabsStyle.WithStyle(StyleOperator, this,
                ("grid-area", Id));

            WithAttribute("draggable", out AttributeString drag);
            drag.Value = "true";

            PreventDefaults.Add("ondragover");
            PreventDefaults.Add("ondrop");
            StopPropagations.Add("ondragover");
            StopPropagations.Add("ondrop");
            
            AddEvent("onmousedown", args => OnTabDown(args));
            AddEvent("onmouseup", args => OnTabUp(args));
            AddEvent("onmouseleave", args => OnTabLeave(args));
            
            AddEvent("ondragstart", args => OnTabDragStart(args));
            AddEvent("ondragend", args => OnTabDragEnd(args));
            
            AddEvent("ondrop", args => OnDrop(args));
            
            SetHtml("what");
        }

        public void Hover(bool isHovering)
        {
            WithAttribute("style", out StyleContext tabsStyle);
            if (isHovering)
            {
                tabsStyle.WithStyle(StyleOperator, this,
                    ("border", "2px solid red"));
            }
        }
        
        public void SelectTab(bool selected)
        {
            string color = selected ? "white" : "black";
            WithAttribute("style", out StyleContext tabsStyle);
            tabsStyle.WithStyle(StyleOperator, this,
                ("border", $"2px solid {color}"));
        }

        public void OnDrop(object args)
        {
            WindowingService.OnTabTabDrop(args as DragEventArgs, TabData);
        }

        public void OnTabUp(object args)
        {
            
        }

        public void OnTabDown(object args)
        {
            WindowingService.TabService.SelectTab(TabData);
            WindowingService.TabMouseDown(args, TabData);
        }

        public void OnTabLeave(object args)
        {
            
        }

        public void OnTabDragStart(object args)
        {
            WindowingService.TabDragStart(args, TabData);
        }
        
        public void OnTabDragEnd(object args)
        {
            WindowingService.TabDragEnd(args, TabData);
        }

        
    }
}