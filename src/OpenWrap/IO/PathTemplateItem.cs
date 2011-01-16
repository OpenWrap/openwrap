using System.Collections.Generic;
using OpenFileSystem.IO;

namespace OpenWrap.IO
{
    public class PathTemplateItem<T> where T:IFileSystemItem
    {
        public PathTemplateItem(T item, IDictionary<string, string> parameters)
        {
            Item = item;
            Parameters = parameters;
        }

        public T Item { get; private set; }
        public IDictionary<string, string> Parameters { get; private set; }
    }
}