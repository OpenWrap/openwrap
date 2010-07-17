using System;

namespace OpenWrap.Commands
{
    public class Result : ICommandOutput
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

        public ICommand Source
        {
            get { throw new NotImplementedException(); }
        }

        public CommandResultType Type
        {
            get { return CommandResultType.Data; }
        }

        public override string ToString()
        {
            return _value;
        }
    }
}