using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace NextLabs.CRMEnforcer.Common.DataModel
{
    [XmlRoot("secureentity", Namespace = "urn:nextlabs-crm-schema")]
    public class SecureEntity
    {
        [XmlElement("secured")]
        public Boolean Secured { get; set; }

        [XmlElement("schema")]
        public MetaData.Entity Schema { get; set; }

        public override string ToString()
        {
            IStringSerialize xmlSericaliehelp = new XMLSerializeHelper();
            return xmlSericaliehelp.Serialize(typeof(SecureEntity), this);
        }

        public SecureEntity Clone()
        {
            SecureEntity cloned = new SecureEntity();
            cloned.Schema = new MetaData.Entity();
            cloned.Secured = this.Secured;
            cloned.Schema = this.Schema.Clone();
            return cloned;
        }

        public void MergeAttributes(SecureEntity other)
        {
            if(other == null || other.Schema == null)
            {
                return;
            }
            if(this.Schema == null)
            {
                this.Schema = new MetaData.Entity();
            }

            if(other.Schema.Attributes == null || other.Schema.Attributes.Count == 0)
            {
                return;
            }

            if(this.Schema.Attributes == null)
            {
                this.Schema.Attributes = new List<MetaData.Attribute>();
            }

            foreach(MetaData.Attribute attr in other.Schema.Attributes)
            {
                bool notFound = true;
                foreach (MetaData.Attribute attrOrg in this.Schema.Attributes)
                {
                    if (attr.LogicName.Equals(attrOrg.LogicName, StringComparison.OrdinalIgnoreCase))
                    {
                        notFound = false;
                        break;
                    }
                }

                if (notFound)
                {
                    this.Schema.Attributes.Add(attr);
                }
            }
        }
    }
}
