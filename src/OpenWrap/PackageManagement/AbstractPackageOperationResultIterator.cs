using System;
using System.Collections;
using System.Collections.Generic;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    /// <summary>
    /// Provides a wrapper for iterating over results. Doesn't do much right now, will have more in the future when MemoizeAll has been added.
    /// </summary>
    public abstract class AbstractPackageOperationResultIterator : IEnumerable<PackageOperationResult>
    {
        IEnumerable<PackageOperationResult> _root;

        public AbstractPackageOperationResultIterator(IEnumerable<PackageOperationResult> root)
        {
            _root = root;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PackageOperationResult>)this).GetEnumerator();
        }

        IEnumerator<PackageOperationResult> IEnumerable<PackageOperationResult>.GetEnumerator()
        {
            foreach (var result in _root)
                yield return result;
        }
    }   
}