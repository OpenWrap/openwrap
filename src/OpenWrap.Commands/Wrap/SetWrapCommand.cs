using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "set", Noun = "wrap")]
    public class SetWrapCommand : WrapCommand
    {
        bool? _content;
        bool? _anchored;

        [CommandInput(IsRequired = true, Position = 0)]
        public string Name { get; set; }

        [CommandInput]
        public bool Content
        {
            get { return _content ?? false; }
            set { _content = value; }
        }

        [CommandInput]
        public bool Anchored
        {
            get { return _anchored ?? false; }
            set { _anchored = value; }
        }

        [CommandInput]
        public string Version { get; set; }

        [CommandInput]
        public string MinVersion { get; set; }

        [CommandInput]
        public string MaxVersion { get; set; }

        [CommandInput]
        public bool AnyVersion { get; set; }

        public override IEnumerable<ICommandOutput> Execute()
        {
            return Either(ValidateInputs()).Or(ExecuteCore());    
        }

        PackageDependency dependency;

        PackageDependency FindDependencyByName()
        {
            return Environment.Descriptor.Dependencies.FirstOrDefault(d => d.Name.EqualsNoCase(Name));
        }

        IEnumerable<ICommandOutput> ValidateInputs()
        {
            dependency = FindDependencyByName();
            if (dependency == null)
            {
                yield return new Error("Dependency not found: " + Name);
                yield break;
            }

            var gotVersion = Version != null;
            var gotMinVersion = MinVersion != null;
            var gotMaxVersion = MaxVersion != null;
            var numberOfVersionInputTypes = (new[] { gotVersion, (gotMinVersion || gotMaxVersion), AnyVersion }).Count(v => v);

            if (numberOfVersionInputTypes > 1)
            {
                yield return new Error("Arguments for 'version', 'version boundaries' and 'anyVersion' cannot be combined.");
                yield break;
            }

            if (gotVersion && Version.ToVersion() == null)
            {
                yield return new Error("Could not parse version: " + Version);
                yield break;
            }

            if (gotMinVersion && MinVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse minversion: " + Version);
                yield break;
            }

            if (gotMaxVersion && MaxVersion.ToVersion() == null)
            {
                yield return new Error("Could not parse maxversion: " + Version);
                yield break;
            }

        }

        IEnumerable<ICommandOutput> ExecuteCore()
        {
            UpdatedDependency(dependency);
            RewriteDescriptorFile();
            yield break;
        }

        void UpdatedDependency(PackageDependency dependency)
        {
            if (_content.HasValue)
            {
                dependency.ContentOnly = _content.Value;
            }
            if (_anchored.HasValue)
            {
                dependency.Anchored = _anchored.Value;
            }
            if (SomeVersionInputGiven)
            {
                dependency.VersionVertices.Clear();
            }
            if (AnyVersion)
            {
                dependency.VersionVertices.Add(new AnyVersionVertex());
            }
            if (Version != null)
            {
                dependency.VersionVertices.Add(new ExactVersionVertex(Version.ToVersion()));
            }
            if (MinVersion != null)
            {
                dependency.VersionVertices.Add(new GreaterThanVersionVertex(MinVersion.ToVersion()));
            }
            if (MaxVersion != null)
            {
                dependency.VersionVertices.Add(new LessThanVersionVertex(MaxVersion.ToVersion()));
            }
        }

        bool SomeVersionInputGiven
        {
            get
            {
                return AnyVersion || (new[] { Version, MinVersion, MaxVersion }).Any(v => v != null);
            }
        }

        void RewriteDescriptorFile()
        {
            using (var destinationStream = Environment.DescriptorFile.OpenWrite())
            {
                new PackageDescriptorReaderWriter().Write(Environment.Descriptor, destinationStream);
            }
        }
    }
}
