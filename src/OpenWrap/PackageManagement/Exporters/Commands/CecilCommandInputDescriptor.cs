using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using OpenWrap.Commands;

namespace OpenWrap.PackageManagement.Exporters.Commands
{
    public class CecilCommandInputDescriptor : ICommandInputDescriptor
    {
        string _propertyName;

        public CecilCommandInputDescriptor(PropertyDefinition property, IDictionary<string, object> inputAttrib)
        {
            IsValueRequired = property.PropertyType.FullName != typeof(bool).FullName;
            
            _propertyName = property.Name;
            inputAttrib.TryGet("Name", name => Name = (string)name);
            inputAttrib.TryGet("Description", description => Description = (string)description);
            inputAttrib.TryGet("IsValueRequired", _ => IsValueRequired = (bool)_);
            inputAttrib.TryGet("IsRequired", _ => IsRequired = (bool)_);
            inputAttrib.TryGet("Position", position => Position = (int)position);

            Name = Name ?? property.Name;
            Type = property.PropertyType.Name;
            
        }

        public bool IsRequired { get; set; }
        public bool IsValueRequired { get; set; }

        public bool MultiValues
        {
            get { return false; }
        }

        public string Name { get; set; }

        public string Type { get; private set; }

        public string Description { get; set; }
        public int? Position { get; set; }


        public bool TrySetValue(ICommand target, IEnumerable<string> values)
        {
            var pi = target.GetType().GetProperty(_propertyName);
            var destinationType = pi.PropertyType;

            if (IsValueRequired == false && values.Count() == 0)
            {
                if (destinationType == typeof(bool))
                    pi.SetValue(target, true, null);
                else if (destinationType.IsValueType)
                    pi.SetValue(target, Activator.CreateInstance(destinationType), null);
                else
                    pi.SetValue(target, null, null);
                return true;
            }
            object value;

            if (!StringConversion.TryConvert(destinationType, values, out value))
                return false;
            try
            {
                pi.SetValue(target, value, null);
                return true;
            }
            catch
            {
            }
            return false;
        }
    }
}