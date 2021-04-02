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
                new LinkMember(null).Add(menuItem.ElementNode);
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