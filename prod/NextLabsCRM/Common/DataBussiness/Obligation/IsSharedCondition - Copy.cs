using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    class IsSharedCondition: Condition
    {
        private const string m_relationshipEntity = "principalobjectaccess";
        private const string m_sharerAttrName = "principalid";
        private const string m_recordAttrName = "objectid";
        private const string m_isSharedAttribute = "nxl_isshared";

        private Guid m_userid;
        private string m_entityName;
        private string m_entityPrimaryId;

        private FilterExpression m_positiveQueryFilter;
        private FilterExpression m_negativeQueryFilter;
        private filter m_positiveFetchFilter;
        private filter m_negativeFetchFilter;

        private List<LinkEntity> m_queryLinkEntities;
        private List<FetchLinkEntityType> m_fetchLinkEntities;


        /*delegate void AppendLinkEntity(LinkEntity parentEntity, List<LinkEntity> childEntities);

        void AppendChildLinkEntity(List<LinkEntity> entities, List<LinkEntity> childEntities)
        {
            //<alias,linkentity>
            Dictionary<string, LinkEntity> endpointEntities = new Dictionary<string, LinkEntity>();
            foreach (LinkEntity entity in entities)
            {
                GetLinkEnititesWithAlias(entity, endpointEntities);
            }


        }
        void GetLinkEnititesWithAlias(LinkEntity parentEntity, Dictionary<string,LinkEntity> endpointEntities)
        {
            if(parentEntity == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(parentEntity.EntityAlias))
            {
                endpointEntities[parentEntity.EntityAlias] = parentEntity;
            }

            if(parentEntity.LinkEntities == null || parentEntity.LinkEntities.Count == 0)
            {
                return;
            }
            foreach(LinkEntity linkEntity in parentEntity.LinkEntities)
            {
                GetLinkEnititesWithAlias(linkEntity, endpointEntities);
            }
        }
        public void UpdateEntityItem(List<LinkEntity> entities, List<ApplySecurityFilter> appSeurityFilters)
        {
            
        }*/
        public IsSharedCondition(string entityName, Guid userid)
        {
            if(userid == null)
            {
                throw new ArgumentNullException("user id cannot be null for the share condition initialization.");
            }

            if(string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entity name cannot be null or blank for the share condition initialization.");
            }
            m_userid = userid;
            m_entityName = entityName;
            m_entityPrimaryId = entityName + "id";
        }

        /*public IsSharedCondition And(IsSharedCondition source)
        {
            IsSharedCondition target = new IsSharedCondition(m_entityName,m_userid);
            throw new NotImplementedException();

        }*/

        /*public void Generate()
        {
            CreateEntityItem(m_entityName, m_userid, out m_queryLinkEntities, out m_positiveQueryFilter, out m_negativeQueryFilter);
            CreateEntityItem(m_entityName, m_userid, out m_fetchLinkEntities, out m_positiveFetchFilter, out m_negativeFetchFilter);
        }*/
        /*public IsSharedCondition Or(IsSharedCondition source)
        {
            if(source == null)
            {
                return this;
            }

            IsSharedCondition target = new IsSharedCondition(m_entityName,m_userid);
            //target.m_queryLinkEntities

            throw new NotImplementedException();
        }*/
        public static bool IsIsSharedEnabled(SecureEntity secureEntity)
        {
            
            if (!secureEntity.Secured || secureEntity.Schema == null)
            {
                return false;
            }

            if(secureEntity.Schema.Attributes == null || secureEntity.Schema.Attributes.Count == 0)
            {
                return false;
            }

            DataModel.MetaData.Attribute isSharedAttr = 
            secureEntity.Schema.Attributes.Find((DataModel.MetaData.Attribute attr) 
            => attr.LogicName.Equals(m_isSharedAttribute, StringComparison.OrdinalIgnoreCase));

            if(isSharedAttr == null)
            {
                return false;
            }
            return true;
        }

        public static bool IsIsSharedEnabled(string entityName, MemoryCache<SecureEntity> secureEntityCache)
        {
            SecureEntity entity = secureEntityCache.Lookup(entityName, x => null);
            return IsIsSharedEnabled(entity);
        }

        public override List<Relation> GetRelations()
        {
            return CreateRelations();
        }
        private List<Relation> CreateRelations()
        {
            /*
             * select * from entity
                leftjoin principalobjectaccess as POA1 on POA1.objectid = entity.entityid
                leftjoin systemuser as SU1 on SU1.systemuserid = POA1.principalid
                leftjoin principalobjectaccess as POA2 on POA2.objectid = entity.entityid
                leftjoin teammembership as TS on TS.teamid = POA2.principalid
                where (SU1.systemuserid = userid or TS.systemuserid = userid)
             */

            NNRelation entityUserPrincipalobjectaccessRelation = new NNRelation(
                m_entityName + m_relationshipEntity,
                m_entityName,
                "systemuser",
                m_entityPrimaryId,
                "systemuserid",
                m_relationshipEntity,
                m_recordAttrName,
                m_sharerAttrName
                );

            NNRelation entityTeamUserPrincipalobjectaccessRelation = new NNRelation(
                m_entityName + m_relationshipEntity,
                m_entityName,
                "teammembership",
                m_entityPrimaryId,
                "systemuserid",
                m_relationshipEntity,
                m_recordAttrName,
                m_sharerAttrName
                );

            List<Relation> relations = new List<Relation>();
            relations.Add(entityUserPrincipalobjectaccessRelation);
            relations.Add(entityTeamUserPrincipalobjectaccessRelation);

            return relations;
        }
        private void CreateEntityItem(string entityName, Guid userid, out List<LinkEntity> linkentities, 
            out FilterExpression obligationFilter,out FilterExpression notObligationFilter)
        {
            /*
             * select * from entity
                leftjoin principalobjectaccess as POA1 on POA1.objectid = entity.entityid
                leftjoin systemuser as SU1 on SU1.systemuserid = POA1.principalid
                leftjoin principalobjectaccess as POA2 on POA2.objectid = entity.entityid
                leftjoin teammembership as TS on TS.teamid = POA2.principalid
                where (SU1.systemuserid = userid or TS.systemuserid = userid)
             */

            NNRelation entityUserPrincipalobjectaccessRelation = new NNRelation(
                entityName+ m_relationshipEntity,
                entityName,
                "systemuser",
                m_entityPrimaryId,
                "systemuserid",
                m_relationshipEntity,
                m_recordAttrName,
                m_sharerAttrName
                );

            NNRelation entityTeamUserPrincipalobjectaccessRelation = new NNRelation(
                entityName + m_relationshipEntity,
                entityName,
                "teammembership",
                m_entityPrimaryId,
                "systemuserid",
                m_relationshipEntity,
                m_recordAttrName,
                m_sharerAttrName
                );

            EntityOutJoin outJoin = new EntityOutJoin();
            outJoin.AddRelation(entityUserPrincipalobjectaccessRelation);
            outJoin.AddRelation(entityTeamUserPrincipalobjectaccessRelation);
            linkentities = outJoin.ToQueryLinkEntities();


            FilterExpression isSharedObligationFilter = new FilterExpression(LogicalOperator.Or);
            ConditionExpression userIsSharerCond = new ConditionExpression();
            userIsSharerCond.EntityName = entityUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userIsSharerCond.AttributeName = "systemuserid";
            userIsSharerCond.Operator = ConditionOperator.Equal;
            userIsSharerCond.Values.Add(userid);

            ConditionExpression userTeamIsSharerCond = new ConditionExpression();
            userTeamIsSharerCond.EntityName = entityTeamUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userTeamIsSharerCond.AttributeName = "systemuserid";
            userTeamIsSharerCond.Operator = ConditionOperator.Equal;
            userTeamIsSharerCond.Values.Add(userid);

            isSharedObligationFilter.AddCondition(userIsSharerCond);
            isSharedObligationFilter.AddCondition(userTeamIsSharerCond);
            obligationFilter = isSharedObligationFilter;

            //----------------------------------
            /*
             * select * from entity
                leftjoin principalobjectaccess as POA1 on POA1.objectid = entity.entityid
                leftjoin systemuser as SU1 on SU1.systemuserid = POA1.principalid
                leftjoin principalobjectaccess as POA2 on POA2.objectid = entity.entityid
                leftjoin teammembership as TS on TS.teamid = POA2.principalid
                where (SU1.systemuserid <> userid and TS.systemuserid <> userid) or
                (SU1.systemuserid is null and TS.systemuserid is null)
             */
            FilterExpression isNotSharedObligationFilter = new FilterExpression(LogicalOperator.And);
            ConditionExpression userIsNotSharerCond = new ConditionExpression();
            userIsNotSharerCond.EntityName = entityUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userIsNotSharerCond.AttributeName = "systemuserid";
            userIsNotSharerCond.Operator = ConditionOperator.NotEqual;
            userIsNotSharerCond.Values.Add(userid);

            ConditionExpression userTeamIsNotSharerCond = new ConditionExpression();
            userTeamIsNotSharerCond.EntityName = entityTeamUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userTeamIsNotSharerCond.AttributeName = "systemuserid";
            userTeamIsNotSharerCond.Operator = ConditionOperator.Equal;
            userTeamIsNotSharerCond.Values.Add(userid);

            isNotSharedObligationFilter.AddCondition(userIsNotSharerCond);
            isNotSharedObligationFilter.AddCondition(userTeamIsNotSharerCond);

            FilterExpression isNotNullUserFilter = new FilterExpression(LogicalOperator.And);
            ConditionExpression userNullCond = new ConditionExpression();
            userIsNotSharerCond.EntityName = entityUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userIsNotSharerCond.AttributeName = "systemuserid";
            userIsNotSharerCond.Operator = ConditionOperator.Null;

            ConditionExpression teamuserNullCond = new ConditionExpression();
            userTeamIsNotSharerCond.EntityName = entityTeamUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userTeamIsNotSharerCond.AttributeName = "systemuserid";
            userTeamIsNotSharerCond.Operator = ConditionOperator.Null;

            isNotNullUserFilter.AddCondition(userNullCond);
            isNotNullUserFilter.AddCondition(teamuserNullCond);

            FilterExpression totalNotFilter = new FilterExpression(LogicalOperator.Or);
            totalNotFilter.AddFilter(isNotSharedObligationFilter);
            totalNotFilter.AddFilter(isNotNullUserFilter);

            notObligationFilter = totalNotFilter;
        }


        public void CreateEntityItem(string entityName, Guid userid, out List<FetchLinkEntityType> linkentities, 
            out filter obligationFilter, out filter notObligationFilter)
        {
            linkentities = new List<FetchLinkEntityType>();
            NNRelation entityUserPrincipalobjectaccessRelation = new NNRelation(
                entityName + m_relationshipEntity,
                entityName,
                "systemuser",
                m_entityPrimaryId,
                "systemuserid",
                m_relationshipEntity,
                m_recordAttrName,
                m_sharerAttrName
                );

            FetchLinkEntityType entityUserLinkEntity = entityUserPrincipalobjectaccessRelation.ToFetchLinkEntity();


            NNRelation entityTeamUserPrincipalobjectaccessRelation = new NNRelation(
                entityName + m_relationshipEntity,
                entityName,
                "teammembership",
                m_entityPrimaryId,
                "systemuserid",
                m_relationshipEntity,
                m_recordAttrName,
                m_sharerAttrName
                );

            FetchLinkEntityType entityTeamUserLinkEntity = entityTeamUserPrincipalobjectaccessRelation.ToFetchLinkEntity();

            linkentities.Add(entityUserLinkEntity);
            linkentities.Add(entityTeamUserLinkEntity);

            filter isSharedObligationFilter = new filter();
            isSharedObligationFilter.Items = new List<object>();
            isSharedObligationFilter.type = filterType.or;
            condition userIsSharerCond = new condition();
            userIsSharerCond.entityname = entityUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userIsSharerCond.attribute = "systemuserid";
            userIsSharerCond.@operator = @operator.eq;
            userIsSharerCond.value = userid.ToString();

            condition userTeamIsSharerCond = new condition();
            userTeamIsSharerCond.entityname = entityTeamUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userTeamIsSharerCond.attribute = "systemuserid";
            userTeamIsSharerCond.@operator = @operator.eq;
            userTeamIsSharerCond.value = userid.ToString();

            isSharedObligationFilter.Items.Add(userIsSharerCond);
            isSharedObligationFilter.Items.Add(userTeamIsSharerCond);
            obligationFilter = isSharedObligationFilter;

            //----------------------------------
            filter isNotSharedObligationFilter = new filter();
            isNotSharedObligationFilter.Items = new List<object>();
            isNotSharedObligationFilter.type = filterType.and;
            condition userIsNotSharerCond = new condition();
            userIsNotSharerCond.entityname = entityUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userIsNotSharerCond.attribute = "systemuserid";
            userIsNotSharerCond.@operator = @operator.neq;
            userIsNotSharerCond.value = userid.ToString();

            condition userTeamIsNotSharerCond = new condition();
            userTeamIsNotSharerCond.entityname = entityTeamUserPrincipalobjectaccessRelation.ParentEntityAlias;
            userTeamIsNotSharerCond.attribute = "systemuserid";
            userTeamIsNotSharerCond.@operator = @operator.neq;
            userTeamIsNotSharerCond.value = userid.ToString();

            isNotSharedObligationFilter.Items.Add(userIsNotSharerCond);
            isNotSharedObligationFilter.Items.Add(userTeamIsNotSharerCond);
            notObligationFilter = isNotSharedObligationFilter;
        }

        public QueryExpression CreateQueryExpressionByRecord(Guid recordid)
        {
            FilterExpression topFilter = new FilterExpression(LogicalOperator.And);
            ConditionExpression recordCond = new ConditionExpression();
            recordCond.AttributeName = m_entityPrimaryId;
            recordCond.Operator = ConditionOperator.Equal;
            recordCond.Values.Add(recordid);
            topFilter.AddCondition(recordCond);
            topFilter.AddFilter(m_positiveQueryFilter);

            QueryExpression qe = new QueryExpression();
            qe.EntityName = m_entityName;
            qe.ColumnSet = new ColumnSet(m_entityPrimaryId);
            qe.LinkEntities.AddRange(m_queryLinkEntities);
            qe.Criteria = topFilter;

            return qe;
        }

        public FetchExpression CreateFetchExpressionByRecord(Guid recordid)
        {
            filter topFilter = new filter();
            topFilter.type = filterType.and;
            topFilter.Items = new List<object>();


            condition recordCond = new condition();
            recordCond.attribute = m_entityPrimaryId;
            recordCond.@operator = @operator.eq;
            recordCond.value = recordid.ToString();
            topFilter.Items.Add(recordCond);
            topFilter.Items.Add(m_positiveFetchFilter);

            FetchType fetch = new FetchType();
            fetch.Items = new List<object>();

            FetchEntityType entity = new FetchEntityType();
            entity.Items = new List<object>();
            entity.name = m_entityName;

            FetchAttributeType attr = new FetchAttributeType();
            attr.name = m_entityPrimaryId;
           
            entity.Items.Add(topFilter);
            entity.Items.Add(attr);
            entity.Items.AddRange(m_fetchLinkEntities);

            IStringSerialize serializer = new XMLSerializeHelper();
            string xml = serializer.SerializeWithoutNamespace(typeof(FetchType), fetch);
            FetchExpression fe = new FetchExpression(xml);
            return fe;
        }

        





        [Obsolete]
        public QueryExpression GetQueryExpressionXXX(Guid userid, Guid recordid)
        {
            /*select systemuserid from systemuser SU leftjoin teammembership TS on TS.systemuserid = SU.systemuserid
            leftjoin team as TM on TM.teamid = teammembership.teamid
            leftjoin principalobjectaccess PJA1 on SU.systemuserid = PJA1.principalid
            leftjoin principalobjectaccess PJA2 on TM.teamid = PJA2.principalid
            where (PJA1.objectid = recordid and SU.systemuserid = userid) 
            or (PJA2.objectid = recordid and SU.systemuserid = userid)*/

            QueryExpression qe = new QueryExpression();
            qe.EntityName = "systemuser";
            qe.ColumnSet = new ColumnSet("systemuserid");

            NNRelation teammembership = new NNRelation(
                "teammembership",
                "systemuser",
                "team",
                "systemuserid",
                "teamid",
                "teammembership",
                "systemuserid",
                "teamid"
                );

            LinkEntity teammemberLink = teammembership.ToQueryLinkEntity(false);

            N1Relation teamPrincipalobjectaccess = new N1Relation(
               "TeamPrincipalobjectaccess",          
               "team",
               m_relationshipEntity,        
               "teamid",
               m_sharerAttrName
               );

            LinkEntity teamPrincipalobjectaccessLink = teamPrincipalobjectaccess.ToQueryLinkEntity();
            teammemberLink.LinkEntities[0].LinkEntities.Add(teamPrincipalobjectaccessLink);


            //---------------------------------------------------------------------
            N1Relation userPrincipalobjectaccess = new N1Relation(
                "UserPrincipalobjectaccess",
                "systemuser",
                m_relationshipEntity,
                "systemuserid",
                m_sharerAttrName
                );

            LinkEntity userPrincipalobjectaccessLink = userPrincipalobjectaccess.ToQueryLinkEntity();

            FilterExpression topFilter = new FilterExpression(LogicalOperator.Or);
            FilterExpression userFilter = new FilterExpression(LogicalOperator.And);
            //ConditionExpression cndSharedToUser = new ConditionExpression();
            //cndSharedToUser.EntityName = userPrincipalobjectaccess.ParentEntityAlias;
            //cndSharedToUser.AttributeName = m_sharerAttrName;
            //cndSharedToUser.Operator = ConditionOperator.NotNull;

            ConditionExpression cndRecord1 = new ConditionExpression();
            cndRecord1.EntityName = userPrincipalobjectaccess.ParentEntityAlias;
            cndRecord1.AttributeName = m_recordAttrName;
            cndRecord1.Operator = ConditionOperator.Equal;
            cndRecord1.Values.Add(recordid);

            ConditionExpression cndUser1 = new ConditionExpression();
            cndUser1.EntityName = "systemuser";
            cndUser1.AttributeName = "systemuserid";
            cndUser1.Operator = ConditionOperator.Equal;
            cndUser1.Values.Add(userid);

            //userFilter.AddCondition(cndSharedToUser);
            userFilter.AddCondition(cndRecord1);
            userFilter.AddCondition(cndUser1);


            FilterExpression teamFilter = new FilterExpression(LogicalOperator.And);
            //ConditionExpression cndSharedToTeam = new ConditionExpression();
            //cndSharedToTeam.EntityName = teamPrincipalobjectaccess.ParentEntityAlias;
            //cndSharedToTeam.AttributeName = m_sharerAttrName;
            //cndSharedToTeam.Operator = ConditionOperator.NotNull;

            ConditionExpression cndRecord2 = new ConditionExpression();
            cndRecord2.EntityName = teamPrincipalobjectaccess.ParentEntityAlias;
            cndRecord2.AttributeName = m_recordAttrName;
            cndRecord2.Operator = ConditionOperator.Equal;
            cndRecord2.Values.Add(recordid);

            ConditionExpression cndUser2 = new ConditionExpression();
            cndUser2.EntityName = "systemuser";
            cndUser2.AttributeName = "systemuserid";
            cndUser2.Operator = ConditionOperator.Equal;
            cndUser2.Values.Add(userid);


            //teamFilter.AddCondition(cndSharedToTeam);
            teamFilter.AddCondition(cndRecord2);
            teamFilter.AddCondition(cndUser2);

            topFilter.AddFilter(userFilter);
            topFilter.AddFilter(teamFilter);

            qe.Criteria = topFilter;
            qe.LinkEntities.Add(teammemberLink);
            qe.LinkEntities.Add(userPrincipalobjectaccessLink);

            return qe;
        }
    }
}
