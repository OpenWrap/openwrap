using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Exports;
using OpenWrap.Repositories;
using IOPath = System.IO.Path;

namespace OpenWrap.Services
{
    // TODO: Implement file monitoring in the IFileSystem implementation and remove FileSystemEventHandler
    public class PackageDescriptorMonitor : IWrapDescriptorMonitoringService
    {
        readonly Dictionary<Path, DescriptorSubscriptions> _notificationClients = new Dictionary<Path, DescriptorSubscriptions>();
        


        IPackageResolver PackageResolver { get { return Services.GetService<IPackageResolver>(); } }
        public void ProcessWrapDescriptor(IFile wrapFile, IPackageRepository packageRepository, IPackageAssembliesListener listener)
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

        public void Initialize()
        {
        }

        DescriptorSubscriptions GetDescriptor(IFile wrapPath, IPackageRepository packageRepository)
        {
            DescriptorSubscriptions descriptorSubscriptions;
            if (!_notificationClients.TryGetValue(wrapPath.Path, out descriptorSubscriptions))
                _notificationClients.Add(wrapPath.Path, descriptorSubscriptions = new DescriptorSubscriptions(wrapPath, packageRepository, HandleWrapFileUpdate));
            return descriptorSubscriptions;
        }

        void HandleWrapFileUpdate(object sender, System.IO.FileSystemEventArgs e)
        {
            NotifyAllClients(LocalFileSystem.Instance.GetFile(e.FullPath));
        }
        void NotifyClient(IFile wrapPath, IPackageAssembliesListener listener)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];
            d.Repository.Refresh();
            var parsedDescriptor = new PackageDescriptorReaderWriter().Read(wrapPath);


            listener.AssembliesUpdated(PackageResolver.GetAssemblyReferences(false, listener.Environment, parsedDescriptor, d.Repository));
        }
        void NotifyAllClients(IFile wrapPath)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];
            d.Repository.Refresh();
            var parsedDescriptor = new PackageDescriptorReaderWriter().Read(wrapPath);

            foreach (var client in d.Clients)
            {
                client.AssembliesUpdated(PackageResolver.GetAssemblyReferences(false, client.Environment, parsedDescriptor, d.Repository));
            }
        }

        class DescriptorSubscriptions
        {
            public DescriptorSubscriptions(IFile path, IPackageRepository repository, System.IO.FileSystemEventHandler handler)
            {
                Repository = repository;
                Clients = new List<IPackageAssembliesListener>();
                FileSystemWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(path.Path.FullPath), System.IO.Path.GetFileName(path.Path.FullPath))
                {
                    NotifyFilter = System.IO.NotifyFilters.LastWrite
                };
                FileSystemWatcher.Changed += handler;
                FileSystemWatcher.EnableRaisingEvents = true;
            }

            public List<IPackageAssembliesListener> Clients { get; set; }
            public System.IO.FileSystemWatcher FileSystemWatcher { get; set; }
            public IPackageRepository Repository { get; set; }
        }
    }
}