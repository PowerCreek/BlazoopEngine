namespace Blazoop.ExternalDeps.Classes
{
    public class NodeBase
    {
        private static int _NodeIndex = 0;
        public static int NextNodalId => _NodeIndex++;
        
        public readonly int Id = NextNodalId;
    }
}