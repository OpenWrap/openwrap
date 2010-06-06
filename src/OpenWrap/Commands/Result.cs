using System;

namespace OpenWrap.Commands.Core
{
    public class Result : ICommandResult
    {
        readonly string _value;

        public Result(object value)
        {
            _value = value.ToString();
            Success = true;
        }
        public Result(string str, params object[] parameters)
        {
            _value = string.Format(str, parameters);
            Success = true;
        }
        public bool Success
        {
            get; set;
        }

        public ICommand Command
        {
            get { throw new NotImplementedException(); }
        }
        public override string ToString()
        {
            return _value;
        }
    }
}