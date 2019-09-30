using System;

namespace dell_switch_exporter
{
    public class Interface
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int AdminStatus { get; set; }
        public int OperStatus { get; set; }
        public UInt64 InOctets { get; set; }
        public UInt64 OutOctets { get; set; }
    }
}