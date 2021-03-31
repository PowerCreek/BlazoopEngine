using Blazoop.ExternalDeps.Classes;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.Source.ElementContexts;
using Blazoop.Source.Operations;

namespace Blazoop.Source.NodeContexts
{
    public interface IRootElement
    {
        public ElementContext RootElement { get; init; }
        public IServiceData ServiceData { get; }
        
        public NodeBase NodeBase { get; }
    }
    
    public class RootNode : NodeBase, IRootElement
    {
        public ElementContext RootElement { get; init; }
        public LinkMember Node { get; set; }
        
        public IServiceData ServiceData { get; }
        public StyleOperator StyleOperator { get; }
        public NodeBase NodeBase => this;

        public RootNode(IServiceData serviceData)
        {
            ServiceData = serviceData;

            StyleOperator = serviceData.OperationManager.GetOperation<StyleOperator>();
            
            Node = (RootElement = new ContainerContext(this))
                .Get<LinkMember>("node");

            //setup first window
        }
    }
}