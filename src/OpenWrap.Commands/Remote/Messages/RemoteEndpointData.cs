using System.Text;

namespace OpenWrap.Commands.Remote.Messages
{
    public class RemoteEndpointData
    {
        const string ALIGN = "               ";
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeName { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public RemoteEndpointData(string name, string type, string feedType, string token, string username, string password)
        {
            Name = name;
            Type = type;
            TypeName = feedType;
            Token = token;
            Username = username;
            Password = password;
        }
        public override string ToString()
        {
            string template = ALIGN + (Username != null ? "{0,-8}: {1}" : "{0,-5}: {1}");
            var sb = new StringBuilder()
                .AppendFormat("[{0}]", Type).AppendLine()
                .AppendFormat(template, "name", Name).AppendLine()
                .AppendFormat(template, "type", TypeName).AppendLine()
                .AppendFormat(template, "token", Token).AppendLine();
            if (Username != null)
                sb.AppendFormat(template, "username", Username).AppendLine()
                    .AppendFormat(template, "password", Password).AppendLine();
            return sb.ToString();

        }
    }
}