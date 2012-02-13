using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Collections.Generic;
using OpenWrap.Collections;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class LoggingPackageResolver : PackageResolverVisitor<IPackageInfo>
    {
        public LoggingPackageResolver(
            IEnumerable<IPackageInfo> allPackages,
            IEnumerable<IPackageInfo> success = null,
            IEnumerable<IPackageInfo> fail = null,
            Action<IPackageInfo, Func<IPackageInfo, bool>, IPackageInfo, bool> arcTraversal = null)
            : base(allPackages, null, PackageStrategy.Latest, success, fail, arcTraversal)
        {
            DependencyReader = ReadDependencies;
            if (arcTraversal == null)
            {
                Traversal = LoggingTraversal;
            }
            else
            {
                Traversal = (@in, dep, @out, result) =>
                {
                    LoggingTraversal(@in, dep, @out, result);
                    arcTraversal(@in, dep, @out, result);
                };
            }
        }

        Dictionary<Func<IPackageInfo, bool>, PackageDependency> _dependencyMap;

        IEnumerable<Func<IPackageInfo, bool>> ReadDependencies(IPackageInfo arg)
        {
            return arg.Dependencies.Select(ReadDependency);
        }
        public virtual bool Visit(IEnumerable<PackageDependency> dependencies)
        {
            return Visit(dependencies.Select(ReadDependency));
        }

        Func<IPackageInfo, bool> ReadDependency(PackageDependency packageDependency)
        {
            var matcher = ToFunc(packageDependency);
            if (_dependencyMap.ContainsKey(matcher) == false)
                _dependencyMap[matcher] = packageDependency;
            return matcher;
        }

        static Func<IPackageInfo, bool> ToFunc(PackageDependency packageDependency)
        {
            return package => packageDependency.IsFulfilledBy(package.SemanticVersion);
        }

        public override bool Visit(IEnumerable<Func<IPackageInfo, bool>> dependencies)
        {
            _dependencyMap = new Dictionary<Func<IPackageInfo, bool>, PackageDependency>();
            NotFound = new List<CallStack>();
            Success = new List<CallStack>();
            Fail = new List<CallStack>();

            return base.Visit(dependencies);
        }

        public List<CallStack> Fail { get; set; }

        protected override bool VisitPackage(IPackageInfo package)
        {
            Push(package.Identifier);
            var success = base.VisitPackage(package);
            Pop();
            return success;
        }
        void LoggingTraversal(IPackageInfo package, Func<IPackageInfo, bool> dependency, IPackageInfo destination, bool success)
        {
            if (!success && destination != null)
                Fail.Add(new CallStack(_currentNode));
            else if (!success)
                NotFound.Add(new CallStack(_currentNode));
            else
                Success.Add(new CallStack(_currentNode));
        }

        public List<CallStack> Success { get; private set; }
        protected override bool VisitDependency(IPackageInfo package, Func<IPackageInfo, bool> dependency)
        {
            Push(_dependencyMap[dependency]);
            var success = base.VisitDependency(package, dependency);
            Pop();
            return success;
        }

        Stack<Node> _currentNode = new Stack<Node>();
        public List<CallStack> NotFound { get; private set; }


        void Pop()
        {
            _currentNode.Pop();
        }

        void Push(PackageIdentifier identifier)
        {
            _currentNode.Push(new PackageNode(identifier));
            WriteDebug("P:" + identifier);
        }

        void Push(PackageDependency dependency)
        {
            _currentNode.Push(new DependencyNode(dependency));
            WriteDebug("D:" + dependency);
        }

        void WriteDebug(string text)
        {
            var packages = string.Empty;
            ////string.Format(
            ////     " Yes: {0} No:{1}",
            ////     SelectionContext.CompatiblePackageVersions.Select(x => x.Key).JoinString(","),
            ////     SelectionContext.IncompatiblePackageVersions.Select(x => x.Key).JoinString(","));

            Debug.WriteLine(new string(' ', _currentNode.Count) + text + packages);
        }

    }
    public static class PackageStrategy
    {
        public static IEnumerable<IPackageInfo> Latest(IEnumerable<IPackageInfo> arg)
        {
#pragma warning disable 612,618
            return arg.Where(_ => _.SemanticVersion != null)
                .OrderByDescending(_ => _.SemanticVersion)
                .Concat(
                    arg.Where(_ => _.SemanticVersion == null && _.Version != null)
                        .OrderBy(_ => _.Version)
                );
#pragma warning restore 612,618
        }
    }
    public class PackageResolverVisitor<T> where T : class
    {
        readonly Func<IEnumerable<T>, IEnumerable<T>> _strategy;
        readonly IEnumerable<T> _allPackages;
        public Func<T, IEnumerable<Func<T, bool>>> DependencyReader { get; set; }
        public Action<T, Func<T, bool>, T, bool> Traversal { get; set; }
        public ICollection<T> SuccessfulPackages { get; set; }
        public ICollection<T> IncompatiblePackages { get; set; }

        public PackageResolverVisitor(
            IEnumerable<T> allPackages,
            Func<T, IEnumerable<Func<T, bool>>> dependencyReader,
            Func<IEnumerable<T>, IEnumerable<T>> strategy,
            IEnumerable<T> success = null,
            IEnumerable<T> fail = null,
            Action<T, Func<T, bool>, T, bool> arcTraversal = null)
        {
            _strategy = strategy;
            _allPackages = allPackages;
            DependencyReader = dependencyReader;
            Traversal = arcTraversal;
            SuccessfulPackages = success.CopyOrNew();
            IncompatiblePackages = fail.CopyOrNew();
        }


        public virtual bool Visit(IEnumerable<Func<T, bool>> dependencies)
        {
            return dependencies.All(info => VisitDependency(null, info));
        }

        protected virtual bool VisitDependency(T package, Func<T, bool> dependency)
        {
            if (SuccessfulPackages.Any(dependency))
                return true;

            var matchingPackages =
                _allPackages
                .Where(info => IncompatiblePackages.Contains(info) == false)
                .Where(dependency);
            if (matchingPackages.Any() == false)
            {
                InvokeTraversal(package, dependency, null, false);
                return false;
            }
            foreach (var matchingPackage in matchingPackages)
            {
                var success = VisitPackage(matchingPackage);
                InvokeTraversal(package, dependency, matchingPackage, success);
                if (success) return true;
            }
            return false;
        }

        protected void InvokeTraversal(T package, Func<T, bool> dependency, T destination, bool success)
        {
            if (Traversal == null) return;
            Traversal(package, dependency, destination, success);
        }

        protected virtual bool VisitPackage(T package)
        {
            var newResolver = new PackageResolverVisitor<T>(_allPackages, DependencyReader, _strategy, SuccessfulPackages, IncompatiblePackages, Traversal);
            newResolver.SuccessfulPackages.Add(package);
            if (newResolver.Visit(DependencyReader(package)))
            {
                SuccessfulPackages = newResolver.SuccessfulPackages;
                IncompatiblePackages = newResolver.IncompatiblePackages;
                return true;
            }

            IncompatiblePackages.Add(package);

            return false;
        }
    }

}