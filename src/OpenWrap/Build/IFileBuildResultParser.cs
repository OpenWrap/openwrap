using System.Collections.Generic;

namespace OpenWrap.Build
{
    public interface IFileBuildResultParser
    {
        IEnumerable<FileBuildResult> Parse(string line);
    }
}