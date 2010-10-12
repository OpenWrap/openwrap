using System;
using System.Collections.Generic;

namespace OpenWrap.Dependencies.Parsers
{
    class UseProjectRepositoryParser : AbstractDescriptorParser
    {
        public UseProjectRepositoryParser()
                : base("use-project-repository")
        {
        }

        protected override IEnumerable<string> WriteContent(PackageDescriptor descriptor)
        {
            // only return something if the default value of true is not used
            if (!descriptor.UseProjectRepository)
                yield return "false";
        }

        protected override void ParseContent(string content, PackageDescriptor descriptor)
        {
            bool useProjectRepository;
            if (Boolean.TryParse(content, out useProjectRepository))
                descriptor.UseProjectRepository = useProjectRepository;
        }
    }
}