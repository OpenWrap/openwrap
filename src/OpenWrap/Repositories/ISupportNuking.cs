using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportNuking : IRepositoryFeature
    {
        void Nuke(IPackageInfo packageInfo);
    }
}