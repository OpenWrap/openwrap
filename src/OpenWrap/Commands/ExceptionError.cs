using System;

namespace OpenWrap.Commands
{
    public class ExceptionError : Error
    {
        readonly Exception _exception;

        public ExceptionError(Exception exception)
        {
            _exception = exception;
        }

        public override string ToString()
        {
            return _exception.Message;
        }
    }
}