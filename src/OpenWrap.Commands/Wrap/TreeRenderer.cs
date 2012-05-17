using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Wrap
{
    public class TreeRenderer
    {
        public string Content = string.Empty;
        public Stack<string> Prefix = new Stack<string>();
        string _currentPrefix;


        public TreeRenderer()
        {
            Prefix.Push(" ");

        }
        public void PrintLine(string text)
        {
            Content += Content.Length == 0 ? _currentPrefix + text : "\r\n" + _currentPrefix + text;
        }

        public void PrintChildren<T>(IEnumerable<T> nodes, Action<T> action)
        {
            var allNodes = nodes.ToList();
            if (allNodes.Count == 0) return;
            var oldPrefix = Prefix.Peek();

            _currentPrefix = oldPrefix + "├─";

            Prefix.Push(oldPrefix + (allNodes.Count > 1 ? "│ " : "  "));
            foreach (var node in allNodes.Take(allNodes.Count - 1))
            {
                _currentPrefix = oldPrefix + "├─";
                action(node);
            }
            Prefix.Pop();
            Prefix.Push(oldPrefix + "  ");
            _currentPrefix = oldPrefix + "└─";

            
            action(allNodes.Last());
            Prefix.Pop();
        }
    }
}