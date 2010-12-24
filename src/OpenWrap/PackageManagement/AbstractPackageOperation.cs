using System.Collections;
using System.Collections.Generic;

namespace OpenWrap.PackageManagement
{
    public abstract class AbstractPackageOperation : IEnumerable<PackageOperationResult>
    {
        readonly IEnumerable<PackageOperationResult> _root;

        public AbstractPackageOperation(IEnumerable<PackageOperationResult> root)
        {
            _root = root;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PackageOperationResult>)this).GetEnumerator();
        }

        IEnumerator<PackageOperationResult> IEnumerable<PackageOperationResult>.GetEnumerator()
        {
            foreach (var value in _root)
                yield return value;
        }
    }
}