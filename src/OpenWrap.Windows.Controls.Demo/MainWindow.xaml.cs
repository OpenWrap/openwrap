using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenWrap.Windows.Controls.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<object> _values;
        private DemoType1 _first = new DemoType1();

        public MainWindow()
        {
            InitializeComponent();
            _values = new ObservableCollection<object> {_first };
            DataContext = this;
        }
        public object First { get { return _first; } }
        public ICollection Values { get { return _values; } }
    }
}
