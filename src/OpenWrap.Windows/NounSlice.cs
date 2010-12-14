using System.Collections.Generic;
using System.ComponentModel;

namespace OpenWrap.Windows
{
    public class NounSlice : INotifyPropertyChanged
    {
        public NounSlice(string noun, IEnumerable<VerbSlice> commandDescriptors)
        {
            Noun = noun;
            Commands = commandDescriptors;
        }

        public IEnumerable<VerbSlice> Commands { get; set; }

        public string Noun { get; set; }
        public event PropertyChangedEventHandler PropertyChanged = (s,e)=>{};
        protected virtual void NotifyPropertyChanged(string propertyName) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName));}
    }
}