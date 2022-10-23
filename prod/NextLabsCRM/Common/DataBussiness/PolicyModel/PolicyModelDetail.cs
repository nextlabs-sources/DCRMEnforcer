using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel
{
    public class PolicyModelDetail
    {
        [DataContract]
        public class Root
        {
            [DataMember]
            public string statusCode { get; set; }

            [DataMember]
            public string message { get; set; }

            [DataMember]
            public Data data { get; set; }
        }

        [DataContract]
        public class Data
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
            public string lastUpdatedDate { get; set; }

            [DataMember]
            public string createdDate { get; set; }

            [DataMember]
            public string ownerId { get; set; }

            [DataMember]
            public string ownerDisplayName { get; set; }

            [DataMember]
            public string modifiedById { get; set; }

            [DataMember]
            public string modifiedBy { get; set; }

            [DataMember]
            public List<TagsItem> tags { get; set; }

            [DataMember]
            public List<AttributesItem> attributes { get; set; }

            [DataMember]
            public List<ActionsItem> actions { get; set; }

            [DataMember]
            public List<ObligationsItem> obligations { get; set; }

            [DataMember]
            public List<AuthoritiesItem> authorities { get; set; }

            [DataMember]
            public string version { get; set; }
        }

        [DataContract]
        public class AuthoritiesItem
        {
            [DataMember]
            public string authority { get; set; }
        }

        [DataContract]
        public class ObligationsItem
        {
            [DataMember]
            public string version { get; set; }

            [DataMember]
            public string lastUpdatedDate { get; set; }

            [DataMember]
            public string createdDate { get; set; }

            [DataMember]
            public string ownerId { get; set; }

            [DataMember]
            public string lastUpdatedBy { get; set; }

            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string runAt { get; set; }

            [DataMember]
            public List<ParametersItem> parameters { get; set; }

            [DataMember]
            public string isReferenced { get; set; }

            [DataMember]
            public int sortOrder { get; set; }
        }

        [DataContract]
        public class ParametersItem
        {
            [DataMember]
            public string version { get; set; }

            [DataMember]
            public string lastUpdatedDate { get; set; }

            [DataMember]
            public string createdDate { get; set; }

            [DataMember]
            public string ownerId { get; set; }

            [DataMember]
            public string lastUpdatedBy { get; set; }

            [DataMember]
            public string id { get; set; }

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
        }

        [DataContract]
        public class ActionsItem
        {

            [DataMember]
            public string version { get; set; }

            [DataMember]
            public string lastUpdatedDate { get; set; }

            [DataMember]
            public string createdDate { get; set; }

            [DataMember]
            public string ownerId { get; set; }

            [DataMember]
            public string lastUpdatedBy { get; set; }

            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string shortCode { get; set; }

            [DataMember]
            public string isReferenced { get; set; }

            [DataMember]
            public int sortOrder { get; set; }
        }

        [DataContract]
        public class AttributesItem
        {

            [DataMember]
            public string version { get; set; }

            [DataMember]
            public string lastUpdatedDate { get; set; }

            [DataMember]
            public string createdDate { get; set; }

            [DataMember]
            public string ownerId { get; set; }

            [DataMember]
            public string lastUpdatedBy { get; set; }

            [DataMember]
            public string id { get; set; }

            [DataMember]
            public string name { get; set; }

            [DataMember]
            public string shortName { get; set; }

            [DataMember]
            public string dataType { get; set; }

            [DataMember]
            public List<OperatorConfigsItem> operatorConfigs { get; set; }

            [DataMember]
            public string regExPattern { get; set; }

            [DataMember]
            public string policyModel { get; set; }

            [DataMember]
            public string isReferenced { get; set; }

            [DataMember]
            public int sortOrder { get; set; }
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
            public TagsItem(int id1, string key1, string label1, string type1, string status1)
            {
                id = id1;
                key = key1;
                label = label1;
                type = type1;
                status = status1;
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
