using System.Collections.Generic;
using System.IO;
using OpenRasta.Wrap.Dependencies;
using OpenRasta.Wrap.Sources;

namespace OpenRasta.Wrap.Build.Services
{
    public class WrapDescriptorMonitor : IService, IWrapDescriptorMonitoringService
    {
        readonly Dictionary<string, WrapFileDescriptor> _notificationClients = new Dictionary<string, WrapFileDescriptor>();
        readonly WrapDependencyResolver _resolver = new WrapDependencyResolver();



        public void ProcessWrapDescriptor(string wrapPath, IWrapRepository wrapsDirectory, IWrapAssemblyClient client)
        {
            if (!File.Exists(wrapPath))
                return;

            var descriptor = GetDescriptor(wrapPath, wrapsDirectory);
            if (client.IsLongRunning)
                descriptor.Clients.Add(client);

            NotifyClient(wrapPath, client);
        }

        public void Initialize()
        {
        }

        WrapFileDescriptor GetDescriptor(string wrapPath, IWrapRepository wrapsDirectory)
        {
            WrapFileDescriptor descriptor;
            if (!_notificationClients.TryGetValue(wrapPath, out descriptor))
                _notificationClients.Add(wrapPath, descriptor = new WrapFileDescriptor(wrapPath, wrapsDirectory, HandleWrapFileUpdate));
            return descriptor;
        }

        void HandleWrapFileUpdate(object sender, FileSystemEventArgs e)
        {
            NotifyAllClients(e.FullPath);
        }
        void NotifyClient(string wrapPath, IWrapAssemblyClient client)
        {
            if (!_notificationClients.ContainsKey(wrapPath))
                return;
            var d = _notificationClients[wrapPath];

            var parsedDescriptor = new WrapDescriptorParser().Parse(wrapPath);

            client.WrapAssembliesUpdated(_resolver.GetAssemblyReferences(parsedDescriptor, d.Repository, client));
        }

        void NotifyAllClients(string wrapPath)
        {
            if (!_notificationClients.ContainsKey(wrapPath))
                return;
            var d = _notificationClients[wrapPath];

            var parsedDescriptor = new WrapDescriptorParser().Parse(wrapPath);

            foreach (var client in d.Clients)
            {
                client.WrapAssembliesUpdated(_resolver.GetAssemblyReferences(parsedDescriptor, d.Repository, client));
            }
        }

        class WrapFileDescriptor
        {
            public WrapFileDescriptor(string path, IWrapRepository wraps, FileSystemEventHandler handler)
            {
                Repository = wraps;
                Clients = new List<IWrapAssemblyClient>();
                FilePath = path;
                FileSystemWatcher = new FileSystemWatcher(Path.GetDirectoryName(path), Path.GetFileName(path))
                {
                    NotifyFilter = NotifyFilters.LastWrite
                };
                FileSystemWatcher.Changed += handler;
                FileSystemWatcher.EnableRaisingEvents = true;
            }

            public List<IWrapAssemblyClient> Clients { get; set; }
            public string FilePath { get; set; }
            public FileSystemWatcher FileSystemWatcher { get; set; }
            public IWrapRepository Repository { get; set; }
        }
    }
}