using System;

namespace OpenWrap.Commands
{
    public class Result : ICommandOutput
    {
        readonly string _value;

        public Result(string str, params object[] parameters)
        {
            _value = string.Format(str, parameters);
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