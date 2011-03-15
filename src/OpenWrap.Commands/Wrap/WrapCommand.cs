using System;
using System.Collections.Generic;
using System.Linq;
using OpenFileSystem.IO;
using OpenWrap.Commands.Remote;
using OpenWrap.Configuration;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.DependencyResolvers;
using OpenWrap.PackageModel;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using OpenWrap.Runtime;

namespace OpenWrap.Commands.Wrap
{
    public abstract class WrapCommand : AbstractCommand
    {
        protected WrapCommand()
        {
            Successful = true;
        }
        public IEnvironment Environment
        {
            get { return Services.ServiceLocator.GetService<IEnvironment>(); }

        }
        protected IPackageManager PackageManager
        {
            get { return Services.ServiceLocator.GetService<IPackageManager>(); }
        }
        protected IPackageResolver PackageResolver
        {
            get { return Services.ServiceLocator.GetService<IPackageResolver>(); }
        }

        protected IPackageExporter PackageExporter
        {
            get { return Services.ServiceLocator.GetService<IPackageExporter>(); }
        }
        protected IPackageDeployer PackageDeployer
        {
            get { return Services.ServiceLocator.GetService<IPackageDeployer>(); }
        }
        protected IFileSystem FileSystem
        {
            get { return Services.ServiceLocator.GetService<IFileSystem>(); }
        }
        protected IConfigurationManager ConfigurationManager { get { return Services.ServiceLocator.GetService<IConfigurationManager>(); } }

        protected DependencyResolutionResult ResolveDependencies(PackageDescriptor packageDescriptor, IEnumerable<IPackageRepository> repos)
        {
            return PackageResolver.TryResolveDependencies(packageDescriptor, repos);
        }

        protected virtual ICommandOutput ToOutput(PackageOperationResult packageOperationResult)
        {
            var output = packageOperationResult.ToOutput();
            Successful = Successful && (output.Type != CommandResultType.Error);
            return output;
        }

        protected bool Successful { get; private set; }


        protected void TrySaveDescriptorFile(FileBased<IPackageDescriptor> targetDescriptor)
        {
            if (!Successful) return;
            using (var descriptorStream = targetDescriptor.File.OpenWrite())
                new PackageDescriptorReaderWriter().Write(targetDescriptor.Value, descriptorStream);
        }

        protected IEnumerable<ICommandOutput> HintRemoteRepositories()
        {
            yield return new Info("The list of configured repositories can be seen using the 'list-remote' command. The currently configured repositories are:");
            foreach(var m in ConfigurationManager.LoadRemoteRepositories()
                    .OrderBy(x => x.Value.Priority)
                    .Select(x => new RemoteRepositoryMessage(this, x.Key, x.Value)))
                yield return m;
        }

        protected IPackageRepository GetRemoteRepository(string repositoryName)
        {
            Uri possibleUri;
            try
            {
                possibleUri = new Uri(repositoryName, UriKind.Absolute);
            }
            catch
            {
                possibleUri = null;
            }
            return possibleUri != null
                           ? new RemoteRepositoryBuilder(FileSystem, ConfigurationManager).BuildPackageRepositoryForUri(repositoryName, possibleUri)
                           : Environment.RemoteRepositories.FirstOrDefault(x => x.Name.EqualsNoCase(repositoryName));
        }
        protected IDisposable ChangeMonitor(FileBased<IPackageDescriptor> descriptor)
        {
                bool packagesChanged = false;
                PackageChanged change = (a, b, c, d) =>
                {
                    packagesChanged = true;
                    return Enumerable.Empty<object>();
                };
                PackageUpdated update = (a, b, c, d, e) =>
                {
                    packagesChanged = true;
                    return Enumerable.Empty<object>();
                };
                var listener = PackageManager.Monitor(change, change, update);
                return new ActionOnDispose(()=>
                {
                    listener.Dispose();
                    if (packagesChanged)
                    {
                        if (Environment.Descriptor != descriptor.Value)
                        {
                            descriptor.File.Touch();
                            Environment.DescriptorFile.Touch();
                        }
                        else
                        {
                            foreach (var file in Environment.ScopedDescriptors.Select(x => x.Value.File))
                                file.Touch();
                        }
                    }
                });
        }
    }
}