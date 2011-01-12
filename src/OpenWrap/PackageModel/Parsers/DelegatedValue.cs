namespace OpenWrap.PackageModel.Parsers
{
    public class DelegatedValue<T>
    {
        readonly SingleValue<T> _parent;
        readonly SingleValue<T> _current;

        public DelegatedValue(SingleValue<T> parent, SingleValue<T> current)
        {
            _parent = parent;
            _current = current;
        }
        public T Value
        {
            get { return _current.HasValue ? _current.Value : _parent.Value; }
            set
            {
                if (_parent.HasValue)
                    _current.ForceValue(value);
                else
                    _current.Value = value;
            }
        }
    }
}