using System;
using Blazoop.Source.NodeContexts;
using Blazoop.Source.Operations;

namespace Blazoop.Source.ElementContexts
{
    public class InfoBoxContent : TabContentContext{

        public InfoBoxContent(IRootElement nodeBase) : base(nodeBase)
        {
            WithAttribute("style", out StyleContext sliderStyle);
            sliderStyle.WithStyle(StyleOperator, this,
                ("background-color", "yellow"),
                ("top", "0px"),
                ("left", "0px"),
                ("width", "100%"),
                ("height", "100%"),
                ("position", "absolute"));
                //SetHtml(Id);
        }
}
}