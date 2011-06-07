namespace OpenWrap.Configuration.Core
{
    [Path("core")]
    public class CoreConfiguration
    {
        public string ProxyHref { get; set; }
        public string ProxyUsername { get; set; }
        [Encrypt]
        public string ProxyPassword { get;set; }
    }
}