using System;
using System.Collections.Generic;
using Microsoft.JSInterop;

namespace Blazoop.ExternalDeps.Classes.Management.Operations
{
    public class OperationManager
    {
        public IJSRuntime JsRuntime;

        public Dictionary<Type, OperationBase> OperationMap = new();

        public OperationManager(IJSRuntime jsRuntime)
        {
            JsRuntime = jsRuntime;
        }

        public T GetOperation<T>() where T : OperationBase
        {
            if (OperationMap.TryGetValue(typeof(T), out var obj)) return (T) obj;

            T hold;
            OperationMap.Add(typeof(T), hold = (T) Activator.CreateInstance(typeof(T), JsRuntime));
            //hold.MakeOperator();
            return hold;
        }
    }
}