using System;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement
{
    public abstract class PackageOperationResult
    {
        public abstract bool Success { get; }

        public abstract ICommandOutput ToOutput();
    }
    public class PackageHookResult : PackageOperationResult, ICommandOutput
    {
        readonly object _output;

        public PackageHookResult(object output)
        {
            _output = output;
        }

        public override bool Success { get { return true; } }

        public override ICommandOutput ToOutput()
        {
            return this;
        }

        public ICommand Source
        {
            get { return null; }
        }

        public CommandResultType Type
        {
            get { return CommandResultType.Info; }
        }
        public override string ToString()
        {
            return _output.ToString();
        }
    }
}