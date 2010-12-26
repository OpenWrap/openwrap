using System;

namespace OpenWrap.PackageModel
{
    public class PackageNameOverride
    {
        public PackageNameOverride(string oldPackage, string newPackage)
        {
            if (string.IsNullOrEmpty(oldPackage)) throw new ArgumentException("oldPackage cannot be empty or null.", "oldPackage");
            if (string.IsNullOrEmpty(newPackage)) throw new ArgumentException("newPackage cannot be empty or null.", "newPackage");
            OldPackage = oldPackage;
            NewPackage = newPackage;
        }

        public string NewPackage { get; private set; }
        public string OldPackage { get; private set; }

        /// <summary>
        ///   Applies the override, if it's relevant to the dependency, to produce a modified dependency.
        /// </summary>
        public PackageDependency Apply(PackageDependency dependency)
        {
            if (dependency.Name.EqualsNoCase(OldPackage))
            {
                // TODO: Should we create a new PackageDependency instance instead?
                // Might be a good idea to make these objects immutable...
                return new PackageDependencyBuilder(dependency).Name(NewPackage);
            }
            return dependency;
        }

        public override string ToString()
        {
            return string.Format("override: {0} {1}", OldPackage, NewPackage);
        }
    }
}