using NextLabs.CRMEnforcer.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    [XmlRoot("logsettings", Namespace = "urn:nextlabs-crm-schema")]
    public class LogSettings
    {
        public LogSettings()
        {

        }

        public LogSettings(LogLevel curLevel)
        {
            CurrentLevel = curLevel;
        }
        [XmlElement("currentlevel")]
        public LogLevel CurrentLevel { get; set; }
    }
}
