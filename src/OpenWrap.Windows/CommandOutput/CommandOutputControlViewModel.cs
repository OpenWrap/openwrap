using System;
using System.Collections.Generic;
using System.Text;
using OpenWrap.Commands;
using OpenWrap.Windows.Framework;
using OpenWrap.Windows.Framework.Messaging;

namespace OpenWrap.Windows.CommandOutput
{
    public class CommandOutputControlViewModel : ViewModelBase
    {
        private const int ResultsHistorySize = 10;

        private readonly List<ICommandOutput> _commandOutputHistory = new List<ICommandOutput>();
        private string _commandOutput;
        
        public CommandOutputControlViewModel()
        {
            Messenger.Default.Subcribe<IEnumerable<ICommandOutput>>(MessageNames.CommandOutput, this, ReadCommandOutput);
        }

        public string CommandOutput
        {
            get
            {
                return _commandOutput;
            }
            set
            {
                if (_commandOutput != value)
                {
                    _commandOutput = value;
                    RaisePropertyChanged(() => this.CommandOutput);
                }
            }
        }

        private void ReadCommandOutput(IEnumerable<ICommandOutput> data)
        {
            _commandOutputHistory.AddRange(data);

            TruncateHistory();
            DisplayHistory();
        }

        private void TruncateHistory()
        {
            while (_commandOutputHistory.Count > ResultsHistorySize)
            {
                _commandOutputHistory.RemoveAt(0);
            }
        }
        
        private void DisplayHistory()
        {
            StringBuilder outputDisplayed = new StringBuilder();
            bool firstLine = true;
            foreach (ICommandOutput commandOutput in _commandOutputHistory)
            {
                if (firstLine)
                {
                    firstLine = false;
                }
                else
                {
                    outputDisplayed.AppendLine();
                }

                outputDisplayed.Append(commandOutput.ToString());
            }

            CommandOutput = outputDisplayed.ToString();
        }
    }
}
