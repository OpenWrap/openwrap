using System.Collections.Generic;
using System.Linq;

namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteRepositoryDetailedData : Info
    {
        public RemoteRepositoryDetailedData(int priority, string name, IEnumerable<RemoteEndpointData> remotePaths)
        {
            Priority = priority;
            Name = name;
            Endpoints = remotePaths;
        }

        public IEnumerable<RemoteEndpointData> Endpoints { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }

        public override string ToString()
        {
            return string.Format("{0,3} {1,-10} {2}",
                                 Priority,
                                 Name,
                                 Endpoints.Select(x => x.ToString()).JoinString("\r\n               ")
                );
        }
    }
}