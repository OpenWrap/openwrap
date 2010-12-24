using System;
using System.Collections.Generic;
using System.IO;
using OpenFileSystem.IO;
using OpenFileSystem.IO.FileSystems.Local;
using OpenWrap.PackageManagement.Exporters;
using OpenWrap.PackageModel.Serialization;
using OpenWrap.Repositories;
using Path = OpenFileSystem.IO.Path;

namespace OpenWrap.PackageManagement.Monitoring
{
    // TODO: Implement file monitoring in the IFileSystem implementation and remove FileSystemEventHandler
    public class PackageDescriptorMonitor : IPackageDescriptorMonitor
    {
        readonly Dictionary<Path, DescriptorSubscriptions> _notificationClients = new Dictionary<Path, DescriptorSubscriptions>();


        IPackageResolver PackageResolver
        {
            get { return Services.Services.GetService<IPackageResolver>(); }
        }

        public void Initialize()
        {
        }

        public void RegisterListener(IFile wrapFile, IPackageRepository packageRepository, IResolvedAssembliesUpdateListener listener)
        {
            if (!wrapFile.Exists)
                return;

            if (packageRepository == null) throw new ArgumentNullException("packageRepository");
            if (listener == null) throw new ArgumentNullException("listener");

            var descriptor = GetDescriptor(wrapFile, packageRepository);
            if (listener.IsLongRunning)
                descriptor.Clients.Add(listener);

            NotifyClient(wrapFile, listener);
        }

        DescriptorSubscriptions GetDescriptor(IFile wrapPath, IPackageRepository packageRepository)
        {
            DescriptorSubscriptions descriptorSubscriptions;
            if (!_notificationClients.TryGetValue(wrapPath.Path, out descriptorSubscriptions))
                _notificationClients.Add(wrapPath.Path, descriptorSubscriptions = new DescriptorSubscriptions(wrapPath, packageRepository, HandleWrapFileUpdate));
            return descriptorSubscriptions;
        }

        void HandleWrapFileUpdate(object sender, FileSystemEventArgs e)
        {
            NotifyAllClients(LocalFileSystem.Instance.GetFile(e.FullPath));
        }

        void NotifyAllClients(IFile wrapPath)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];
            d.Repository.RefreshPackages();
            var parsedDescriptor = new PackageDescriptorReaderWriter().Read(wrapPath);

            foreach (var client in d.Clients)
            {
                client.AssembliesUpdated(PackageResolver.GetAssemblyReferences(false, client.Environment, parsedDescriptor, d.Repository));
            }
        }

        void NotifyClient(IFile wrapPath, IResolvedAssembliesUpdateListener listener)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];
            d.Repository.RefreshPackages();
            var parsedDescriptor = new PackageDescriptorReaderWriter().Read(wrapPath);


            listener.AssembliesUpdated(PackageResolver.GetAssemblyReferences(false, listener.Environment, parsedDescriptor, d.Repository));
        }

        class DescriptorSubscriptions
        {
            public DescriptorSubscriptions(IFile path, IPackageRepository repository, FileSystemEventHandler handler)
            {
                Repository = repository;
                Clients = new List<IResolvedAssembliesUpdateListener>();
                FileSystemWatcher = new FileSystemWatcher(System.IO.Path.GetDirectoryName(path.Path.FullPath), System.IO.Path.GetFileName(path.Path.FullPath))
                {
                        NotifyFilter = NotifyFilters.LastWrite
                };
                FileSystemWatcher.Changed += handler;
                FileSystemWatcher.EnableRaisingEvents = true;
            }

            public List<IResolvedAssembliesUpdateListener> Clients { get; set; }
            public FileSystemWatcher FileSystemWatcher { get; set; }
            public IPackageRepository Repository { get; set; }
        }
    }
}