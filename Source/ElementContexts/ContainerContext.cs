using System;
using System.Threading.Tasks;
using Blazoop.ExternalDeps.Classes;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;
using Blazoop.Source.Properties;
using Blazoop.Source.Properties.Vector;

namespace Blazoop.Source.ElementContexts
{
    public class ContainerContext : ElementContext, INodeElement
    {
        public LinkMember ElementNode { get; }
        
        public WindowingService WindowingService { get; }
        
        public StyleOperator StyleOperator { get; }

        public ElementContext Slider { get; }

        public IRootElement NodeBase { get; }
        public LinkMember SlideMember { get; }
        public Transform SliderTransform { get; }
        
        public ContainerContext(IRootElement nodeBase) : base($"Container_{nodeBase.NodeBase.Id}_")
        {
            cssClass = "window-container";
            
            NodeBase = nodeBase;
            Add("node", ElementNode = new LinkMember(this));

            WithAttribute("draggable", out AttributeString drag);
            drag.Value = "false";
            
            WindowingService = nodeBase.ServiceData.OperationManager.GetOperation<WindowingService>();
            WindowingService.ContainerContext = this;
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            
            WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOperator, this,
                ("background-color", "red"),
                ("position", "relative"),
                ("touch-action","none"));

            Slider = new ElementContext("Slider");
            Slider.WithAttribute("style", out StyleContext sliderStyle);
            sliderStyle.WithStyle(StyleOperator, Slider, 
                ("top","0px"),
                ("left","0px"),
                ("position","absolute"));

            SlideMember = new LinkMember(Slider);
            ElementNode.Add(SlideMember);
            
            SliderTransform = new Transform();
            SliderTransform.OnMove = (transform1, position) =>
            {
                sliderStyle.WithStyle(StyleOperator, Slider, 
                    ("margin-top",$"{position.Y}px"));
            };
            
            string[] events = { 
            };
            
            StopPropagations.AddRange(events);
            StopPropagations.Add("onwheel");

            PreventDefaults.Add("ondragover");
            PreventDefaults.Add("ondrop");

            AddEvent("onmousedown", WindowingService.ContainerMouseDown);
            AddEvent("onmousemove", WindowingService.ContainerMouseMove);
            AddEvent("onmouseup", WindowingService.ContainerMouseUp);
            AddEvent("onmouseleave", WindowingService.ContainerMouseLeave);
            AddEvent("ondragend", WindowingService.ContainerMouseUp);
            AddEvent("onwheel", WindowingService.ContainerWheel);
            AddEvent("ondrop", WindowingService.OnContainerTabDropped);
            AddEvent("ondragover", a=>{  });
            
            var hold = WindowingService.CreateWindow;

            for (int i = 0; i < 4; i++)
            {
                var tab = WindowingService.CreateTab();
                tab.TabContext = new TabContext(nodeBase, tab) { };
                tab.TabContext.SetHtml($"TAB {i}");
                WindowingService.AddTabToWindow(hold, tab);
            }
        }
        
        public void SetCursor(string cursor)
        {
            WithAttribute("Style", out StyleContext styleContext);
            styleContext.Valid = true;
            styleContext.WithStyle(StyleOperator,
                this,
                ("cursor", cursor)
            );
        }
    }
}