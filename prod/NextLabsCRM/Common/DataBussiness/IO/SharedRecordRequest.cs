using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class SharedRecordRequest
    {
        private const string m_userRelationshipNamePrefix = "isshareduser_";
        private const string m_teamRelationshipNamePrefix = "issharedteam_";
        private const string m_teammembershipEntity = "teammembership";
        private const string m_shareRelationshipEntity = "principalobjectaccess";
        private const string m_sharedRecordAttrName = "objectid";
        private const string m_sharedAccessRightsMask = "accessrightsmask";
        private const string m_sharerAttrName = "principalid";
        private const string m_sharedRecordType = "objecttypecode";
        private List<Guid> m_toTeamIds = new List<Guid>();
        private MemoryCache<DataModel.SecureEntity> m_entityMetadataCache;
        private List<Guid> m_teamids;
        private IOrganizationService m_service;
        private Guid m_userid;
        private QueryExpression m_query = new QueryExpression();
             
        public SharedRecordRequest(IOrganizationService service,
            MemoryCache<DataModel.SecureEntity> entityMetadataCache, 
            Guid userid, 
            List<Guid> teamids = null)
        {
            if(entityMetadataCache == null)
            {
                throw new ArgumentException("entity list cannot be null and empty");
            }

            if(service == null)
            {
                throw new ArgumentNullException("service cannot be null");
            }

            m_entityMetadataCache = entityMetadataCache;
            m_service = service;
            m_userid = userid;
            m_teamids = teamids;
            InitQuery();
        }

        public SharedRecordResponse Execute()
        {
            if (m_service == null)
            {
                return null;
            }

            EntityCollection ec = EntityDataHelp.RetrieveMultiple(m_service, m_query);

            return CreateResponse(ec);
        }

        protected SharedRecordResponse CreateResponse(EntityCollection ec)
        {
            return new SharedRecordResponse(m_entityMetadataCache, ec);
        }

        private ConditionExpression CreatePrincipleUserEqualCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = m_sharerAttrName;
            cond.Operator = ConditionOperator.Equal;
            cond.Values.Add(m_userid);
            return cond;
        }

        private ConditionExpression CreatePrincipleInTeamEqualCondition()
        {
            ConditionExpression cond = new ConditionExpression(m_sharerAttrName,ConditionOperator.In,m_teamids.ToArray());
            return cond;
        }

        private ConditionExpression CreateFilterOutInheritCondition()
        {
            ConditionExpression cond = new ConditionExpression(m_sharedAccessRightsMask, ConditionOperator.NotEqual, 0);
            return cond;
        }

        private FilterExpression CreatePrincipleUserOrInTeamEqualFilter()
        {

            ConditionExpression userCond = CreatePrincipleUserEqualCondition();
            ConditionExpression teamCond = CreatePrincipleInTeamEqualCondition();

            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(userCond);
            filter.AddCondition(teamCond);

            FilterExpression topFilter = new FilterExpression(LogicalOperator.And);
            topFilter.AddFilter(filter);
            //topFilter.AddCondition(CreateObjectTypeCodeCondition());
            topFilter.AddCondition(CreateFilterOutInheritCondition());
            return topFilter;
        }


        private ConditionExpression CreatePrincipleTeamNotNullCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = "teamid";
            cond.Operator = ConditionOperator.NotNull;
            return cond;
        }

        //private ConditionExpression CreateObjectTypeCodeCondition()
        //{
        //    List<string> objectTypeCodes = new List<string>();
        //    foreach (DataModel.SecureEntity entityMetaData in m_securedEntities)
        //    {
        //        objectTypeCodes.Add(entityMetaData.Schema.LogicName);
        //    }
        //    ConditionExpression cond = new ConditionExpression(
        //        m_sharedRecordType,
        //        ConditionOperator.In,
        //        objectTypeCodes.ToArray());

        //    return cond;
        //}

        private Relation CreateRelation()
        {
            /*
             * select * from principalobjectaccess left join
             * teammember as TM on TM.teamid = principalid
             * where principalid = userid or TM.teamid is not null
             */

            Relation relation = new N1Relation(m_shareRelationshipEntity + "teammembership",
                m_shareRelationshipEntity,
                "teammembership",
                m_sharerAttrName,
                "teamid");

            return relation;
        }
        protected void InitQuery()
        {
            m_query.EntityName = m_shareRelationshipEntity;
            m_query.Criteria.FilterOperator = LogicalOperator.And;
            m_query.Distinct = true;
            m_query.ColumnSet.AddColumn(m_sharedRecordAttrName);
            m_query.ColumnSet.AddColumn(m_sharedRecordType);

            if (m_teamids != null)
            {
                m_query.Criteria = CreatePrincipleUserOrInTeamEqualFilter();
            }
            else
            {
                Relation relation = CreateRelation();
                m_query.LinkEntities.Add(relation.ToQueryLinkEntity());
                ConditionExpression userCond = CreatePrincipleUserEqualCondition();
                ConditionExpression teamCond = CreatePrincipleTeamNotNullCondition();

                FilterExpression filter = new FilterExpression(LogicalOperator.Or);
                filter.AddCondition(userCond);
                filter.AddCondition(teamCond);
                teamCond.EntityName = relation.ParentEntityAlias;

                FilterExpression topFilter = new FilterExpression(LogicalOperator.And);
                topFilter.AddFilter(filter);
                //topFilter.AddCondition(CreateObjectTypeCodeCondition());
                topFilter.AddCondition(CreateFilterOutInheritCondition());

                m_query.Criteria = topFilter;
            }
        }
    }
}
