using System;
using System.Collections.Generic;

namespace Blazoop.ExternalDeps.Classes.ElementProps
{
    public class ElementProperties
    {
        public Dictionary<string, IAttribute> AttributeMap = new();
        public Dictionary<string, object> ContextItemMap = new();

        public ElementProperties(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public ElementProperties Add(string name, object data)
        {
            ContextItemMap.Add(name, data);
            return this;
        }

        public T Get<T>(string name)
        {
            if (ContextItemMap.ContainsKey(name))
            {
                return (T) ContextItemMap[name];
            }

            Add(name, out T item);
            return item;
        }

        public ElementProperties Add<T>(string name, out T item)
        {
            ContextItemMap.Add(name, item = Activator.CreateInstance<T>());
            return this;
        }

        public ElementProperties WithAttribute<T>(string name, out T iAttribute) where T : IAttribute
        {
            if (AttributeMap.ContainsKey(name))
            {
                iAttribute = (T) AttributeMap[name];
            }
            else
            {
                AttributeMap.Add(name, iAttribute = Activator.CreateInstance<T>());
            }
            
            return this;
        }

        public void RemoveAttribute(string name)
        {
            AttributeMap.Remove(name);
        }
    }
}