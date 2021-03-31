using System;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazoop.Source.Operations
{
    public class StyleOperator : OperationBase
    {
        public StyleOperator(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        public override void MakeOperator()
        {
            Console.WriteLine(nameof(StyleOperator));
        }

        private void SetStyle(string id, string styleKey, string value)
        {
            JsRuntime.InvokeVoidAsync("SetStyles", id, styleKey, value);
        }

        public void SetStyle(ElementReference elem, string key, string value)
        {
            JsRuntime.InvokeVoidAsync("SetStylesByReference", elem, key, value);
        }

        public void SetStyle(string id, params (string key, string val)[] args)
        {
            foreach (var (key, val) in args) SetStyle(id, key, val);
        }
    }
}