using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    public enum CommandAction
    {
        Remove,
        PublishAll,
        EnableSPAction,
        DisableSPAction
    }

    [XmlRoot("iocommand", Namespace = "urn:nextlabs-crm-schema")]
    public class IOCommand
    {
        [XmlElement("version")]
        public string Version { get; set; }

        [XmlElement("action")]
        public CommandAction Action { get; set; }
    }
}
