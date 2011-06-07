using System.Text;

namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteRepositoryData : Info
    {
        public RemoteRepositoryData(int priority, string name, bool publish, bool fetch)
        {
            Priority = priority;
            Name = name;
            Publish = publish;
            Fetch = fetch;
        }

        public bool Fetch { get; set; }
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool Publish { get; set; }

        string EndPoints
        {
            get
            {
                var sb = new StringBuilder();
                if (Fetch) sb.Append("fetch");
                if (Publish) sb.Append(sb.Length > 0 ? ", publish" : "publish");
                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return string.Format("{0,3} {1,-10} [{2}]",
                                 Priority,
                                 Name,
                                 EndPoints
                );
        }
    }
}