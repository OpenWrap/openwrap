using System;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement
{
    public abstract class PackageOperationResult
    {
        public abstract bool Success { get; }

        public abstract ICommandOutput ToOutput();
    }
}