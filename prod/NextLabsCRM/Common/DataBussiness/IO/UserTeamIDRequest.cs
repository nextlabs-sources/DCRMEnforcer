using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class UserTeamIDRequest: IRecordIDRequest
    {
        QueryExpression m_query = new QueryExpression();
        private IOrganizationService m_service;
        private Guid m_userid;
        public UserTeamIDRequest(IOrganizationService service, Guid userid)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service cannot be null");
            }
            m_userid = userid;
            m_service = service;
            InitQuery();
        }

        private void InitQuery()
        {
            m_query.EntityName = "teammembership";
            m_query.Distinct = true;
            m_query.ColumnSet.AddColumn("teamid");
            //m_query.Criteria.AddCondition("systemuserid", ConditionOperator.EqualUserId);
            //EqualUserId cannot work here...
            m_query.Criteria.AddCondition("systemuserid", ConditionOperator.Equal, m_userid);
        }

        public QueryBase GetQuery()
        {
            return m_query;
        }

        public IRecordIDResponse Execute()
        {
            if (m_service == null)
            {
                return null;
            }

            EntityCollection ec = EntityDataHelp.RetrieveMultiple(m_service, m_query);

            return new UserTeamIDResponse(ec);
        }
    }
}
