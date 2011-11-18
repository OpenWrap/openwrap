using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Build;
using OpenWrap.Build.PackageBuilders;
using OpenWrap.Commands.Wrap;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Testing;

namespace Tests.Commands.build_wrap.assembly_info
{
    public class specified : command<BuildWrapCommand>
    {
        public specified()
        {
            given_descriptor(
                "name: sauron",
                "version: 1.0.0",
                "build: custom;typeName=" + typeof(NullPackageBuilderWithProperties).AssemblyQualifiedName,
                "assembly-info: assembly-version");
            when_executing_command();
        }
        [Test]
        public void assembly_file_is_generated()
        {
            NullPackageBuilderWithProperties._properties.Contains("OpenWrap-SharedAssemblyInfoFile").ShouldBeTrue();

        }
    }
    public class not_specified : command<BuildWrapCommand>
    {
        public not_specified()
        {
            given_descriptor(
                "name: sauron",
                "version: 1.0.0",
                "build: custom;typeName=" + typeof(NullPackageBuilderWithProperties).AssemblyQualifiedName);
            when_executing_command();
        }
        [Test]
        public void assembly_file_is_generated()
        {
            NullPackageBuilderWithProperties._properties.Contains("OpenWrap-SharedAssemblyInfoFile").ShouldBeFalse();

        }
    }
    public class NullPackageBuilderWithProperties : IPackageBuilder
    {
        public static ILookup<string, string> _properties;
        public ILookup<string, string> Properties { get { return _properties; } set { _properties = value; } }
        public IEnumerable<BuildResult> Build()
        {
            yield break;
        }
    } 
}