using System;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;

namespace Blazoop.Source.ElementContexts
{
    public class TitlebarContext : ElementContext, INodeElement
    {        
        public StyleOperator StyleOperator { get; }
        public LinkMember ElementNode { get; }
        public WindowingService WindowingService { get; }
        
        public WindowContext WindowContext { get; set; }
        
        public TitlebarContext(IRootElement nodeBase) : base($"Title_{nodeBase.NodeBase.Id}_")
        {
            WindowingService = nodeBase.ServiceData.OperationManager.GetOperation<WindowingService>();
            cssClass = "window-titlebar";
            
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            Add("node", ElementNode = new LinkMember(this));
            
            WithAttribute("style", out StyleContext titlebarStyle);
            titlebarStyle.WithStyle(StyleOperator, this, 
                ("justify-items","end"),
                ("top","0px"),
                ("left","0px"));

            AddEvent("onmousedown", OnMouseDown);
            AddEvent("onmouseup", OnMouseUp);

            var minButton = AddControl("min");
            minButton.cssClass += " fas fa-minus";
            
            var exitButton = AddControl("exit");
            exitButton.cssClass += " fas fa-times";
            exitButton.AddEvent("onmousedown", o =>
            {
                WindowingService.RemoveWindow(WindowContext);
                //Console.WriteLine("clicked exit");
            });
        }

        public ElementContext AddControl(string name)
        {
            var hold = new ElementContext($"{Id}_{name}");
            hold.cssClass = "window-titlebar-control";
            LinkMember controlLink = new LinkMember(hold);
            ElementNode.Add(controlLink);
            return hold;
        }
        
        public Action<dynamic> TitlebarMouseDown { get; set; } 
        
        public Action<dynamic> TitlebarMouseUp { get; set; }

        public void OnMouseDown(object args) => TitlebarMouseDown?.Invoke(args);
        public void OnMouseUp(object args) => TitlebarMouseUp?.Invoke(args);
    }
}