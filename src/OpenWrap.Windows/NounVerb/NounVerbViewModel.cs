using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;

namespace OpenWrap.Windows.NounVerb
{
    public class NounVerbViewModel : ViewModelBase
    {
        private readonly ObservableCollection<NounSlice> _nouns = new ObservableCollection<NounSlice>();
        private readonly ICommand _populateDataCommand = new PopulateNounVerb();
        private NounSlice _selectedNoun;

        public NounVerbViewModel()
        {
            Messenger.Default.Subcribe(MessageNames.NounsVerbsChanged, this, PopulateData);
        }

        public ObservableCollection<NounSlice> Nouns
        {
            get
            {
                return _nouns;
            }
        }

        public NounSlice SelectedNoun
        {
            get
            {
                return _selectedNoun;
            }
            set
            {
                _selectedNoun = value;
                RaisePropertyChanged(() => this.SelectedNoun);
            }
        }

        private void PopulateData()
        {
            _populateDataCommand.Execute(this);
        }
    }
}
