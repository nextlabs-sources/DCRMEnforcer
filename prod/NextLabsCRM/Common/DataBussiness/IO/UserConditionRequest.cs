using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    abstract public class UserConditionRequest:IRecordRequest
    {
        protected QueryExpression m_query = new QueryExpression();
        protected IOrganizationService m_service;
        protected Guid m_userid;
        
        abstract protected void InitQuery();

        public UserConditionRequest(IOrganizationService service, Guid userid)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service cannot be null");
            }
            m_userid = userid;
            m_service = service;
            
        }
        protected void Init()
        {
            InitQuery();
        }
        public QueryBase GetQuery()
        {
            return m_query;
        }

        public IRecordResponse Execute()
        {
            if (m_service == null)
            {
                return null;
            }

            EntityCollection ec = EntityDataHelp.RetrieveMultiple(m_service, m_query);

            return CreateResponse(ec);
        }

        abstract protected IRecordResponse CreateResponse(EntityCollection ec);
    }
}
