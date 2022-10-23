using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel
{
    public class PolicyModelRepModel
    {
        [DataContract]
        public class Root
        {
            [DataMember]
            public string statusCode { get; set; }
            [DataMember]
            public string message { get; set; }
            [DataMember]
            public List<DataItem> data { get; set; }
            [DataMember]
            public string pageNo { get; set; }
            [DataMember]
            public string pageSize { get; set; }
            [DataMember]
            public string totalPages { get; set; }
            [DataMember]
            public string totalNoOfRecords { get; set; }
        }

        [DataContract]
        public class DataItem
        {
            [DataMember]
            public int id { get; set; }
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
            public List<Tag> tags { get; set; }
            [DataMember]
            public List<string> attributes { get; set; }
            [DataMember]
            public List<string> actions { get; set; }
            [DataMember]
            public List<string> obligations { get; set; }
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
        public class Tag
        {
            [DataMember]
            public string id { get; set; }
            [DataMember]
            public string key { get; set; }
            [DataMember]
            public string label { get; set; }
            [DataMember]
            public string type { get; set; }
            [DataMember]
            public string status { get; set; }
        }
    }

    public class TagsRepModel
    {
        [DataContract]
        public class DataItem
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

        }
        [DataContract]
        public class Root
        {
            [DataMember]
            public string statusCode { get; set; }
            [DataMember]
            public string message { get; set; }
            [DataMember]
            public List<DataItem> data { get; set; }
            [DataMember]
            public int pageNo { get; set; }
            [DataMember]
            public int pageSize { get; set; }
            [DataMember]
            public int totalPages { get; set; }
            [DataMember]
            public int totalNoOfRecords { get; set; }

            public Root() {
                data = new List<DataItem>();
            }

        }
    }

    public class AddTagRepModel
    {
        [DataContract]
        public class Root
        {
            [DataMember]
            public string statusCode { get; set; }
            [DataMember]
            public string message { get; set; }
            [DataMember]
            public int data { get; set; }
        }
    }

    public class RepStatusModel
    {
        [DataContract]
        public class Root
        {
            [DataMember]
            public string statusCode { get; set; }
            [DataMember]
            public string message { get; set; }
        }
    }

    public class SystemVersion
    {
        [DataContract]
        public class Root
        {
            [DataMember]
            public string statusCode { get; set; }
            [DataMember]
            public string message { get; set; }
            [DataMember]
            public string data { get; set; }

        }
    }
}
