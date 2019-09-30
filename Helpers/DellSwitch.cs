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
        public Switch GetInfo()
        {
            Switch sw = new Switch();
            // sw.Interfaces = GetInterfaceList();
            // sw.Stack = GetStackList();
            return sw;
        }

        public IList<StackUnit> GetStackInfo()
        {
            List<StackUnit> stack = new List<StackUnit>();
            // Pdu pdu = new Pdu(PduType.Get);
            // // http://oidref.com/
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.5.0	Build Number
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.6.1	Boot ROM Version
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.4.1	Service Tag
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.3.1	Asset Tag
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.2.1	BIOS Serial Number
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.6.0	URL
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.4.0	Version
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.3.0	System Manufacturer
            // // .1.3.6.1.4.1.674.10895.3000.1.2.100.1.0	System Model
            // pdu.VbList.Add("1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.4.1"); // Dell.ServiceTag
            // pdu.VbList.Add("1.3.6.1.4.1.674.10895.3000.1.2.100.3.0"); // Dell.SystemManufacturer
            // pdu.VbList.Add("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"); // Dell.SystemModel
            // pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); // sysUpTime
            // SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
            // VbCollection vbList = result.Pdu.VbList;
            // stack.Add(new StackUnit()
            // {
            //     ServiceTag = vbList.Where(a => a.Oid == new Oid("1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.4.1")).FirstOrDefault().Value.ToString()
            //     // sw.SystemManufacturer = vbList.Where(a => a.Oid == new Oid("1.3.6.1.4.1.674.10895.3000.1.2.100.3.0")).FirstOrDefault().Value.ToString()
            //     // sw.SystemModel = vbList.Where(a => a.Oid == new Oid("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0")).FirstOrDefault().Value.ToString()
            //     // sw.SysUpTime = vbList.Where(a => a.Oid == new Oid("1.3.6.1.2.1.1.3.0")).FirstOrDefault().Value.ToString()
            // });
            return stack;
        }
        public IList<Interface> GetInterfaceInfo()
        {
            IList<Interface> interfaces = new List<Interface>();

            snmp.AddOid("1.3.6.1.2.1.2.1.0"); // ifNumber
            snmp.AddOid("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"); // productIdentification
            int maxRepetitions = int.Parse(snmp.Get()[0].Data.ToString());
            string productIdentification = snmp.Get()[1].Data.ToString();

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
                iface.InOctets = UInt64.Parse(info[2].Data.ToString());
                iface.OutOctets = UInt64.Parse(info[3].Data.ToString());

            }

            // IList<Variable> idList = GetBulkSnmpInfo("1.3.6.1.2.1.2.2.1.1"); // ifIndex
            // IList<Variable> aliasList = GetBulkSnmpInfo("1.3.6.1.2.1.31.1.1.1.18"); // ifAlias
            // IList<Variable> nameList = GetBulkSnmpInfo("1.3.6.1.2.1.31.1.1.1.1"); // ifName
            // IList<Variable> speedList = GetBulkSnmpInfo("1.3.6.1.2.1.31.1.1.1.15"); // ifHighSpeed
            // IList<Variable> inOctetsList = GetBulkSnmpInfo("1.3.6.1.2.1.31.1.1.1.6"); // ifHCInOctets
            // IList<Variable> outOctetsList = GetBulkSnmpInfo("1.3.6.1.2.1.31.1.1.1.10"); // ifHCOutOctets
            // foreach (var ifIndex in idList)
            // {
            //     string name = nameList.Where(a => a.Id == new ObjectIdentifier("1.3.6.1.2.1.31.1.1.1.1." + ifIndex.Data.ToString())).FirstOrDefault().Data.ToString();
            //     interfaces.Add(new Interface()
            //     {
            //         Id = int.Parse(ifIndex.Data.ToString()),
            //         Name = name
            //         Description = (aliasList[i].Data.ToString() == "\0\0") ? "\"No description\"" : aliasList[i].Data.ToString(),
            //         Speed = UInt64.Parse(speedList[i].Data.ToString()),
            //         InOctets = UInt64.Parse(inOctetsList[i].Data.ToString()),
            //         OutOctets = UInt64.Parse(outOctetsList[i].Data.ToString())
            //     });
            // }
            return interfaces;
        }
        // private void GetSnmpInfo()
        // {
        //     List<Variable> oidGetList = new List<Variable>();
        //     oidGetList.Add(new Variable(new ObjectIdentifier("1.3.6.1.2.1.2.1.0"))); // ifNumber
        //     oidGetList.Add(new Variable(new ObjectIdentifier("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"))); // productIdentification
        //     IList<Variable> getResult = Messenger.Get(SnmpVersion, Ip, Community, oidGetList, Timeout);
        // }
        // private IList<Variable> GetBulkSnmpInfo(string oid)
        // {
        //     oidGetList.Add(new Variable(new ObjectIdentifier("1.3.6.1.2.1.2.1.0"))); // ifNumber
        //     oidGetList.Add(new Variable(new ObjectIdentifier("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"))); // productIdentification

        //     int maxRepetitions = int.Parse(getResult[0].Data.ToString());

        //     // Model S4048T-ON needs this modification
        //     if (getResult[1].Data.ToString().Contains("S4048T-ON"))
        //     {
        //         maxRepetitions++;
        //     }

        //     List<Variable> oids = new List<Variable>();
        //     oids.Add(new Variable(new ObjectIdentifier(oid)));
        //     GetBulkRequestMessage message = new GetBulkRequestMessage(0, SnmpVersion, Community, 0, maxRepetitions, oids);
        //     ISnmpMessage result = message.GetResponse(Timeout, Ip);
        //     return result.Variables();
        // }
    }
}
