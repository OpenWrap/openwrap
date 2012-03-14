using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Parsers;
using OpenWrap.Testing;

namespace Tests.Dependencies.parser
{
    public abstract class dependency_parser_context : context
    {
        protected PackageDependency Declaration { get; set; }

        public void given_dependency(string dependencyLine)
        {
            var target = new PackageDescriptor();
            ((IPackageDescriptor)target).Dependencies.Add(DependsParser.ParseDependsLine(dependencyLine));
            Declaration = target.Dependencies.First();
        }
    }
}