using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;

namespace Blazoop.Source.ElementContexts
{
    public abstract class TabContentContext : ElementContext, INodeElement
    {
        public StyleOperator StyleOperator { get; }
        public LinkMember ElementNode { get; }

        protected TabContentContext(IRootElement nodeBase) : base($"TabContent_{nodeBase.NodeBase.Id}_")
        {
            StyleOperator = nodeBase.ServiceData.OperationManager.GetOperation<StyleOperator>();
            Add("node", ElementNode = new LinkMember(this));
            
            WithAttribute("style", out StyleContext sliderStyle);
            sliderStyle.WithStyle(StyleOperator, this,
                ("background-color","magenta"),
                ("top","0px"),
                ("left","0px"),
                ("width","100%"),
                ("height","100%"),
                ("position","absolute"));
            SetHtml("default");
        }

        public void Show(bool show)
        {
            WithAttribute("style", out StyleContext sliderStyle);
            if (show)
            {
                sliderStyle.WithoutStyles(StyleOperator, this, "display");
            }
            else
            {
                sliderStyle.WithStyle(StyleOperator, this,
                    ("display", "none"));
            }
        }
    }
}