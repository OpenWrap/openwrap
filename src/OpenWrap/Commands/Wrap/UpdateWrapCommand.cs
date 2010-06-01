using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.Build.Services;
using OpenWrap.Commands.Core;
using OpenWrap.Dependencies;
using OpenWrap.Repositories;

namespace OpenWrap.Commands.Wrap
{
    [Command(Verb = "update", Noun = "wrap")]
    public class UpdateWrapCommand : ICommand
    {

        protected IEnvironment Environment
        {
            get { return WrapServices.GetService<IEnvironment>(); }
        }

        protected IPackageManager PackageManager
        {
            get { return WrapServices.GetService<IPackageManager>(); }
        }

        public IEnumerable<ICommandResult> Execute()
        {
            if (Environment.ProjectRepository != null)
                return UpdateProjectPackages();
            return UpdateUserPackages();
        }

        IEnumerable<ICommandResult> UpdateUserPackages()
        {
            //TODO: Obvious sin't it :)
            yield return new GenericError { Message = "Not supported" };
        }

        IEnumerable<ICommandResult> UpdateProjectPackages()
        {
            var packagesToCopy = from dependency in Environment.Descriptor.Dependencies
                                 let remotePackage = Environment.RemoteRepositories
                                     .Select(x => x.Find(dependency)).FirstOrDefault()
                                 where remotePackage != null
                                 select remotePackage;
            foreach(var packageToCopy in packagesToCopy)
            {
                if (!Environment.UserRepository.HasDependency(packageToCopy.Name, packageToCopy.Version))
                {
                    yield return new Result("Copying {0} to user repository", packageToCopy.FullName);
                    using(var stream = packageToCopy.Load().OpenStream())
                        Environment.UserRepository.Publish(packageToCopy.FullName, stream);
                }
                if (!Environment.ProjectRepository.HasDependency(packageToCopy.Name, packageToCopy.Version))
                {
                    yield return new Result("Copying {0} to project repository", packageToCopy.FullName);
                    using (var stream = packageToCopy.Load().OpenStream())
                        Environment.ProjectRepository.Publish(packageToCopy.FullName, stream);                    
                }
            }
        }
    }
    public static class PackageRepositoryExtensions
    {
        public static bool HasDependency(this IPackageRepository packageRepository, string name, Version version)
        {
            return packageRepository.Find(new WrapDependency { Name = name, VersionVertices = { new ExactVersionVertice(version) } }) != null;
        }
    }
}
