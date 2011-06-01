namespace OpenWrap.Commands.Remote
{
    public class UnknownRepositoryType : Error
    {
        public UnknownRepositoryType(string repositoryInput)
            : base("The address '{0}' was not recognized as a known repository type.", repositoryInput)
        {   
        }
    }
}