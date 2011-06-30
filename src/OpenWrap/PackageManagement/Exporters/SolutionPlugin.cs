using System;
using Mono.Cecil;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters
{
    public class SolutionPlugin : Exports.ISolutionPlugin, IEquatable<SolutionPlugin>
    {
        readonly TypeDefinition _type;

        public SolutionPlugin(IPackage package, string path, TypeDefinition type)
        {
            _type = type;
            Package = package;
            VersionIdentifier = type.FullName + ":" + type.Module.Name + ":" + package.Descriptor.Identifier;
            Path = path;
            Name = type.Name;
        }

        protected string VersionIdentifier { get; set; }

        public string Path { get; private set; }

        public IPackage Package { get; private set; }

        public string Name { get; private set; }

        public IDisposable Start()
        {
            var plugin = Activator.CreateInstanceFrom(_type.Module.FullyQualifiedName, _type.FullName).Unwrap();

            if (plugin is IDisposable)
                return (IDisposable)plugin;
            return new PluginWrapper(plugin);
        }

        public bool Equals(SolutionPlugin other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.VersionIdentifier, VersionIdentifier);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SolutionPlugin)) return false;
            return Equals((SolutionPlugin)obj);
        }

        public override int GetHashCode()
        {
            return VersionIdentifier.GetHashCode();
        }

        public static bool operator ==(SolutionPlugin left, SolutionPlugin right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SolutionPlugin left, SolutionPlugin right)
        {
            return !Equals(left, right);
        }

        class PluginWrapper : IDisposable
        {
            object _plugin;

            public PluginWrapper(object plugin)
            {
                _plugin = plugin;
            }

            public void Dispose()
            {
                _plugin = null;
            }
        }
    }
}