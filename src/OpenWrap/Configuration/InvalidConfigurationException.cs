using System;

namespace OpenWrap.Configuration
{
    [Serializable]
    public class InvalidConfigurationException : Exception
    {
        public InvalidConfigurationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}