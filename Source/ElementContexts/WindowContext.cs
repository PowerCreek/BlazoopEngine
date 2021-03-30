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
        
        public TitlebarContext Titlebar { get; }
        public ElementContext TabSection { get; }
        public ElementContext ContentPane { get; }
        
        public LinkMember ElementNode { get; }
        
        public WindowContext(IRootElement nodeBase) : base($"Window_{nodeBase.NodeBase.Id}_")
        {

            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            
            Add("node", ElementNode = new LinkMember(this));
            
            Titlebar = new TitlebarContext(nodeBase);
            ElementNode.Add(Titlebar.ElementNode);
            
            TabSection = new ElementContext($"Tabs_{nodeBase.NodeBase.Id}");
            ContentPane = new ElementContext($"Content_{nodeBase.NodeBase.Id}");

            cssClass = "window-context";
            
            WithAttribute("style", out StyleContext styleContext);
            styleContext.WithStyle(StyleOperator, this, 
                ("position","absolute"), 
                ("top",$"{Transform.Position.X}px"),
                ("left",$"{Transform.Position.Y}px"),
                ("width",$"{Transform.Size.Width}px"),
                ("height",$"{Transform.Size.Height}px"),
                ("border","1px solid black"));
            
            Transform.OnMove = (transform, position) =>
            {
                
            };
            
            Transform.OnResize = (transform, position) =>
            {

            };
        }
    }

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
                ("background-color","gray"),
                ("top","0px"),
                ("left","0px"),
                ("place-self","stretch"),
                ("position","relative"));

            SetHtml("what");
        }
    }
    
}