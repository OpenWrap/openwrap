using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Repositories
{
    //public class AggregatePackageRepository : IPackageRepository
    //{
    //    readonly IPackageRepository _localRepository;
    //    readonly IPackageRepository _userRepository;
    //    readonly IEnumerable<IPackageRepository> _remoteRepositories;

    //    public AggregatePackageRepository(IPackageRepository localRepository, IPackageRepository userRepository, IEnumerable<IPackageRepository> remoteRepositories)
    //    {
    //        _localRepository = localRepository;
    //        _userRepository = userRepository;
    //        _remoteRepositories = remoteRepositories;
    //        UnifyPackagesByName();
    //    }

    //    void UnifyPackagesByName()
    //    {
    //        PackagesByName = _localRepository.PackagesByName.SelectMany(x=>x).Concat(_userRepository.PackagesByName.SelectMany(i=>i)).ToLookup(x => x.Name);
    //    }

    //    public ILookup<string, IPackageInfo> PackagesByName { get; private set; }

    //    public IPackageInfo Find(WrapDependency dependency)
    //    {
    //        return _localRepository.Find(dependency)
    //               ?? CopyFromUserToLocalRepository(dependency)
    //                  ?? LoadFromRemoteRepository(dependency);
            
    //    }

    //    IPackageInfo LoadFromRemoteRepository(WrapDependency dependency)
    //    {
    //        var remotePackageInfo = _remoteRepositories.Select(x => x.Find(dependency)).Where(x => x != null).FirstOrDefault();
    //        if (remotePackageInfo == null)
    //            return null;

    //        // copy the package to the user repository first
    //        var package = remotePackageInfo.Load();
    //        throw new NotImplementedException();

    //    }

    //    IPackageInfo CopyFromUserToLocalRepository(WrapDependency dependency)
    //    {
    //        return null;
    //    }
    //}
}
