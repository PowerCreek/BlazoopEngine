using Blazoop.ExternalDeps.Interfaces;

namespace Blazoop.ExternalDeps.Classes
{
    public class AttributeString : IAttribute, IRenderableItem
    {
        public string Value { get; set; }
        public object GetRenderOutput() => Value;
    }
}