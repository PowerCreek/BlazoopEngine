using System;
using System.Collections.Generic;
using System.Diagnostics;
using Blazoop.ExternalDeps.Classes.Types.ActionCall;

namespace Blazoop.ExternalDeps.Classes.ElementProps
{
    public class ElementProperty<T> where T : class
    {
        public Dictionary<string, ActionCall> PropertyChangedMap = new();

        public ActionCall<T, V> GetPropertyActionCall<V>()
        {
            var name = new StackTrace().GetFrame(1)?.GetMethod()?.Name.Split('_')[1];
            if (PropertyChangedMap.TryGetValue(name ?? string.Empty, out var hold))
                return hold as ActionCall<T, V>;

            var action = Activator.CreateInstance<ActionCall<T, V>>();
            PropertyChangedMap.Add(name ?? string.Empty, action);

            return action;
        }
    }
}