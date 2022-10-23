using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel.MetaData
{
    [XmlRoot("entity")]
    public class Entity
    {
        [XmlElement("owneridexist")]
        public bool OwneridExist { get; set; }
        [XmlElement("primaryidname")]
        public String PrimaryIDName { get; set; }
        [XmlElement("logicname")]
        public String LogicName { get; set; }

        [XmlElement("displayname")]
        public String DisplayName { get; set; }

        [XmlElement("pluralname")]
        public String PluralName { get; set; }

        [XmlElement("objecttypecode")]
        public int ObjectTypeCode { get; set; }

        [XmlArray("attributes"), XmlArrayItem("attribute")]
        public List<Attribute> Attributes { get; set; }

        public Entity Clone()
        {
            Entity cloned = new Entity();
            cloned.OwneridExist = this.OwneridExist;
            cloned.DisplayName = this.DisplayName;
            cloned.LogicName = this.LogicName;
            cloned.PluralName = this.PluralName;
            cloned.PrimaryIDName = this.PrimaryIDName;
            cloned.ObjectTypeCode = this.ObjectTypeCode;

            cloned.Attributes = new List<Attribute>();

            if (this.Attributes != null)
            {
                foreach (Attribute attr in this.Attributes)
                {
                    cloned.Attributes.Add(attr.Clone());
                }
            }

            return cloned;
        }
    }

}
