using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement
{
    public class HookedPackageOperationResults : IEnumerable<PackageOperationResult>
    {
        readonly string _repository;
        readonly IEnumerable<PackageOperationResult> _results;
        readonly HooksStore _hooks;
        readonly Func<IEnumerable<IPackageInfo>> _before;
        readonly Func<IEnumerable<IPackageInfo>> _after;

        public HookedPackageOperationResults(string repository, IEnumerable<PackageOperationResult> results, HooksStore hooks, Func<IEnumerable<IPackageInfo>> before, Func<IEnumerable<IPackageInfo>> after)
        {
            _repository = repository;
            _results = results;
            _hooks = hooks;
            _before = before;
            _after = after;
        }

        IEnumerator<PackageOperationResult> IEnumerable<PackageOperationResult>.GetEnumerator()
        {
            var resolvedBefore = _before();

            bool? success = null;
            foreach (var result in _results)
            {
                success = success ?? true;
                if (!result.Success)
                    success = false;
                yield return result;
            }
            if (success == true)
            {
                var resolvedAfter = _after();
                // TODO: process removes, then process adds and updates based on a dependency tree
                foreach (var output in GetRemoved(resolvedBefore, resolvedAfter, _hooks).Concat(GetAdded(resolvedBefore, resolvedAfter, _hooks)).Concat(GetUpdated(resolvedBefore, resolvedAfter, _hooks)))
                    yield return new PackageHookResult(output);
            }
        }
        IEnumerable<object> GetUpdated(IEnumerable<IPackageInfo> before, IEnumerable<IPackageInfo> after, HooksStore hooks)
        {
            var afterByName = after.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return from oldPackage in before
                   where afterByName.ContainsKey(oldPackage.Name)
                   let newPackage = afterByName[oldPackage.Name]
                   where newPackage.SemanticVersion != oldPackage.SemanticVersion
                   from output in hooks.Updated(_repository, newPackage.Name, oldPackage.SemanticVersion, newPackage.SemanticVersion, after)
                   select output;
        }

        IEnumerable<object> GetAdded(IEnumerable<IPackageInfo> before, IEnumerable<IPackageInfo> after, HooksStore hooks)
        {
            var beforeByName = before.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return from newPackage in after
                   where beforeByName.ContainsKey(newPackage.Name) == false
                   from output in hooks.Installed(_repository, newPackage.Name, newPackage.SemanticVersion, after)
                   select output;
        }
        IEnumerable<object> GetRemoved(IEnumerable<IPackageInfo> before, IEnumerable<IPackageInfo> after, HooksStore hooks)
        {
            var afterByName = after.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
            return from oldPackage in before
                   where afterByName.ContainsKey(oldPackage.Name) == false
                   from output in hooks.Removed(_repository, oldPackage.Name, oldPackage.SemanticVersion, before)
                   select output;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PackageOperationResult>)this).GetEnumerator();
        }
    }
}