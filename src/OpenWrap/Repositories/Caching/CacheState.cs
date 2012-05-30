namespace OpenWrap.Repositories.Caching
{
    public class CacheState
    {
        public long CacheSize { get; set; }
        public UpdateToken UpdateToken { get; set; }
        public string FileName { get; set; }
    }
}