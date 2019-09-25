using System.Collections.Generic;

namespace dell_switch_exporter
{
    public class Switch
    {
        public IList<Interface> Interfaces  { get; set; }
        public IList<StackUnit> Stack { get; set; }
    }
}