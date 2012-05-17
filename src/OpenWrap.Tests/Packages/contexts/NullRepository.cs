using System.Linq;
using OpenWrap.PackageModel;
using OpenWrap.Repositories;

namespace Tests.Packages.contexts
{
    public class NullRepository : IPackageRepository
    {
        public static readonly NullRepository Instance = new NullRepository();

        public NullRepository()
        {
            PackagesByName = new IPackageInfo[0].ToLookup(_ => default(string));
        }
        public ILookup<string, IPackageInfo> PackagesByName { get; private set; }
        public void RefreshPackages()
        {
        }

        public string Name { get; private set; }
        public string Token { get; private set; }
        public string Type { get; private set; }
        public TFeature Feature<TFeature>() where TFeature : class, IRepositoryFeature
        {
            return null;
        }
    }
}