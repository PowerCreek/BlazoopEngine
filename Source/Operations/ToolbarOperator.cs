using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Blazoop.Source.ElementContexts;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;

namespace Blazoop.Source.Operations
{
    public class ToolbarOperator : OperationBase
    {

        public LinkMember EmptyLink = new LinkMember(null);
        
        public List<MenuPart> Menus = new();
        public MenuPart ActiveMenu { get; set; }

        public Dictionary<MenuPart, List<MenuItem>> Labels = new();
        
        public static int id = 0;
        public ToolbarOperator(IJSRuntime jsRuntime) : base(jsRuntime)
        {
            
        }

        public void CloseMenu(MenuPart menu)
        {
            if (menu is null) return;

            foreach (var menuItem in Labels[menu])
            {
                menuItem.Key = $"hide_{menuItem.Id}";
                EmptyLink.Add(menuItem.ElementNode);
            }
            
            menu.SurrogateReference?.ChangeState();
        }
        
        public void OpenMenu(MenuPart menu)
        {
            
            CloseMenu(ActiveMenu);
            ActiveMenu = menu;
            foreach (var menuItem in Labels[menu])
            {
                menu.ElementNode.Add(menuItem.ElementNode);
                menuItem.Key = $"show_{menuItem.Id}";
            }
            
            menu.SurrogateReference?.ChangeState();
        }

        public string GetJoinedMenus() => string.Join(" ", Menus.Select(e => e.Id));

        public MenuPart CreateMenu(string menuLabel, StyleOperator styleOp) 
        {
            var hold = new MenuPart(id++);

            hold.WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(styleOp, hold, 
                ("grid-area",hold.Id)
                );
            
            hold.SetHtml(menuLabel);
            Menus.Add(hold);
            if (!Labels.ContainsKey(hold))
            {
                Labels.Add(hold, new());
            }

            hold.AddEvent("onmouseenter", o =>
            {
                OpenMenu(hold);
            });
            
            hold.AddEvent("onmouseleave", o =>
            {
                CloseMenu(hold);
            });
            
            hold.AddEvent("onclick", o =>
            {
                CloseMenu(hold);
            });
            
            hold.SurrogateReference?.ChangeState();
            
            return hold;
        }

        public MenuItem CreateItem(MenuPart menu, string item)
        {
            var hold = new MenuItem(id++);
            hold.SetHtml(item);
            AddItemToMenu(menu, hold);
            menu.SurrogateReference?.ChangeState();
            return hold;
        }

        public void AddItemToMenu(MenuPart menu, MenuItem item)
        {
            if (item.ElementNode.Parent is not null) 
                Labels[item.ElementNode.Parent.Value as MenuPart].Remove(item);
            Labels[menu].Add(item);
        }
        
        


        public TabData DragItem = null;
        public MenuItem TabItem = null;
        public void StartItemDrag(TabData tabData, MenuItem tabItem)
        {
            DragItem = tabData;
            TabItem = tabItem;
        }

        public void DroppedTabItem(MenuPart menu, WindowingService windowingService, TabData tabData, MenuItem target)
        {
            EmptyLink.Add(TabItem.ElementNode);
            target.ElementNode.InsertAfter(TabItem.ElementNode);
            List<MenuItem> items = Labels[menu];
            items.Remove(TabItem);
            items.Insert(items.IndexOf(target)+1, TabItem);
            
            if (DragItem is null || tabData.TabGroup == DragItem.TabGroup) return;
            
            string previous = DragItem.TabGroup;
            
            WindowContext oldWindow =
                windowingService.TabService.ObjectTabMap.FirstOrDefault(item => item.Value == DragItem.TabGroup).Key as WindowContext;

            WindowContext window =
                windowingService.TabService.ObjectTabMap.FirstOrDefault(item => item.Value == tabData.TabGroup).Key as WindowContext;

            windowingService.AddTabToWindow(window, DragItem);
            windowingService.TabService.InsertTab(DragItem, tabData, true);
            windowingService.UpdateTabs(previous);
            windowingService.UpdateTabs(tabData.TabGroup);
            
            window?.TabSection.SurrogateReference?.ChangeState();
            oldWindow?.TabSection.SurrogateReference?.ChangeState();
            if (oldWindow is not null && windowingService.TabService.TabGroupMap[previous].Group.Count is 0) windowingService.RemoveWindow(oldWindow);
            
        }

        public void EndItemDrag()
        {
            DragItem = null;
            TabItem = null;
        }
        
    }

    public class MenuPart : ElementContext, INodeElement
    {
        public LinkMember ElementNode { get; }
        public MenuPart(int id) : base($"MenuPart_{id}")
        {
            cssClass = "toolbar-menu-part";
            ElementNode = new LinkMember(this);
        }
    }

    public class MenuItem : ElementContext, INodeElement
    {
        public LinkMember ElementNode { get; set; }
        public MenuItem(int id) : base($"MenuItem_{id}")
        {
            cssClass = "toolbar-menu-item";
            ElementNode = new LinkMember(this);
        }
    }
}