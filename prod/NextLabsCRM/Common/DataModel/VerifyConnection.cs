using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    [XmlRoot("testpc", Namespace = "urn:nextlabs-crm-schema")]
    public class TestPC
    {
        [XmlElement("policycontrolhost")]
        public string PolicyControllerHost { get; set; }

        [XmlElement("policycontrolport")]
        public int PolicyControllerPort { get; set; }

        [XmlElement("ishttps")]
        public bool IsHttps { get; set; }

        [XmlElement("oauthserverhost")]
        public string OAuthServerHost { get; set; }

        [XmlElement("oauthport")]
        public int OAuthPort { get; set; }

        [XmlElement("clientid")]
        public string ClientID { get; set; }

        [XmlElement("clientpassword")]
        public string ClientPassword { get; set; }
    }

    [XmlRoot("testwc", Namespace = "urn:nextlabs-crm-schema")]
    public class TestWC
    {

        [XmlElement("ishttps")]
        public bool IsHttps { get; set; }

        [XmlElement("oauthserverhost")]
        public string OAuthServerHost { get; set; }

        [XmlElement("oauthport")]
        public int OAuthPort { get; set; }

        [XmlElement("wcusrname")]
        public string wcUsername { get; set; }

        [XmlElement("wcpwd")]
        public string wcPassword { get; set; }
    }
}
