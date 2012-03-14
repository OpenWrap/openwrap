using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenWrap.PackageModel;

namespace OpenWrap.PackageManagement.DependencyResolvers
{
    public class LoggingPackageVisitor : PackageResolverVisitor<IPackageInfo>
    {
        Stack<Node> _currentNode = new Stack<Node>();
        Dictionary<PackageDependency, Func<IPackageInfo, bool?>> _depToFunc;
        Dictionary<Func<IPackageInfo, bool?>, PackageDependency> _funcToDep;

        public LoggingPackageVisitor(
            IEnumerable<IPackageInfo> allPackages, 
            Func<IEnumerable<IPackageInfo>, IEnumerable<IPackageInfo>> strategy, 
            IEnumerable<IPackageInfo> success = null, 
            IEnumerable<IPackageInfo> fail = null)
            : base(allPackages, null, strategy, success, fail)
        {
            DependencyReader = ReadDependencies;
            ReadDependency = ReadDependencyCore;
            _funcToDep = new Dictionary<Func<IPackageInfo, bool?>, PackageDependency>();
            _depToFunc = new Dictionary<PackageDependency, Func<IPackageInfo, bool?>>();
            NotFound = new List<CallStack>();
            Success = new List<CallStack>();
            Fail = new List<CallStack>();
        }

        public List<CallStack> Fail { get; set; }
        public List<CallStack> NotFound { get; private set; }
        public Func<PackageDependency, Func<IPackageInfo, bool?>> ReadDependency { get; set; }
        public List<CallStack> Success { get; private set; }

        public Func<IPackageInfo, bool?> ReadDependencyCore(PackageDependency packageDependency)
        {
            if (_depToFunc.ContainsKey(packageDependency))
                return _depToFunc[packageDependency];

            var matcher = ToFunc(packageDependency);
            _funcToDep[matcher] = packageDependency;
            _depToFunc[packageDependency] = matcher;
            return matcher;
        }

        public virtual bool Visit(IEnumerable<PackageDependency> dependencies)
        {
            return VisitCore(dependencies.Select(ReadDependency).ToList());
        }

        public override bool Visit(IEnumerable<Func<IPackageInfo, bool?>> dependencies)
        {
            return VisitCore(dependencies);
        }

        protected override PackageResolverVisitor<IPackageInfo> CreateNestedResolver(IEnumerable<IPackageInfo> allPackages, 
                                                                                     Func<IPackageInfo, IEnumerable<Func<IPackageInfo, bool?>>> dependencyReader, 
                                                                                     Func<IEnumerable<IPackageInfo>, IEnumerable<IPackageInfo>> strategy, 
                                                                                     ICollection<IPackageInfo> successfulPackages, 
                                                                                     ICollection<IPackageInfo> incompatiblePackages)
        {
            return new LoggingPackageVisitor(allPackages, Strategy, successfulPackages, incompatiblePackages)
            {
                _currentNode = new Stack<Node>(_currentNode.Reverse()), 
                _depToFunc = _depToFunc, 
                _funcToDep = _funcToDep, 
                DependencyReader = DependencyReader
            };
        }

        protected override void NestedPackageFail(PackageResolverVisitor<IPackageInfo> newResolver)
        {
            base.NestedPackageFail(newResolver);
            Fail.AddRange(((LoggingPackageVisitor)newResolver).Fail);
            NotFound.AddRange(((LoggingPackageVisitor)newResolver).NotFound);
        }

        protected override void NestedPackageSucceeds(PackageResolverVisitor<IPackageInfo> newResolver)
        {
            base.NestedPackageSucceeds(newResolver);

            Success.AddRange(((LoggingPackageVisitor)newResolver).Success);
            Fail.AddRange(((LoggingPackageVisitor)newResolver).Fail);
            NotFound.AddRange(((LoggingPackageVisitor)newResolver).NotFound);
        }

        protected override bool VisitDependency(IPackageInfo package, Func<IPackageInfo, bool?> dependency)
        {
            using (Push(_funcToDep[dependency]))
            {
                var existingPackage = SuccessfulPackages.FirstOrDefault(_ => dependency(_) == true);

                var depSuccess = base.VisitDependency(package, dependency);

                if (depSuccess && existingPackage != null)
                    using (Push(existingPackage.Identifier))
                        Success.Add(new CallStack(_currentNode));

                WriteDebug(depSuccess ? "D:OK" : "D:FAIL");
                return depSuccess;
            }
        }

        protected override bool VisitPackage(IPackageInfo package)
        {
            using (Push(package.Identifier))
            {
                var success = base.VisitPackage(package);
                (success ? Success : Fail).Add(new CallStack(_currentNode));
                WriteDebug(success ? "P:OK" : "P:FAIL");

                return success;
            }
        }

        protected override bool VisitPackages(IEnumerable<IPackageInfo> matchingPackages)
        {
            if (!matchingPackages.Any())
                NotFound.Add(new CallStack(_currentNode));
            return base.VisitPackages(matchingPackages);
        }

        static Func<IPackageInfo, bool?> ToFunc(PackageDependency packageDependency)
        {
            return package => package.Name.EqualsNoCase(packageDependency.Name)
                                  ? packageDependency.IsFulfilledBy(package.SemanticVersion)
                                  : (bool?)null;
        }

        void Clear()
        {
            _funcToDep.Clear();
            _depToFunc.Clear();
            NotFound.Clear();
            Success.Clear();
            Fail.Clear();
        }

        void Pop()
        {
            _currentNode.Pop();
        }

        IDisposable Push(PackageIdentifier identifier)
        {
            _currentNode.Push(new PackageNode(identifier));
            WriteDebug("P:" + identifier);
            return new ActionOnDispose(Pop);
        }

        IDisposable Push(PackageDependency dependency)
        {
            _currentNode.Push(new DependencyNode(dependency));
            WriteDebug("D:" + dependency);
            return new ActionOnDispose(Pop);
        }

        IEnumerable<Func<IPackageInfo, bool?>> ReadDependencies(IPackageInfo arg)
        {
            return arg.Dependencies.Select(ReadDependency).ToList();
        }

        bool VisitCore(IEnumerable<Func<IPackageInfo, bool?>> dependencies)
        {
            return base.Visit(dependencies);
        }

        void WriteDebug(string text)
        {
            string packages = // string.Empty;
                string.Format(
                    " Yes: {0} No:{1}", 
                    base.SuccessfulPackages.Select(x => x.Identifier).Distinct().JoinString(","), 
                    base.IncompatiblePackages.Select(x => x.Identifier).Distinct().JoinString(","));

            Debug.WriteLine(new string(' ', _currentNode.Count) + text + packages);
        }
    }
}