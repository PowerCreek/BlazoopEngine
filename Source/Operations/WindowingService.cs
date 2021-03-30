using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Blazoop.Source.ElementContexts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Blazoop.Source.Operations
{
    public class WindowingService : OperationBase
    {
        
        public ContainerContext ContainerContext { get; set; }

        public Dictionary<WindowContext, WindowContext> WindowContextMap = new();
        //public Dictionary<TabContext, TabContext> TabContextMap = new();
        public List<WindowContext> WindowRenderOrder = new();
        
        public WindowingService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
        }

        public WindowContext CreateWindow
        {
            get
            {
                WindowContext windowContext = new WindowContext(ContainerContext.NodeBase);
                ContainerContext.SlideMember.Add(RegisterWindow(windowContext).ElementNode);
                ContainerContext.SurrogateReference?.ChangeState();
                return windowContext;
            }
        }

        public void RemoveWindow(WindowContext context)
        {
            WindowContextMap.Add(context, context);
            WindowRenderOrder.Remove(context);
            context.ElementNode.Pop();
            ContainerContext.SurrogateReference?.ChangeState();
        }

        public WindowContext RegisterWindow(WindowContext context)
        {
            WindowToFront(context);
            WindowContextMap.Add(context, context);
            return context;
        }
        
        public void WindowToFront(WindowContext context)
        {
            WindowRenderOrder.Remove(context);
            int index = 1;
            foreach (var item in WindowRenderOrder.Where(item => item != context))
            {
                item.WithAttribute("Style", out StyleContext styleContext);
                styleContext.WithStyle(context.StyleOperator, item, ("z-index",$"{index++}"));
            }

            WindowRenderOrder.Add(context);
            context.WithAttribute("Style", out StyleContext styleContext2);
            styleContext2.WithStyle(context.StyleOperator, context, ("z-index",$"{index}"));
        }
        
        
        public WindowContext WindowDraggingWithTitlebar { get; set; }
        public WindowContext WindowDraggingWithResize { get; set; }
        public int StartWheelPos = 0;

        
        public void ContainerWheel(object args)
        {
            WheelEventArgs wArgs = args as WheelEventArgs;
            int amount = - Math.Sign(wArgs.DeltaY) * 25;
            
            ContainerContext.SliderTransform.Position.Y += amount;
            StartWheelPos += amount;
            
            if (WindowDraggingWithTitlebar == null && WindowDraggingWithResize == null) return;

            if (WindowDraggingWithTitlebar != null)
            {
                WindowDraggingWithTitlebar.Transform.Position.Y = 
                    WindowDraggingWithTitlebar.Transform.Position.Y + Math.Sign(wArgs.DeltaY)*25;
            }
        }
    }
    
}