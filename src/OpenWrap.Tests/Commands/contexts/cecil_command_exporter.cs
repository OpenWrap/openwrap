using System.Linq;
using OpenWrap;
using OpenWrap.PackageManagement;
using OpenWrap.PackageManagement.Exporters.Commands;

namespace Tests.Commands.contexts
{
    public class cecil_command_exporter : exporter<CecilCommandExporter, Exports.ICommand>
    {
        public cecil_command_exporter()
        {
            Exporter = new CecilCommandExporter();
        }
        protected Exports.ICommand command(string verb, string noun)
        {
            return Items.SelectMany(_ => _).FirstOrDefault(x => StringExtensions.EqualsNoCase(x.Descriptor.Noun, noun) && StringExtensions.EqualsNoCase(x.Descriptor.Verb, verb));
        }
    }
}