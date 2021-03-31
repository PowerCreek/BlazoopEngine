using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.ExternalDeps.Interfaces;

namespace Blazoop.Source.Operations
{
    public class StyleContext : IRenderableItem, IAttribute
    {
        public bool Valid;

        public Dictionary<string, string> StyleMap { get; } = new();

        private string Output { get; set; }

        public object GetRenderOutput()
        {
            Valid = true;
            return Output;
        }

        public void WithStyle(StyleOperator styleOperator, string ElementId, params (string styleKey, string styleValue)[] styles)
        {
            foreach (var (key, value) in styles) WithStyle(styleOperator, ElementId, key, value);
            CreateOutput();
        }

        private void WithStyle(StyleOperator styleOperator, string ElementId, string key, string value)
        {
            if (!StyleMap.ContainsKey(key))
            {
                StyleMap.Add(key, value);

                if (!Valid) return;
                styleOperator.SetStyle(ElementId, (key, value));
            }
            else
            {
                if (StyleMap[key] != value && Valid) styleOperator.SetStyle(ElementId, (key, value));

                StyleMap[key] = value;
            }
        }

        public void WithoutStyles(StyleOperator styleOperator, ElementContext elementContext, params string[] styles)
        {
            foreach (string key in styles)
            {
                styleOperator.SetStyle(elementContext.ElementReference, key, "");
                StyleMap.Remove(key);
            }
            CreateOutput();
        }
        
        public void WithStyle(StyleOperator styleOperator, ElementContext elementContext,
            params (string, string)[] styles)
        {
            foreach (var (key, value) in styles) WithStyle(styleOperator, elementContext, key, value);
            
            CreateOutput();
        }

        private void WithStyle(StyleOperator styleOperator, ElementContext elementContext, string key, string value)
        {
            if (!StyleMap.ContainsKey(key))
            {
                StyleMap.Add(key, value);
                if (Equals(default, elementContext.ElementReference) || !Valid) return;
                styleOperator.SetStyle(elementContext.ElementReference, key, value);
                
            }
            else
            {
                if (!Equals(default, elementContext.ElementReference) && StyleMap[key] != value && Valid)
                    styleOperator.SetStyle(elementContext.ElementReference, key, value);

                StyleMap[key] = value;
            }
        }

        public string CreateOutput()
        {
            Output = string.Join(';', StyleMap.Select(e => $"{e.Key}: {e.Value}"));
            return Output;
            //style="background-color: blue;"
        }
    }
}