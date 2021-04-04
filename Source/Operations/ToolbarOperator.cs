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
        
        public StyleOperator StyleOp { get; set; }

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
            
            menu.WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOp, menu, 
                ("background-color","none"),
                ("border","2px solid transparent"));
            
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
            
            menu.WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOp, menu, 
                ("background-color","#0a0a0a33"),
                ("border","2px solid gray"));
            
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
            if (tabData == DragItem) return;
            
            EmptyLink.Add(TabItem.ElementNode);

            Labels.Values.FirstOrDefault(e => e.Contains(TabItem));
            
            List<MenuItem> items = Labels[menu];

            int item1 = items.IndexOf(TabItem);
            int item2 = items.IndexOf(target);

            items?.Remove(TabItem);

            if (item2 is not -1)
            {
                if (item1 < item2)
                {
                    item1 = item2+1;
                }
                else
                    item1 = item2;
            }


            WindowContext window =
                windowingService.TabService.ObjectTabMap.FirstOrDefault(item => item.Value == tabData.TabGroup).Key as WindowContext;

            if (DragItem is null || tabData.TabGroup == DragItem.TabGroup)
            {
                windowingService.UpdateTabs(tabData.TabGroup);
                window?.TabSection.SurrogateReference?.ChangeState();
                windowingService.TabService.InsertTab(DragItem, tabData, item1 <= item2);
                OpenMenu(menu);
                return;
            }
            
            string previous = DragItem.TabGroup;
            
            WindowContext oldWindow =
                windowingService.TabService.ObjectTabMap.FirstOrDefault(item => item.Value == DragItem.TabGroup).Key as WindowContext;

            windowingService.RemoveTabFromWindow(DragItem);
            windowingService.AddTabToWindow(window, DragItem);
            
            if (oldWindow is not null && windowingService.TabService.TabGroupMap[previous].Group.Count is 0)
            {
                windowingService.RemoveWindow(oldWindow);
                previous = TabService.UNJOINED;
            }

            windowingService.TabService.InsertTab(DragItem, tabData, item1 < item2);

           if (previous is not TabService.UNJOINED)
           { 
               windowingService.UpdateTabs(previous);
           }

           if (window?.TabSection is not null)
           {
               window.Key = Guid.NewGuid().ToString();
               window.TabSection.Key = Guid.NewGuid().ToString();
               foreach (var linkMember in window.TabSection.ElementNode.GetChildren())
               {
                   (linkMember.Value as ElementContext).Key = Guid.NewGuid().ToString();
               }
           }

            window?.TabSection.SurrogateReference?.ChangeState();
            oldWindow?.TabSection.SurrogateReference?.ChangeState();
            OpenMenu(menu);
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