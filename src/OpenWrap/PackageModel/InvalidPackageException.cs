using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OpenWrap.PackageModel
{
    [Serializable]
    public class InvalidPackageException : Exception
    {
        public InvalidPackageException()
        {
        }

        public InvalidPackageException(string message) : base(message)
        {
        }

        public InvalidPackageException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidPackageException(
                SerializationInfo info,
                StreamingContext context) : base(info, context)
        {
        }
    }
}
