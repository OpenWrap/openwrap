using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.ProjectModel.Drivers.File
{
    public static class ProjectConstants
    {
        public static readonly Guid VisualBasic = new Guid("F184B08F-C81C-45F6-A57F-5ABD9991F28F");
        public static readonly Guid VisualCSharp = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

        static readonly IDictionary<Guid, string> _types = new Dictionary<Guid, string>
        {
            { VisualCSharp, "csharp" },
            { VisualBasic, "vb" }
        };

        public static Guid GuidFromType(string type)
        {
            return _types.Where(x => x.Value == type).Select(x => x.Key).First();
        }

        public static string TypeFromGuid(Guid guid)
        {
            return _types[guid];
        }
    }
}