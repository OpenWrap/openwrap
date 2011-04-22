using System;

namespace OpenWrap.PackageManagement
{
    public static class PackageManagerExtensions
    {
        public static IDisposable Monitor(this IPackageManager manager, PackageChanged add  = null, PackageChanged remove = null, PackageUpdated update = null)
        {
            if (add != null) manager.PackageAdded += add;
            if (remove != null) manager.PackageRemoved += remove;
            if (update != null) manager.PackageUpdated += update;
            return new ActionOnDispose(() =>
            {
                if (add != null) manager.PackageAdded -= add;
                if (remove != null) manager.PackageRemoved -= remove;
                if (update != null) manager.PackageUpdated -= update;
            });
        }
    }
}