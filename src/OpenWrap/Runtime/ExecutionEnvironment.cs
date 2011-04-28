namespace OpenWrap.Runtime
{
    // TODO Make immutable
    public class ExecutionEnvironment
    {
        public static readonly ExecutionEnvironment Any = new ExecutionEnvironment("*", "*");
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