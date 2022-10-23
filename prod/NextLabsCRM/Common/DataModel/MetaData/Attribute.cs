using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel.MetaData
{
    [XmlRoot("attribute")]
    public class Attribute
    {

        [XmlElement("logicname")]
        public String LogicName { get; set; }

        [XmlElement("displayname")]
        public String DisplayName { get; set; }

        [XmlElement("datatype")]
        public String DataType { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("required")]
        public bool Required { get; set; }

        [XmlArray("options"), XmlArrayItem("option")]
        public List<Option> Options { get; set; }

        public Attribute Clone()
        {
            Attribute cloned = new Attribute();
            cloned.LogicName = this.LogicName;
            cloned.DisplayName = this.DisplayName;
            cloned.DataType = this.DataType;
            cloned.Description = this.Description;
            cloned.Required = this.Required;
            cloned.Options = new List<Option>();
            if(this.Options != null)
            {
                foreach(Option op in this.Options)
                {
                    cloned.Options.Add(op.Clone());
                }
            }

            return cloned;
        }
    }
}
