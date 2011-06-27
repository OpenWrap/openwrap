using System;
using Mono.Cecil;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.Exporters
{
    public class SolutionPlugin : Exports.ISolutionPlugin
    {
        readonly TypeDefinition _type;

        public SolutionPlugin(IPackage package, string path, TypeDefinition type)
        {
            _type = type;
            Package = package;
            Path = path;
            Name = type.Name;
        }

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

        public bool Equals(SolutionPlugin other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._type.FullName, _type.FullName);
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
            return (_type != null ? _type.GetHashCode() : 0);
        }
    }
}