using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataModel
{
    [XmlRoot("relationship")]
    public class N1RelationShipItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("lookupfield")]
        public string Field { get; set; }

        [XmlElement("primaryentity")]
        public string PrimaryEntityLogicName { get; set; }

        [XmlElement("primaryfield")]
        public string PrimaryEntityFieldLogicName { get; set; }  
    }

    [XmlRoot("n1relationship", Namespace = "urn:nextlabs-crm-schema")]
    public class N1Relationship
    {
        [XmlElement("relatedentity")]
        public string RelatedEntity { get; set; }

        [XmlArray("relationships"), XmlArrayItem("relationship")]
        public List<N1RelationShipItem> Relationships { get; set; }
        public N1RelationShipItem FindRelationshipByName(string relationshipName)
        {
            foreach(N1RelationShipItem item in Relationships)
            {
                if(item.Name.Equals(relationshipName,StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
