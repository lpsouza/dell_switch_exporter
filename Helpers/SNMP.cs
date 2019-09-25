using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SnmpSharpNet;

namespace dell_switch_exporter
{
    class SNMP
    {
        private UdpTarget target { get; set; }
        private AgentParameters param { get; set; }
        public SNMP(string ip, string community)
        {
            OctetString comm = new OctetString(community);
            IpAddress agent = new IpAddress(ip);
            target = new UdpTarget((IPAddress)agent, 161, 2000, 1);
            param = new AgentParameters(comm);
            param.Version = SnmpVersion.Ver2;
        }
        public Switch GetSwitchInfo()
        {
            Switch sw = new Switch();
            sw.Interfaces = GetInterfaceList();
            // sw.Stack = GetStackList();
            return sw;
        }
        public IList<StackUnit> GetStackList()
        {
            List<StackUnit> stack = new List<StackUnit>();
            Pdu pdu = new Pdu(PduType.Get);
            // http://oidref.com/
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.5.0	Build Number
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.6.1	Boot ROM Version
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.4.1	Service Tag
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.3.1	Asset Tag
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.2.1	BIOS Serial Number
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.6.0	URL
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.4.0	Version
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.3.0	System Manufacturer
            // .1.3.6.1.4.1.674.10895.3000.1.2.100.1.0	System Model
            pdu.VbList.Add("1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.4.1"); // Dell.ServiceTag
            pdu.VbList.Add("1.3.6.1.4.1.674.10895.3000.1.2.100.3.0"); // Dell.SystemManufacturer
            pdu.VbList.Add("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0"); // Dell.SystemModel
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); // sysUpTime

            SnmpV2Packet result = (SnmpV2Packet)target.Request(pdu, param);
            VbCollection vbList = result.Pdu.VbList;

            stack.Add(new StackUnit()
            {
                ServiceTag = vbList.Where(a => a.Oid == new Oid("1.3.6.1.4.1.674.10895.3000.1.2.100.8.1.4.1")).FirstOrDefault().Value.ToString()
                // sw.SystemManufacturer = vbList.Where(a => a.Oid == new Oid("1.3.6.1.4.1.674.10895.3000.1.2.100.3.0")).FirstOrDefault().Value.ToString()
                // sw.SystemModel = vbList.Where(a => a.Oid == new Oid("1.3.6.1.4.1.674.10895.3000.1.2.100.1.0")).FirstOrDefault().Value.ToString()
                // sw.SysUpTime = vbList.Where(a => a.Oid == new Oid("1.3.6.1.2.1.1.3.0")).FirstOrDefault().Value.ToString()
            });

            return stack;
        }
        public IList<Interface> GetInterfaceList()
        {
            List<Interface> interfaces = new List<Interface>();
            VbCollection vbIdList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.1"); // interfaces.ifTable.ifEntry.ifIndex
            VbCollection vbDescrList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.2"); // interfaces.ifTable.ifEntry.ifDescr
            VbCollection vbSpeedList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.5"); // interfaces.ifTable.ifEntry.ifSpeed
            VbCollection vbPhysAddressList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.6"); // interfaces.ifTable.ifEntry.ifPhysAddress
            VbCollection vbAdminStatusList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.7"); // interfaces.ifTable.ifEntry.ifAdminStatus
            VbCollection vbInOctetsList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.10"); // interfaces.ifTable.ifEntry.ifInOctets
            VbCollection vbOutOctetsList = GetSnmpInfo("1.3.6.1.2.1.2.2.1.16"); // interfaces.ifTable.ifEntry.ifOutOctets
            for (int i = 0; i < vbIdList.Count; i++)
            {
                interfaces.Add(new Interface()
                {
                    Id = int.Parse(vbIdList[i].Value.ToString()),
                    Name = vbDescrList[i].Value.ToString(),
                    Speed = UInt32.Parse(vbSpeedList[i].Value.ToString()),
                    Mac = vbPhysAddressList[i].Value.ToString(),
                    AdminStatus = vbAdminStatusList[i].Value.ToString(),
                    InOctets = UInt32.Parse(vbInOctetsList[i].Value.ToString()),
                    OutOctets = UInt32.Parse(vbOutOctetsList[i].Value.ToString())
                });
            }
            return interfaces;
        }
        private VbCollection GetSnmpInfo(string oid)
        {
            VbCollection vbList = new VbCollection();
            Oid rootOid = new Oid(oid);
            Oid lastOid = (Oid)rootOid.Clone();
            Pdu pdu = new Pdu(PduType.GetNext);
            while (lastOid != null)
            {
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                pdu.VbList.Clear();
                pdu.VbList.Add(lastOid);
                SnmpV2Packet result = new SnmpV2Packet();
                result = (SnmpV2Packet)target.Request(pdu, param);
                if (result != null)
                {
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        lastOid = null;
                    }
                    else
                    {
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                vbList.Add(v);
                                lastOid = v.Oid;
                            }
                            else
                            {
                                lastOid = null;
                            }
                        }
                    }
                }
            }
            return vbList;
        }
    }
}