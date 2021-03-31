using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;

namespace Blazoop.Source.ElementContexts
{
    public class ContentPaneContext : ElementContext, INodeElement
    {
        public StyleOperator StyleOperator { get; }
        public LinkMember ElementNode { get; }
        
        public ContentPaneContext(IRootElement nodeBase) : base($"Content_{nodeBase.NodeBase.Id}_")
        {
            
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            Add("node", ElementNode = new LinkMember(this));
            
            WithAttribute("style", out StyleContext contentStyle);
            contentStyle.WithStyle(StyleOperator, this, 
                ("background-color","lightblue"),
                ("top","0px"),
                ("left","0px"),
                ("place-self","stretch"),
                ("position","relative"));
            
            SetHtml("what");
        }
        
    }
}