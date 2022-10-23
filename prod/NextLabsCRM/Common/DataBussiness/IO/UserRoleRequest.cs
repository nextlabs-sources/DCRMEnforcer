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
    public class UserRoleRequest: UserConditionRequest
    {
        private SecureEntity m_roleMetadata;
        private bool m_isPrimaryIDEnforced = false;
        public UserRoleRequest(IOrganizationService service, Guid userid,SecureEntity roleMetaData):
            base(service,userid)
        {

            if (roleMetaData == null)
            {
                throw new ArgumentNullException("The meta data of role cannot be null");
            }
            m_roleMetadata = roleMetaData;

            Init();
        }

        protected override void InitQuery()
        {
            Relation relation = new N1Relation(
                "roles_systemuserroles", 
                "role",
                "systemuserroles",
                "roleid",
                "roleid");

            LinkEntity linkEntity = relation.ToQueryLinkEntity();

            HashSet<string> attributeNames = new HashSet<string>();
            foreach(DataModel.MetaData.Attribute attr in m_roleMetadata.Schema.Attributes)
            {
                attributeNames.Add(attr.LogicName);

                if(attr.LogicName.Equals("roleid",StringComparison.OrdinalIgnoreCase))
                {
                    m_isPrimaryIDEnforced = true;
                }
            }
            m_query.EntityName = "role";
            m_query.Distinct = true;
            m_query.ColumnSet.AddColumns(attributeNames.ToArray());
            if(!m_isPrimaryIDEnforced)
            {
                m_query.ColumnSet.AddColumn("roleid");
            }

            ConditionExpression cond = new ConditionExpression("systemuserid", ConditionOperator.Equal, m_userid);
            cond.EntityName = relation.ParentEntityAlias;
            m_query.Criteria.AddCondition(cond);
            m_query.LinkEntities.Add(linkEntity);
        }

        protected override IRecordResponse CreateResponse(EntityCollection ec)
        {
            return new UserRoleResponse(ec, m_isPrimaryIDEnforced);
        }
    }
}
