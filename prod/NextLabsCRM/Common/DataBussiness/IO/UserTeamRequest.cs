using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    /*
     * this class should only be used in the case when isowner is in a negtive case
     * such as "isowner eq false" or "isowner noteq true"
     */
    public class UserTeamRequest:UserConditionRequest
    {
        private SecureEntity m_teamMetadata;
        private bool m_isPrimaryIDEnforced = false;
        public UserTeamRequest(IOrganizationService service,Guid userid, SecureEntity teamMetadata)
            :base(service,userid)
        {
            if (teamMetadata == null)
            {
                throw new ArgumentNullException("The meta data of role cannot be null");
            }

            m_teamMetadata = teamMetadata;
            Init();
        }

        protected override void InitQuery()
        {
            Relation relation = new N1Relation(
                "teams_systemuserteam",
                "team",
                "teammembership",
                "teamid",
                "teamid");

            LinkEntity linkEntity = relation.ToQueryLinkEntity();

            HashSet<string> attributeNames = new HashSet<string>();
            foreach (DataModel.MetaData.Attribute attr in m_teamMetadata.Schema.Attributes)
            {
                attributeNames.Add(attr.LogicName);

                if (attr.LogicName.Equals("teamid", StringComparison.OrdinalIgnoreCase))
                {
                    m_isPrimaryIDEnforced = true;
                }
            }
            m_query.EntityName = "team";
            m_query.Distinct = true;
            m_query.ColumnSet.AddColumns(attributeNames.ToArray());
            if (!m_isPrimaryIDEnforced)
            {
                m_query.ColumnSet.AddColumn("teamid");
            }

            ConditionExpression cond = new ConditionExpression("systemuserid", ConditionOperator.Equal, m_userid);
            cond.EntityName = relation.ParentEntityAlias;
            m_query.Criteria.AddCondition(cond);
            m_query.LinkEntities.Add(linkEntity);
        }

        protected override IRecordResponse CreateResponse(EntityCollection ec)
        {
            return new UserTeamResponse(ec, m_isPrimaryIDEnforced);
        }
    }
}
