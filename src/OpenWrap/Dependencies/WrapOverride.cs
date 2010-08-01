using System;

namespace OpenWrap.Dependencies
{
    public class WrapOverride
    {
        public WrapOverride(string oldPackage, string newPackage)
        {
            if (string.IsNullOrEmpty(oldPackage)) throw new ArgumentException("oldPackage cannot be empty or null.", "oldPackage");
            if (string.IsNullOrEmpty(newPackage)) throw new ArgumentException("newPackage cannot be empty or null.", "newPackage");
            OldPackage = oldPackage;
            NewPackage = newPackage;
        }

        public string OldPackage { get; private set; }
        public string NewPackage { get; private set; }

        /// <summary>
        /// Applies the override, if it's relevant to the dependency, to produce a modified dependency.
        /// </summary>
        public WrapDependency Apply(WrapDependency dependency)
        {
            if (dependency.Name == OldPackage)
            {
                // TODO: Should we create a new WrapDependency instance instead?
                // Might be a good idea to make these objects immutable...
                dependency.Name = NewPackage;
            }
            return dependency;
        }
    }
}
