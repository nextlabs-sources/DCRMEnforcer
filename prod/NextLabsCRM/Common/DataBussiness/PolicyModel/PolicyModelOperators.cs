using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel
{
    public class PolicyModelOperators
    {
        [DataContract]
        public class OperatorElem
        {
            [DataMember]
            public int id { get; set; }

            [DataMember]
            public string key { get; set; }

            [DataMember]
            public string label { get; set; }

            [DataMember]
            public string dataType { get; set; }

            public OperatorElem() { }
        }

        [DataContract]
        public class StringOperator {
            [DataMember]
            public string statusCode { get; set; }

            [DataMember]
            public string message { get; set; }

            [DataMember]
            public List<OperatorElem> data = new List<OperatorElem>();

            [DataMember]
            public int pageNo { get; set; }

            [DataMember]
            public int pageSize { get; set; }

            [DataMember]
            public int totalPages { get; set; }

            [DataMember]
            public int totalNoOfRecords { get; set; }
        }

        [DataContract]
        public class NumberOperator {
            [DataMember]
            public string statusCode { get; set; }

            [DataMember]
            public string message { get; set; }

            [DataMember]
            public List<OperatorElem> data = new List<OperatorElem>();

            [DataMember]
            public int pageNo { get; set; }

            [DataMember]
            public int pageSize { get; set; }

            [DataMember]
            public int totalPages { get; set; }

            [DataMember]
            public int totalNoOfRecords { get; set; }
        }

        [DataContract]
        public class MultivalOperator {
            [DataMember]
            public string statusCode { get; set; }

            [DataMember]
            public string message { get; set; }

            [DataMember]
            public List<OperatorElem> data = new List<OperatorElem>();

            [DataMember]
            public int pageNo { get; set; }

            [DataMember]
            public int pageSize { get; set; }

            [DataMember]
            public int totalPages { get; set; }

            [DataMember]
            public int totalNoOfRecords { get; set; }
        }
    }
}
