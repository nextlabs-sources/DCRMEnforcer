using Microsoft.Xrm.Sdk.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DCRMPluginForSharePoint
{
    [XmlRoot("Credential")]
    public class Credential
    {
        [XmlElement("UserName")]
        public string UserName { get; set; }

        [XmlElement("Domain")]
        public string Domain { get; set; }

        [XmlElement("Password")]
        public string Password { get; set; }
    }


    [XmlRoot("Config")]
    public class Configuration
    {
        [XmlElement("OrganizationUri")]
        public string OrganizationUri { get; set; }

        [XmlElement("HomeRealmUri")]
        public string HomeRealmUri { get; set; }

        [XmlElement("Credential")]
        public Credential Credential { get; set; }

        [XmlElement("AuthProviderType")]
        public AuthenticationProviderType AuthProviderType { get; set; }
    }
}
