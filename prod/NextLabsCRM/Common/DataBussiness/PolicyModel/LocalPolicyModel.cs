using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel
{
    [DataContract]
    public class LocalPolicyModel
    {
        [DataContract]
        public class Root
        {
            [DataMember]
            public List<PolicyModelsItem> policyModels { get; set; }

            [DataMember]
            public List<ComponentsItem> components { get; set; }

            [DataMember]
            public PolicyTree policyTree { get; set; }

            [DataMember]
            public List<string> importedPolicyIds { get; set; }

            [DataMember]
            public string overrideDuplicates { get; set; }

            [DataMember]
            public ComponentToSubCompMap componentToSubCompMap { get; set; }
        }
        [DataContract]
        public class PolicyTree
        {
        }
        [DataContract]
        public class ComponentToSubCompMap
        {
        }
        [DataContract]
        public class ComponentsItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string description { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string category { get; set; }

            [DataMember]
            public PolicyModel policyModel { get; set; }

            [DataMember]
            public List<string> actions { get; set; }

            [DataMember]
            public string status { get; set; }
        }
        [DataContract]
        public class PolicyModel
        {
            [DataMember]
            public string id { get; set; }
        }
        [DataContract]
        public class PolicyModelsItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string description { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string status { get; set; }

            [DataMember]
            public List<AttributesItem> attributes { get; set; }

            [DataMember]
            public List<ActionsItem> actions { get; set; }

            [DataMember]
            public List<ObligationsItem> obligations { get; set; }

            [DataMember]
            public List<TagsItem> tags { get; set; }

            public PolicyModelsItem() {
                attributes = new List<AttributesItem>();
                actions = new List<ActionsItem>();
                obligations = new List<ObligationsItem>();
                tags = new List<TagsItem>();
            }
        }
        [DataContract]
        public class TagsItem
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string key { get; set; }

            [DataMember]
            public string label { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string status { get; set; }

            public TagsItem()
            {

            }
        }
        [DataContract]
        public class ObligationsItem
        {

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string runAt { get; set; }

            [DataMember]
            public int sortOrder { get; set; }

            [DataMember]
            public List<ParametersItem> parameters { get; set; }

            public ObligationsItem()
            {
            }
        }
        [DataContract]
        public class ParametersItem
        {
            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string type { get; set; }

            [DataMember]
            public string defaultValue { get; set; }

            [DataMember]
            public string listValues { get; set; }

            [DataMember]
            public bool hidden { get; set; }

            [DataMember]
            public bool editable { get; set; }

            [DataMember]
            public bool mandatory { get; set; }

            [DataMember]
            public int sortOrder { get; set; }

            public ParametersItem()
            {
            }
        }
        [DataContract]
        public class ActionsItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public int sortOrder { get; set; }

            public ActionsItem()
            {
            }
        }
        [DataContract]
        public class AttributesItem
        {
            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string sortOrder { get; set; }

            [DataMember]
            public string dataType { get; set; }

            [DataMember]
            public List<OperatorConfigsItem> operatorConfigs { get; set; }

            public AttributesItem()
            {
            }
        }
        [DataContract]
        public class OperatorConfigsItem
        {

            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string key { get; set; }

            [DataMember]
            public string label { get; set; }

            [DataMember]
            public string dataType { get; set; }

            public OperatorConfigsItem()
            {
            }
        }
    }
}
 