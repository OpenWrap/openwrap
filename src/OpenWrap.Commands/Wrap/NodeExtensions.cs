using System.Collections.Generic;
using OpenWrap.PackageModel;

namespace OpenWrap.Commands.Wrap
{
    public static class NodeExtensions
    {
        public static void Render(this IEnumerable<Node> nodes, TreeRenderer renderer)
        {
            renderer.PrintChildren(nodes, node => node.Render(renderer));
        }
    }
}