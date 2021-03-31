using System;
using Blazoop.ExternalDeps.Classes;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;
using Blazoop.Source.Properties;
using Blazoop.Source.Properties.Vector;

namespace Blazoop.Source.ElementContexts
{
    public interface INodeElement
    {
        public LinkMember ElementNode { get; }
    }

    public interface ITransform
    {
        public Transform Transform { get; }
    }
    
    public class WindowContext : ElementContext, INodeElement, ITransform
    {

        public StyleOperator StyleOperator { get; }

        public Transform Transform { get; } = new Transform()
        {
            Position = new Position(400,400),
            Size = new Size(200,400)
        };
        
        public TitlebarContext Titlebar { get; set; }
        public TabSectionContext TabSection { get; set;}
        public ContentPaneContext ContentPane { get; set;}
        
        public LinkMember ElementNode { get; }
        
        public WindowingService WindowingService { get; }
        
        public WindowContext(IRootElement nodeBase) : base($"Window_{nodeBase.NodeBase.Id}_")
        {

            WindowingService = nodeBase.ServiceData.OperationManager.GetOperation<WindowingService>();
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            
            Add("node", ElementNode = new LinkMember(this));
            
            InitTitlebar(nodeBase);
            InitTabSection(nodeBase);
            InitContentPane(nodeBase);

            cssClass = "window-context";
            
            WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOperator, this, 
                ("position","absolute"), 
                ("left",$"{Transform.Position.X}px"),
                ("top",$"{Transform.Position.Y}px"),
                ("width",$"{Transform.Size.Width}px"),
                ("height",$"{Transform.Size.Height}px"),
                ("border","2px solid black"));
            
            Transform.OnMove = (transform, position) =>
            {
                styleContext.WithStyle(StyleOperator, this,
                    ("left", $"{Transform.Position.X}px"),
                    ("top", $"{Transform.Position.Y}px"));
            };
            
            Transform.OnResize = (transform, size) =>
            {
                styleContext.WithStyle(StyleOperator, this,
                    ("width", $"{Transform.Size.Width}px"),
                    ("height", $"{Transform.Size.Height}px"));
            };
            
            AddEvent("onmousedown", OnMouseDown);
            AddEvent("onmouseup", WindowingService.WindowResizeUp);
            AddEvent("onmousemove", OnMouseMove);
            AddEvent("onmouseleave", OnMouseLeave);
            //AddEvent("ondrop", OnDrop);
            AddEvent("ondragover", a=>{});
        }

        public void InitTitlebar(IRootElement nodeBase)
        {
            Titlebar = new TitlebarContext(nodeBase);
            ElementNode.Add(Titlebar.ElementNode);
            
            Titlebar.TitlebarMouseDown = args=>
            {
                WindowingService.WindowTitleBarDown(args, this);
            };
            
            Titlebar.TitlebarMouseUp = args=>
            {
                WindowingService.WindowTitleBarUp(args, this);
            };
        }
        
        public void InitTabSection(IRootElement nodeBase)
        {
            TabSection = new TabSectionContext(nodeBase);
            ElementNode.Add(TabSection.ElementNode);
        }
        
        public void InitContentPane(IRootElement nodeBase)
        {
            ContentPane = new ContentPaneContext(nodeBase);
            ElementNode.Add(ContentPane.ElementNode);
        }
        
        public void OnMouseDown(dynamic args)
        {
            WindowingService.WindowToFront(this);
            WindowingService.WindowMouseDown(args, this);
        }
        
        public void OnMouseMove(dynamic args)
        {
            WindowingService.WindowMouseMove(args, this);
        }
        
        public void OnMouseLeave(dynamic args)
        {
            WindowingService.WindowMouseLeave(args);
        }
    }
}