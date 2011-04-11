using System;

namespace OpenWrap.VisualStudio.Hooks
{
    internal class VsCommandIdentifier : IEquatable<VsCommandIdentifier>
    {
        readonly Guid _commandGroup;
        readonly uint _commandId;

        public VsCommandIdentifier(Guid commandGroup, uint commandId)
        {
            _commandGroup = commandGroup;
            _commandId = commandId;
        }

        public bool Equals(VsCommandIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other._commandGroup.Equals(_commandGroup) && other._commandId == _commandId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(VsCommandIdentifier)) return false;
            return Equals((VsCommandIdentifier)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_commandGroup.GetHashCode() * 397) ^ _commandId.GetHashCode();
            }
        }

        public static bool operator ==(VsCommandIdentifier left, VsCommandIdentifier right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VsCommandIdentifier left, VsCommandIdentifier right)
        {
            return !Equals(left, right);
        }
    }
}