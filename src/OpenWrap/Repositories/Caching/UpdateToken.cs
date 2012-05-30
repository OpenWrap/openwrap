using OpenWrap.Configuration;

namespace OpenWrap.Repositories.Caching
{
    public class UpdateToken
    {
        readonly string _value;

        public UpdateToken(string value)
        {
            _value = value;
        }
        public override string ToString()
        {
            return _value;
        }
    }
}