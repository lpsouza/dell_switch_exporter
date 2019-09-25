using System;

namespace dell_switch_exporter
{
    public class Interface
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UInt32 Speed { get; set; }
        public string Mac { get; set; }
        public string AdminStatus { get; set; }
        public UInt32 InOctets { get; set; }
        public UInt32 OutOctets { get; set; }
    }
}