using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace NextLabs.CRMEnforcer.Common.DataModel
{

    [XmlRoot("relationship")]
    public class NNRelationShipItem
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("relationshipentityname")]
        public string RelationshipEntityLogicName{get;set;}

        [XmlElement("lookupfield")]
        public string Field { get; set; }

        [XmlElement("primaryentity")]
        public string PrimaryEntityLogicName { get; set; }

        [XmlElement("primaryfield")]
        public string PrimaryEntityFieldLogicName { get; set; }
    }

    [XmlRoot("nnrelationship", Namespace = "urn:nextlabs-crm-schema")]
    public class NNRelationship
    {
        [XmlElement("relatedentity")]
        public string RelatedEntity { get; set; }

        [XmlArray("relationships"), XmlArrayItem("relationship")]
        public List<NNRelationShipItem> Relationships { get; set; }

        public NNRelationShipItem FindRelationshipByName(string relationshipName)
        {
            foreach (NNRelationShipItem item in Relationships)
            {
                if (item.Name.Equals(relationshipName, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }

            return null;
        }
    }
}
