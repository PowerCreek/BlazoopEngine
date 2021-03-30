using System;
using Microsoft.JSInterop;

namespace Blazoop.ExternalDeps.Classes.Management.Operations
{
    public class OperationBase : IMakeOperator
    {
        public OperationBase(IJSRuntime jsRuntime)
        {
            JsRuntime = jsRuntime;
        }

        public OperationBase()
        {
        }

        public IJSRuntime JsRuntime { get; init; }

        public virtual void MakeOperator()
        {
            Console.WriteLine(nameof(OperationBase));
        }
    }
}