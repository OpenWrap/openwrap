namespace OpenWrap.Build.PackageBuilders
{
    public class UnknownProjectFileResult : ErrorBuildResult
    {
        public string Spec { get; set; }

        public UnknownProjectFileResult(string spec)
            : base(string.Format("Project '{0}' not found.", spec))
        {
            Spec = spec;
        }
    }
}