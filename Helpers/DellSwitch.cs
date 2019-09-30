using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace dell_switch_exporter
{
    class DellSwitch
    {
        public SNMP snmp { get; set; }
        public DellSwitch(string ip, string community)
        {
            snmp = new SNMP(ip, community);
        }
        public IList<Interface> GetInterfaceInfo()
        {
            IList<Interface> interfaces = new List<Interface>();

            snmp.AddOid("1.3.6.1.2.1.2.1.0"); // ifNumber
            snmp.AddOid("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"); // productIdentification
            IList<Variable> ifInfo = snmp.Get();
            int maxRepetitions = int.Parse(ifInfo[0].Data.ToString());
            string productIdentification = ifInfo[1].Data.ToString();

            bool isS4048T = (productIdentification.Contains("S4048T-ON")) ? true : false;

            maxRepetitions = (isS4048T) ? maxRepetitions++ : maxRepetitions;

            snmp.AddOid("1.3.6.1.2.1.2.2.1.1"); // ifIndex
            IList<Variable> idList = snmp.GetBulk(maxRepetitions);

            foreach (var idItem in idList)
            {
                string id = idItem.Data.ToString();

                interfaces.Add(new Interface() { Id = int.Parse(id) });
                Interface iface = interfaces.Where(a => a.Id == int.Parse(id)).FirstOrDefault();

                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.1", id })); // ifName
                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.18", id })); // ifAlias
                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.2.2.1.7", id })); // ifAdminStatus
                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.2.2.1.8", id })); // ifOperStatus
                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.6", id })); // ifHCInOctets
                snmp.AddOid(string.Join(".", new string[] { "1.3.6.1.2.1.31.1.1.1.10", id })); // ifHCOutOctets
                
                IList<Variable> info = snmp.Get();

                iface.Name = info[0].Data.ToString();
                if (isS4048T)
                {
                    iface.Description = (info[1].Data.ToString() == "\0\0") ? "\"No description\"" : info[1].Data.ToString();
                }
                else
                {
                    iface.Description = (info[1].Data.ToString() == string.Empty) ? "\"No description\"" : string.Format("\"{0}\"", info[1].Data.ToString());
                }
                iface.AdminStatus = int.Parse(info[2].Data.ToString());
                iface.OperStatus = int.Parse(info[3].Data.ToString());
                iface.InOctets = UInt64.Parse(info[4].Data.ToString());
                iface.OutOctets = UInt64.Parse(info[5].Data.ToString());

            }

            return interfaces;
        }
    }
}
