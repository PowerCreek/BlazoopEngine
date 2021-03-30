﻿using System;
using System.Collections.Generic;
using Blazoop.ExternalDeps.Classes.ElementProps;
using Blazoop.ExternalDeps.Interfaces;
using Blazoop.Razor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
// ReSharper disable CollectionNeverUpdated.Global

namespace Blazoop.ExternalDeps.Classes.Management
{

    public class ElementContext : ElementProperties
    {
        private static int _id;
        
        public string cssClass { get; set; }
        
        public ElementContext(string id) : base(id = $"{id}_{_id++}")
        {
        }
        
        public ElementReference ElementReference { get; set; }
        public Surrogate SurrogateReference { get; set; }
        
        public Dictionary<string, EventCallback> EventMap { get; } = new();
        public List<string> PreventDefaults { get; } = new();
        public List<string> StopPropagations { get; } = new();
        public RenderFragment HTML { get; set; }

        public void SetHtml(string html)
        {
            HTML = Surrogate.CreateElement(html);
        }

        public EventCallback GetEvent(string name) => EventMap.TryGetValue(name, 
            out EventCallback item) 
            ? item : default;
        
        public void AddEvent(string name, Action<object> action)
        {
            EventMap.TryAdd(name, EventCallback.Factory.Create(this, action));
        }

    }
}