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
        
        public TitlebarContext(IRootElement nodeBase) : base($"Title_{nodeBase.NodeBase.Id}_")
        {
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            Add("node", ElementNode = new LinkMember(this));
            
            WithAttribute("style", out StyleContext sliderStyle);
            sliderStyle.WithStyle(StyleOperator, this, 
                ("background-color","black"),
                ("top","0px"),
                ("left","0px"),
                ("place-self","stretch"),
                ("position","relative"));

            AddEvent("onmousedown", OnMouseDown);
            AddEvent("onmouseup", OnMouseUp);
            
            SetHtml("what");
        }
        
        public Action<dynamic> TitlebarMouseDown { get; set; } 
        
        public Action<dynamic> TitlebarMouseUp { get; set; }

        public void OnMouseDown(object args) => TitlebarMouseDown?.Invoke(args);
        public void OnMouseUp(object args) => TitlebarMouseUp?.Invoke(args);
    }
}