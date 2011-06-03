using System;
using System.Collections.Generic;
using OpenWrap.Collections;

namespace OpenWrap.Commands
{
    public abstract class AbstractCommand : ICommand
    {
        public IEnumerable<ICommandOutput> Execute()
        {
            foreach (var validator in Validators())
            {
                bool errorDetected = false;
                foreach (var output in validator().NotNull())
                {
                    yield return output;
                    if (output.Type == CommandResultType.Error)
                        errorDetected = true;
                }
                if (errorDetected)
                    yield break;
            }
            foreach (var output in ExecuteCore().NotNull())
                yield return output;
        }

        protected abstract IEnumerable<ICommandOutput> ExecuteCore();

        protected virtual IEnumerable<Func<IEnumerable<ICommandOutput>>> Validators()
        {
            yield break;
        }
    }
}