namespace OpenWrap.Runtime
{
    public class ExecutionEnvironment
    {
        public ExecutionEnvironment()
        {
        }

        public ExecutionEnvironment(string platform, string profile)
        {
            Profile = profile;
            Platform = platform;
        }

        public string Platform { get; set; }
        public string Profile { get; set; }
    }
}