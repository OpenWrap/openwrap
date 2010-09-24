namespace OpenWrap.Build.BuildEngines
{
    public class BuildResult
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}