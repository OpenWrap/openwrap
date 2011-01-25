using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenWrap.Windows.Controls.Demo
{
    public class DemoType1
    {
        private DemoType2 _linked = new DemoType2();
        public DemoType2 Linked { get { return _linked; } }
    }
    public class DemoType2
    {
        private DemoType1 _another;
        public DemoType1 Linked { get
        {
            if (_another == null)
                _another = new DemoType1();
            return _another;
        } }
    }
}
