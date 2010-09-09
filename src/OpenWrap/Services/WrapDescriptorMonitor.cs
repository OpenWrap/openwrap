using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenFileSystem.IO.FileSystem.Local;
using OpenWrap.Build;
using OpenWrap.Dependencies;
using OpenFileSystem.IO;
using OpenWrap.Repositories;

namespace OpenWrap.Services
{
    // TODO: Implement file monitoring in the IFileSystem implementation and remove FileSystemEventHandler
    public class WrapDescriptorMonitor : IWrapDescriptorMonitoringService
    {
        readonly Dictionary<IPath, WrapFileDescriptor> _notificationClients = new Dictionary<IPath, WrapFileDescriptor>();
        readonly WrapDependencyResolver _resolver = new WrapDependencyResolver();



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

        WrapFileDescriptor GetDescriptor(IFile wrapPath, IPackageRepository packageRepository)
        {
            WrapFileDescriptor descriptor;
            if (!_notificationClients.TryGetValue(wrapPath.Path, out descriptor))
                _notificationClients.Add(wrapPath.Path, descriptor = new WrapFileDescriptor(wrapPath, packageRepository, HandleWrapFileUpdate));
            return descriptor;
        }

        void HandleWrapFileUpdate(object sender, FileSystemEventArgs e)
        {
            NotifyAllClients(LocalFileSystem.Instance.GetFile(e.FullPath));
        }
        void NotifyClient(IFile wrapPath, IWrapAssemblyClient client)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];

            var parsedDescriptor = new WrapDescriptorParser().ParseFile(wrapPath);
            

            client.WrapAssembliesUpdated(_resolver.GetAssemblyReferences(parsedDescriptor, d.Repository, client));
        }
        void NotifyAllClients(IFile wrapPath)
        {
            if (!_notificationClients.ContainsKey(wrapPath.Path))
                return;
            var d = _notificationClients[wrapPath.Path];

            var parsedDescriptor = new WrapDescriptorParser().ParseFile(wrapPath);

            foreach (var client in d.Clients)
            {
                client.WrapAssembliesUpdated(_resolver.GetAssemblyReferences(parsedDescriptor, d.Repository, client));
            }
        }

        class WrapFileDescriptor
        {
            public WrapFileDescriptor(IFile path, IPackageRepository repository, FileSystemEventHandler handler)
            {
                Repository = repository;
                Clients = new List<IWrapAssemblyClient>();
                FileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path.Path.FullPath), Path.GetFileName(path.Path.FullPath))
                {
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileSystemWatcher.Changed += handler;
                FileSystemWatcher.EnableRaisingEvents = true;
            }

            public List<IWrapAssemblyClient> Clients { get; set; }
            public FileSystemWatcher FileSystemWatcher { get; set; }
            public IPackageRepository Repository { get; set; }
        }
    }
}