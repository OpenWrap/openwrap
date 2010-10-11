using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;
using IOPath = System.IO.Path;

namespace OpenWrap.Services
{
    // TODO: Implement file monitoring in the IFileSystem implementation and remove FileSystemEventHandler
    public class PackageDescriptorMonitor : IWrapDescriptorMonitoringService
    {
        readonly Dictionary<Path, DescriptorSubscriptions> _notificationClients = new Dictionary<Path, DescriptorSubscriptions>();
        readonly PackageAssemblyResolver _resolver = new PackageAssemblyResolver();



        public void ProcessWrapDescriptor(IFile wrapFile, IPackageRepository packageRepository, IWrapAssemblyClient client)
        {
            if (!wrapFile.Exists)
                return;

            if (packageRepository == null) throw new ArgumentNullException("packageRepository");
            if (client == null) throw new ArgumentNullException("client");
   
            var descriptor = GetDescriptor(wrapFile, packageRepository);
            if (client.IsLongRunning)
                descriptor.Clients.Add(client);

            NotifyClient(wrapFile, client);
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
        void NotifyClient(IFile wrapPath, IWrapAssemblyClient client)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];
            d.Repository.Refresh();
            var parsedDescriptor = new PackageDescriptorReaderWriter().ParseFile(wrapPath);
            

            client.WrapAssembliesUpdated(_resolver.GetAssemblyReferences(parsedDescriptor, d.Repository, client));
        }
        void NotifyAllClients(IFile wrapPath)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];
            d.Repository.Refresh();
            var parsedDescriptor = new PackageDescriptorReaderWriter().ParseFile(wrapPath);

            foreach (var client in d.Clients)
            {
                client.WrapAssembliesUpdated(_resolver.GetAssemblyReferences(parsedDescriptor, d.Repository, client));
            }
        }

        class DescriptorSubscriptions
        {
            public DescriptorSubscriptions(IFile path, IPackageRepository repository, System.IO.FileSystemEventHandler handler)
            {
                Repository = repository;
                Clients = new List<IWrapAssemblyClient>();
                FileSystemWatcher = new System.IO.FileSystemWatcher(System.IO.Path.GetDirectoryName(path.Path.FullPath), System.IO.Path.GetFileName(path.Path.FullPath))
                {
                    NotifyFilter = System.IO.NotifyFilters.LastWrite
                };
                FileSystemWatcher.Changed += handler;
                FileSystemWatcher.EnableRaisingEvents = true;
            }

            public List<IWrapAssemblyClient> Clients { get; set; }
            public System.IO.FileSystemWatcher FileSystemWatcher { get; set; }
            public IPackageRepository Repository { get; set; }
        }
    }
}