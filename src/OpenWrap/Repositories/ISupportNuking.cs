using OpenWrap.PackageModel;

namespace OpenWrap.Repositories
{
    public interface ISupportNuking : IPackageRepository
    {
        void Nuke(IPackageInfo packageInfo);
    }
}