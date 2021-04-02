using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Blazoop.Source.Operations;
using Microsoft.JSInterop;

namespace Blazoop.Source.ElementContexts
{
    public class TabService
    {

        public const string UNJOINED = nameof(UNJOINED);
        
        public Dictionary<object, TabData> TabMap = new();

        public Dictionary<object, string> ObjectTabMap = new();

        public Dictionary<string, (List<TabData> Group, List<TabData> Order)> TabGroupMap = new();
        
        public (List<TabData> Group, List<TabData> Order) GetTabs(object obj) => TabGroupMap[ObjectTabMap[obj]];
        
        public void UnjoinTabGroup(object obj)
        {
            string guid = ObjectTabMap[obj];
            ObjectTabMap.Remove(obj);
            
            var tabs = TabGroupMap[guid];
            
            TabGroupMap.Remove(guid);
            if (!TabGroupMap.TryAdd(UNJOINED, tabs))
            {
                TabGroupMap[UNJOINED].Group.AddRange(tabs.Group);
                TabGroupMap[UNJOINED].Order.InsertRange(0, tabs.Order);
            }
        }

        public void CreateGroup(object obj) => ObjectTabMap.Add(obj, Guid.NewGuid().ToString());

        public void AddTabToGroup(string group, TabData tab)
        {
            if (tab.TabGroup is not null)
            {
                TabGroupMap[tab.TabGroup].Group.Remove(tab);
                TabGroupMap[tab.TabGroup].Order.Remove(tab);
            }

            if (!TabGroupMap.ContainsKey(group)) 
                TabGroupMap.Add(group, (new List<TabData>(), new List<TabData>()));
            
            TabGroupMap[group].Group.Add(tab);
            TabGroupMap[group].Order.Add(tab);
            
            tab.TabGroup = group;
            SetGridAreas(tab);
        }

        public void SetGridAreas(TabData tab)
        {
            if (tab.TabContext?.ElementNode.Parent?.Value is TabSectionContext section)
            {
                section.WithAttribute("style", out StyleContext sectionStyle);
                string areas = string.Join(" ", TabGroupMap[tab.TabGroup].Group.Select(e=>e.TabContext.Id));
                sectionStyle.WithStyle(section.StyleOperator, section,
                    ("grid-template-areas", $"\"{areas}\""));
            }
        }

        public void InsertTab(TabData placing, TabData existing, bool before)
        {
            var hold = TabGroupMap[existing.TabGroup].Group;
            hold.Remove(placing);
            hold.Insert(hold.IndexOf(existing)+(before?0:1), placing);
            SetGridAreas(placing);
        }
        
        public TabData CreateTab()
        {
            TabData data = new();
            TabMap.Add(data, data);
            AddTabToGroup(UNJOINED, data);
            return data;
        }

        public void SelectTab(TabData tab)
        {
            var items = TabGroupMap[tab.TabGroup].Order;
            items.Remove(tab);
            
            foreach (var item in items)
                item.SelectTab(false);
            
            items.Add(tab);
            tab.SelectTab(true);
            SetGridAreas(tab);
        }

    }
}