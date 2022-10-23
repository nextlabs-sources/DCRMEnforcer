using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
namespace NextLabs.CRMEnforcer.Common.DataModel.ShareParam
{
    [DataContract]
    public class EnforcerResult
    {
        [DataMember]
        public string Decision { get; set; }

        [DataMember]
        public List<Obligation> Obligations { get; set; }
    }
    [DataContract]
    public class Obligation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<Attr> Attrs { get; set; }
    }
    [DataContract]
    public class Attr
    {
        [DataMember]
        public string key { get; set; }
        [DataMember]
        public string Value { get; set; }
        [DataMember]
        public string Type { get; set; }
    }
}
