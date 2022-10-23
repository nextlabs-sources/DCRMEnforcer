using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel
{
    //Bug 54930 - Unable to import action to CC automatically when secured entities(CC87) 
    //public class NxlCreateActionResponse
    //{
    //    [DataContract]
    //    public class Root
    //    {
    //        [DataMember]
    //        public string statusCode { get; set; }

    //        [DataMember]
    //        public string message { get; set; }

    //        [DataMember]
    //        public int data { get; set; }
    //    }
    //}

    //public class NxlListActionResponse
    //{
    //    [DataContract]
    //    public class PredicateData
    //    {
    //        [DataMember(Name = "operator")]
    //        public string _operator { get; set; }

    //        [DataMember]
    //        public List<string> referenceIds { get; set; }

    //        [DataMember]
    //        public List<string> attributes { get; set; }

    //        [DataMember]
    //        public List<string> actions { get; set; }
    //    }

    //    [DataContract]
    //    public class TagsItem
    //    {
    //        [DataMember]
    //        public int id { get; set; }

    //        [DataMember]
    //        public string key { get; set; }

    //        [DataMember]
    //        public string label { get; set; }

    //        [DataMember]
    //        public string type { get; set; }

    //        [DataMember]
    //        public string status { get; set; }
    //    }

    //    [DataContract]
    //    public class AuthoritiesItem
    //    {
    //        [DataMember]
    //        public string authority { get; set; }
    //    }

    //    [DataContract]
    //    public class DataItem
    //    {
    //        [DataMember]
    //        public int id { get; set; }

    //        [DataMember]
    //        public string name { get; set; }

    //        [DataMember]
    //        public string lowercase_name { get; set; }

    //        [DataMember]
    //        public string fullName { get; set; }

    //        [DataMember]
    //        public string description { get; set; }

    //        [DataMember]
    //        public string status { get; set; }

    //        [DataMember]
    //        public int modelId { get; set; }

    //        [DataMember]
    //        public string modelType { get; set; }

    //        [DataMember]
    //        public string group { get; set; }

    //        [DataMember]
    //        public int lastUpdatedDate { get; set; }

    //        [DataMember]
    //        public int createdDate { get; set; }

    //        [DataMember]
    //        public int ownerId { get; set; }

    //        [DataMember]
    //        public string ownerDisplayName { get; set; }

    //        [DataMember]
    //        public int modifiedById { get; set; }

    //        [DataMember]
    //        public string modifiedBy { get; set; }

    //        [DataMember]
    //        public string hasIncludedIn { get; set; }

    //        [DataMember]
    //        public string hasSubComponents { get; set; }

    //        [DataMember]
    //        public PredicateData predicateData { get; set; }

    //        [DataMember]
    //        public List<TagsItem> tags { get; set; }

    //        [DataMember]
    //        public List<string> includedInComponents { get; set; }

    //        [DataMember]
    //        public List<string> subComponents { get; set; }

    //        [DataMember]
    //        public string deployed { get; set; }

    //        [DataMember]
    //        public string actionType { get; set; }

    //        [DataMember]
    //        public int revisionCount { get; set; }

    //        [DataMember]
    //        public string empty { get; set; }

    //        [DataMember]
    //        public int version { get; set; }

    //        [DataMember]
    //        public List<AuthoritiesItem> authorities { get; set; }

    //        [DataMember]
    //        public string preCreated { get; set; }
    //    }

    //    [DataContract]
    //    public class Root
    //    {
    //        [DataMember]
    //        public string statusCode { get; set; }

    //        [DataMember]
    //        public string message { get; set; }

    //        [DataMember]
    //        public List<DataItem> data { get; set; }

    //        [DataMember]
    //        public int pageNo { get; set; }

    //        [DataMember]
    //        public int pageSize { get; set; }

    //        [DataMember]
    //        public int totalPages { get; set; }

    //        [DataMember]
    //        public int totalNoOfRecords { get; set; }
    //    }
    //}
}
