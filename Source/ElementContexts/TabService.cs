using System;
using System.Collections.Generic;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Microsoft.JSInterop;

namespace Blazoop.Source.ElementContexts
{
    public class TabService
    {
        
        public Dictionary<object, TabData> TabMap = new();

        public Dictionary<object, string> ObjectTabMap = new();

        public Dictionary<string, List<TabData>> TabGroupMap = new();
        
        public void UnjoinTabGroup(object obj)
        {
            string guid = ObjectTabMap[obj];
            ObjectTabMap.Remove(obj);
            
            var tabs = TabGroupMap[guid];
            
            TabGroupMap.Remove(guid);
            if (!TabGroupMap.TryAdd("unjoined", tabs))
            {
                TabGroupMap["unjoined"].AddRange(tabs);
            }
        }

        public void CreateGroup(object obj) => ObjectTabMap.Add(obj, Guid.NewGuid().ToString());
    }
}