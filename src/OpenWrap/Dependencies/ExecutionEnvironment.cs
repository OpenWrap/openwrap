namespace OpenWrap.Dependencies
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

        public string Profile { get; set; }
        public string Platform { get; set; }
    }
}