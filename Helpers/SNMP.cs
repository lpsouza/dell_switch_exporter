using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace dell_switch_exporter
{
    class SNMP
    {
        private IPEndPoint Ip { get; set; }
        private OctetString Community { get; set; }
        public int Timeout { get; set; }
        public VersionCode SnmpVersion { get; set; }
        private IList<Variable> Oids { get; set; }
        public SNMP(string ip, string community)
        {
            Ip = new IPEndPoint(IPAddress.Parse(ip), 161);
            Community = new OctetString(community);
            Timeout = 2000;
            SnmpVersion = VersionCode.V2;
            Oids = new List<Variable>();
        }

        public IList<Variable> AddOid(string oid)
        {
            Oids.Add(new Variable(new ObjectIdentifier(oid)));
            return Oids;
        }
        public IList<Variable> Get()
        {
            IList<Variable> result = Messenger.Get(SnmpVersion, Ip, Community, Oids, Timeout);
            Oids = new List<Variable>();
            return result;
        }
        public IList<Variable> GetBulk(int maxRepetitions)
        {
            GetBulkRequestMessage message = new GetBulkRequestMessage(0, SnmpVersion, Community, 0, maxRepetitions, Oids);
            Oids = new List<Variable>();
            ISnmpMessage result = message.GetResponse(Timeout, Ip);
            return result.Variables();
        }
    }
}
