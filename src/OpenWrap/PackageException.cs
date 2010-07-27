using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OpenWrap
{
    [Serializable]
    public class PackageException : Exception
    {
        public PackageException()
        {
        }

        public PackageException(string message) : base(message)
        {
        }

        public PackageException(string message, Exception inner) : base(message, inner)
        {
        }

        protected PackageException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
        {
        }
    }
}
