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

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    //[Obsolete]
    //class IsSharedCondition: Condition
    //{
    //    private const string m_relationshipEntity = "principalobjectaccess";
    //    private const string m_sharerAttrName = "principalid";
    //    private const string m_recordAttrName = "objectid";
    //    private const string m_isSharedAttribute = "nxl_isshared";

    //    private Guid m_userid;
    //    private string m_entityName;
    //    private string m_entityPrimaryId;

    //    private Relation m_sharedToUserRelation;
    //    private Relation m_sharedToTeamRelation;

    //    private List<ConditionExpression> m_queryUserConditions = new List<ConditionExpression>();
    //    private List<ConditionExpression> m_queryTeamConditions = new List<ConditionExpression>();
    //    private List<condition> m_fetchUserConditions = new List<condition>();
    //    private List<condition> m_fetchTeamConditions = new List<condition>();

    //    private FilterExpression m_positiveQueryFilter;
    //    private FilterExpression m_negativeQueryFilter;
    //    private filter m_positiveFetchFilter;
    //    private filter m_negativeFetchFilter;

    //    private List<LinkEntity> m_queryLinkEntities;
    //    private List<FetchLinkEntityType> m_fetchLinkEntities;

    //    private bool m_isSharedTrue;

    //    public delegate void RecordQueryConditionDelegate(ConditionExpression userCondition, ConditionExpression teamCondition);
    //    public delegate void RecordFetchConditionDelegate(condition userCondition, condition teamCondition);
    //    public delegate void RecordRelationDelegate(Relation userRelation, Relation teamRelation);

    //    private void RecordQueryCondition(ConditionExpression userCondition, ConditionExpression teamCondition)
    //    {
    //        if(userCondition != null)
    //        {
    //            m_queryUserConditions.Add(userCondition);
    //        }

    //        if(teamCondition != null)
    //        {
    //            m_queryTeamConditions.Add(teamCondition);
    //        }
    //    }

    //    private void RecordFetchCondition(condition userCondition, condition teamCondition)
    //    {
    //        if (userCondition != null)
    //        {
    //            m_fetchUserConditions.Add(userCondition);
    //        }

    //        if (teamCondition != null)
    //        {
    //            m_fetchTeamConditions.Add(teamCondition);
    //        }
    //    }

    //    private void RecordRelation(Relation userRelation, Relation teamRelation)
    //    {
    //        if(userRelation != null)
    //        {
    //            m_sharedToUserRelation = userRelation;
    //        }

    //        if(teamRelation != null)
    //        {
    //            m_sharedToTeamRelation = teamRelation;
    //        }
    //    }

    //    public static bool IsIsSharedEnabled(SecureEntity secureEntity)
    //    {
            
    //        if (!secureEntity.Secured || secureEntity.Schema == null)
    //        {
    //            return false;
    //        }

    //        if(secureEntity.Schema.Attributes == null || secureEntity.Schema.Attributes.Count == 0)
    //        {
    //            return false;
    //        }

    //        DataModel.MetaData.Attribute isSharedAttr = 
    //        secureEntity.Schema.Attributes.Find((DataModel.MetaData.Attribute attr) 
    //        => attr.LogicName.Equals(m_isSharedAttribute, StringComparison.OrdinalIgnoreCase));

    //        if(isSharedAttr == null)
    //        {
    //            return false;
    //        }
    //        return true;
    //    }

    //    public static bool IsIsSharedEnabled(string entityName, MemoryCache<SecureEntity> secureEntityCache)
    //    {
    //        SecureEntity entity = secureEntityCache.Lookup(entityName, x => null);
    //        return IsIsSharedEnabled(entity);
    //    }

    //    static private void CreateRelations(string entityname, string entityPrimaryIdName,
    //        out Relation sharedToUserRelation, out Relation sharedToTeamRelation)
    //    {
    //        /*
    //         * select * from entity
    //            leftjoin principalobjectaccess as POA1 on POA1.objectid = entity.entityid
    //            leftjoin systemuser as SU1 on SU1.systemuserid = POA1.principalid
    //            leftjoin principalobjectaccess as POA2 on POA2.objectid = entity.entityid
    //            leftjoin teammembership as TS on TS.teamid = POA2.principalid
    //            where (SU1.systemuserid = userid or TS.systemuserid = userid)
    //         */
    //        const string userRelationshipNamePrefix = "isshareduser_";
    //        const string teamRelationshipNamePrefix = "issharedteam_";
    //        sharedToUserRelation = new NNRelation(
    //            userRelationshipNamePrefix + entityname + m_relationshipEntity,
    //            entityname,
    //            "systemuser",
    //            entityPrimaryIdName,
    //            "systemuserid",
    //            m_relationshipEntity,
    //            m_recordAttrName,
    //            m_sharerAttrName
    //            );

    //        sharedToTeamRelation = new NNRelation(
    //            teamRelationshipNamePrefix + entityname + m_relationshipEntity,
    //            entityname,
    //            "teammembership",
    //            entityPrimaryIdName,
    //            "teamid",
    //            m_relationshipEntity,
    //            m_recordAttrName,
    //            m_sharerAttrName
    //            );
    //    }

    //    static private FilterExpression CreatePositiveQueryFilter(Guid userid,
    //        string userEntityAlias,
    //        string teamEntityAlias,
    //        RecordQueryConditionDelegate conditionRecorder)
    //    {
    //        /*
    //        * select * from entity
    //           leftjoin principalobjectaccess as POA1 on POA1.objectid = entity.entityid
    //           leftjoin systemuser as SU1 on SU1.systemuserid = POA1.principalid
    //           leftjoin principalobjectaccess as POA2 on POA2.objectid = entity.entityid
    //           leftjoin teammembership as TS on TS.teamid = POA2.principalid
    //           where (SU1.systemuserid = userid or TS.systemuserid = userid)
    //        */
    //        FilterExpression obligationFilter = new FilterExpression(LogicalOperator.Or);
    //        ConditionExpression userIsSharerCond = new ConditionExpression();
    //        userIsSharerCond.EntityName = userEntityAlias;
    //        userIsSharerCond.AttributeName = "systemuserid";
    //        userIsSharerCond.Operator = ConditionOperator.Equal;
    //        userIsSharerCond.Values.Add(userid);

    //        ConditionExpression userTeamIsSharerCond = new ConditionExpression();
    //        userTeamIsSharerCond.EntityName = teamEntityAlias;
    //        userTeamIsSharerCond.AttributeName = "systemuserid";
    //        userTeamIsSharerCond.Operator = ConditionOperator.Equal;
    //        userTeamIsSharerCond.Values.Add(userid);

    //        obligationFilter.AddCondition(userIsSharerCond);
    //        obligationFilter.AddCondition(userTeamIsSharerCond);
    //        if(conditionRecorder != null)
    //        {
    //            conditionRecorder(userIsSharerCond, userTeamIsSharerCond);
    //        }
    //        return obligationFilter;
    //    }

    //    static private FilterExpression CreateNegativeQueryFilter(Guid userid, 
    //        string userEntityAlias, 
    //        string teamEntityAlias,
    //        RecordQueryConditionDelegate conditionRecorder)
    //    {
    //        /*
    //         * select * from entity
    //            leftjoin principalobjectaccess as POA1 on POA1.objectid = entity.entityid
    //            leftjoin systemuser as SU1 on SU1.systemuserid = POA1.principalid
    //            leftjoin principalobjectaccess as POA2 on POA2.objectid = entity.entityid
    //            leftjoin teammembership as TS on TS.teamid = POA2.principalid
    //            where (SU1.systemuserid <> userid and TS.systemuserid <> userid) or
    //            (SU1.systemuserid is null and TS.systemuserid is null)
    //         */

    //        //is shared condition
    //        FilterExpression notSharedObligationFilter = new FilterExpression(LogicalOperator.And);
    //        ConditionExpression userIsNotSharerCond = new ConditionExpression();
    //        userIsNotSharerCond.EntityName = userEntityAlias;
    //        userIsNotSharerCond.AttributeName = "systemuserid";
    //        userIsNotSharerCond.Operator = ConditionOperator.NotEqual;
    //        userIsNotSharerCond.Values.Add(userid);

    //        ConditionExpression userTeamIsNotSharerCond = new ConditionExpression();
    //        userTeamIsNotSharerCond.EntityName = teamEntityAlias;
    //        userTeamIsNotSharerCond.AttributeName = "systemuserid";
    //        userTeamIsNotSharerCond.Operator = ConditionOperator.Equal;
    //        userTeamIsNotSharerCond.Values.Add(userid);

    //        notSharedObligationFilter.AddCondition(userIsNotSharerCond);
    //        notSharedObligationFilter.AddCondition(userTeamIsNotSharerCond);

    //        // is null condition
    //        FilterExpression nullUserFilter = new FilterExpression(LogicalOperator.And);
    //        ConditionExpression userNullCond = new ConditionExpression();
    //        userNullCond.EntityName = userEntityAlias;
    //        userNullCond.AttributeName = "systemuserid";
    //        userNullCond.Operator = ConditionOperator.Null;

    //        ConditionExpression teamuserNullCond = new ConditionExpression();
    //        teamuserNullCond.EntityName = teamEntityAlias;
    //        teamuserNullCond.AttributeName = "systemuserid";
    //        teamuserNullCond.Operator = ConditionOperator.Null;

    //        nullUserFilter.AddCondition(userNullCond);
    //        nullUserFilter.AddCondition(teamuserNullCond);

    //        FilterExpression totalNotFilter = new FilterExpression(LogicalOperator.Or);
    //        totalNotFilter.AddFilter(notSharedObligationFilter);
    //        totalNotFilter.AddFilter(nullUserFilter);

    //        if (conditionRecorder != null)
    //        {
    //            conditionRecorder(userIsNotSharerCond, userTeamIsNotSharerCond);
    //            conditionRecorder(userNullCond, teamuserNullCond);
    //        }

    //        return totalNotFilter;
    //    }

    //    static private filter CreatePositiveFetchFilter(Guid userid, string userEntityAlias, string teamEntityAlias,
    //        RecordFetchConditionDelegate conditionRecorder)
    //    {
    //        filter isSharedObligationFilter = new filter();
    //        isSharedObligationFilter.Items = new List<object>();
    //        isSharedObligationFilter.type = filterType.or;
    //        condition userIsSharerCond = new condition();
    //        userIsSharerCond.entityname = userEntityAlias;
    //        userIsSharerCond.attribute = "systemuserid";
    //        userIsSharerCond.@operator = @operator.eq;
    //        userIsSharerCond.value = userid.ToString();

    //        condition userTeamIsSharerCond = new condition();
    //        userTeamIsSharerCond.entityname = teamEntityAlias;
    //        userTeamIsSharerCond.attribute = "systemuserid";
    //        userTeamIsSharerCond.@operator = @operator.eq;
    //        userTeamIsSharerCond.value = userid.ToString();

    //        isSharedObligationFilter.Items.Add(userIsSharerCond);
    //        isSharedObligationFilter.Items.Add(userTeamIsSharerCond);

    //        if(conditionRecorder != null)
    //        {
    //            conditionRecorder(userIsSharerCond, userTeamIsSharerCond);
    //        }
    //        return isSharedObligationFilter;
    //    }

    //    static private filter CreateNegativeFetchFilter(Guid userid, string userEntityAlias, string teamEntityAlias,
    //        RecordFetchConditionDelegate conditionRecorder)
    //    {
    //        filter notSharedObligationFilter = new filter();
    //        notSharedObligationFilter.Items = new List<object>();
    //        notSharedObligationFilter.type = filterType.and;
    //        condition userIsNotSharerCond = new condition();
    //        userIsNotSharerCond.entityname = userEntityAlias;
    //        userIsNotSharerCond.attribute = "systemuserid";
    //        userIsNotSharerCond.@operator = @operator.neq;
    //        userIsNotSharerCond.value = userid.ToString();

    //        condition userTeamIsNotSharerCond = new condition();
    //        userTeamIsNotSharerCond.entityname = teamEntityAlias;
    //        userTeamIsNotSharerCond.attribute = "systemuserid";
    //        userTeamIsNotSharerCond.@operator = @operator.neq;
    //        userTeamIsNotSharerCond.value = userid.ToString();

    //        notSharedObligationFilter.Items.Add(userIsNotSharerCond);
    //        notSharedObligationFilter.Items.Add(userTeamIsNotSharerCond);


    //        filter nullObligationFilter = new filter();
    //        nullObligationFilter.Items = new List<object>();
    //        nullObligationFilter.type = filterType.and;
    //        condition userIsNullSharerCond = new condition();
    //        userIsNullSharerCond.entityname = userEntityAlias;
    //        userIsNullSharerCond.attribute = "systemuserid";
    //        userIsNullSharerCond.@operator = @operator.@null;

    //        condition userTeamIsNullCond = new condition();
    //        userTeamIsNullCond.entityname = teamEntityAlias;
    //        userTeamIsNullCond.attribute = "systemuserid";
    //        userTeamIsNullCond.@operator = @operator.@null;

    //        nullObligationFilter.Items.Add(userIsNullSharerCond);
    //        nullObligationFilter.Items.Add(userTeamIsNullCond);

    //        filter totalNotFilter = new filter();
    //        totalNotFilter.Items = new List<object>();
    //        totalNotFilter.type = filterType.or;
    //        totalNotFilter.Items.Add(notSharedObligationFilter);
    //        totalNotFilter.Items.Add(nullObligationFilter);

    //        if (conditionRecorder != null)
    //        {
    //            conditionRecorder(userIsNotSharerCond, userTeamIsNotSharerCond);
    //            conditionRecorder(userIsNullSharerCond, userTeamIsNullCond);
    //        }
    //        return totalNotFilter;
    //    }
    //    static public void CreateEntityItem(string entityName, string entityPrimaryIdName, Guid userid,
    //        RecordQueryConditionDelegate conditionRecorder,
    //        RecordRelationDelegate relationRecorder,
    //         out List<LinkEntity> linkentities,
    //        out FilterExpression positiveObligationFilter,out FilterExpression negativeObligationFilter)
    //    {
    //        Relation sharedToUserRelation = null;
    //        Relation sharedToTeamRelation = null;
    //        CreateRelations(entityName, entityPrimaryIdName, out sharedToUserRelation, out sharedToTeamRelation);

    //        EntityOutJoin outJoin = new EntityOutJoin();
    //        outJoin.AddRelation(sharedToUserRelation);
    //        outJoin.AddRelation(sharedToTeamRelation);
    //        linkentities = outJoin.ToQueryLinkEntities();

    //        if(relationRecorder != null)
    //        {
    //            relationRecorder(sharedToUserRelation, sharedToTeamRelation);
    //        }

    //        positiveObligationFilter = CreatePositiveQueryFilter(  userid,
    //                                                                                    sharedToUserRelation.ParentEntityAlias,
    //                                                                                    sharedToTeamRelation.ParentEntityAlias,
    //                                                                                    conditionRecorder);

    //        negativeObligationFilter = CreateNegativeQueryFilter(userid,
    //                                                                                    sharedToUserRelation.ParentEntityAlias,
    //                                                                                    sharedToTeamRelation.ParentEntityAlias,
    //                                                                                    conditionRecorder);

    //    }


    //    static public void CreateEntityItem(string entityName, string entityPrimaryIdName, Guid userid,
    //        RecordFetchConditionDelegate conditionRecorder,
    //        RecordRelationDelegate relationRecorder,
    //        out List<FetchLinkEntityType> linkentities, 
    //        out filter positiveObligationFilter, out filter negativeObligationFilter)
    //    {
    //        linkentities = new List<FetchLinkEntityType>();

    //        Relation sharedToUserRelation = null;
    //        Relation sharedToTeamRelation = null;
    //        CreateRelations(entityName, entityPrimaryIdName,out sharedToUserRelation, out sharedToTeamRelation);

    //        EntityOutJoin outJoin = new EntityOutJoin();
    //        outJoin.AddRelation(sharedToUserRelation);
    //        outJoin.AddRelation(sharedToTeamRelation);
    //        linkentities = outJoin.ToFetchLinkEntities();

    //        if(relationRecorder != null)
    //        {
    //            relationRecorder(sharedToUserRelation, sharedToTeamRelation);
    //        }
    //        positiveObligationFilter = CreatePositiveFetchFilter(  userid,
    //                                                                                    sharedToUserRelation.ParentEntityAlias,
    //                                                                                    sharedToTeamRelation.ParentEntityAlias,
    //                                                                                    conditionRecorder);

    //        negativeObligationFilter = CreateNegativeFetchFilter(userid,
    //                                                                                    sharedToUserRelation.ParentEntityAlias,
    //                                                                                    sharedToTeamRelation.ParentEntityAlias,
    //                                                                                    conditionRecorder);
    //    }

    //    static public QueryExpression CreateQueryExpressionByRecord(string entityName, 
    //                                                                                                string entityPrimaryIdName, 
    //                                                                                                Guid userid,
    //                                                                                                Guid recordid)
    //    {
    //        List<LinkEntity> linkentities = null;
    //        FilterExpression positiveObligationFilter = null;
    //        FilterExpression negativeObligationFilter = null;

    //        CreateEntityItem(entityName, 
    //                                entityPrimaryIdName, 
    //                                userid, 
    //                                null,
    //                                null,
    //                                out linkentities,
    //                                out positiveObligationFilter, 
    //                                out negativeObligationFilter
    //                                );

    //        FilterExpression topFilter = new FilterExpression(LogicalOperator.And);
    //        ConditionExpression recordCond = new ConditionExpression();
    //        recordCond.AttributeName = entityPrimaryIdName;
    //        recordCond.Operator = ConditionOperator.Equal;
    //        recordCond.Values.Add(recordid);
    //        topFilter.AddCondition(recordCond);
    //        topFilter.AddFilter(positiveObligationFilter);

    //        QueryExpression qe = new QueryExpression();
    //        qe.EntityName = entityName;
    //        qe.ColumnSet = new ColumnSet(entityPrimaryIdName);
    //        qe.LinkEntities.AddRange(linkentities);
    //        qe.Criteria = topFilter;

    //        return qe;
    //    }

    //    static public FetchExpression CreateFetchExpressionByRecord(string entityName,
    //                                                                                    string entityPrimaryIdName,
    //                                                                                    Guid userid,
    //                                                                                    Guid recordid)
    //    {
    //        List<FetchLinkEntityType> linkentities = null;
    //        filter positiveObligationFilter = null;
    //        filter negativeObligationFilter = null;

    //        CreateEntityItem(  entityName,
    //                                    entityPrimaryIdName,
    //                                    userid, 
    //                                    null,
    //                                    null,
    //                                    out linkentities,
    //                                    out positiveObligationFilter,
    //                                    out negativeObligationFilter);

    //        filter topFilter = new filter();
    //        topFilter.type = filterType.and;
    //        topFilter.Items = new List<object>();

    //        condition recordCond = new condition();
    //        recordCond.attribute = entityPrimaryIdName;
    //        recordCond.@operator = @operator.eq;
    //        recordCond.value = recordid.ToString();
    //        topFilter.Items.Add(recordCond);
    //        topFilter.Items.Add(positiveObligationFilter);

    //        FetchType fetch = new FetchType();
    //        fetch.Items = new List<object>();

    //        FetchEntityType entity = new FetchEntityType();
    //        entity.Items = new List<object>();
    //        entity.name = entityName;

    //        FetchAttributeType attr = new FetchAttributeType();
    //        attr.name = entityPrimaryIdName;
           
    //        entity.Items.Add(topFilter);
    //        entity.Items.Add(attr);
    //        entity.Items.AddRange(linkentities);

    //        IStringSerialize serializer = new XMLSerializeHelper();
    //        string xml = serializer.SerializeWithoutNamespace(typeof(FetchType), fetch);
    //        FetchExpression fe = new FetchExpression(xml);
    //        return fe;
    //    }

    //    public IsSharedCondition(bool isShared,string entityName, Guid userid)
    //    {
    //        if (userid == null)
    //        {
    //            throw new ArgumentNullException("user id cannot be null for the share condition initialization.");
    //        }

    //        if (string.IsNullOrWhiteSpace(entityName))
    //        {
    //            throw new ArgumentNullException("entity name cannot be null or blank for the share condition initialization.");
    //        }
    //        m_userid = userid;
    //        m_entityName = entityName;
    //        m_entityPrimaryId = entityName + "id";
    //        m_isSharedTrue = isShared;

    //        CreateEntityItem(m_entityName,
    //            m_entityPrimaryId,
    //            userid,
    //            RecordQueryCondition,
    //            RecordRelation,
    //            out m_queryLinkEntities,
    //            out m_positiveQueryFilter,
    //            out m_negativeQueryFilter);

    //        CreateEntityItem(m_entityName,
    //            m_entityPrimaryId,
    //            userid,
    //            RecordFetchCondition,
    //            RecordRelation,
    //            out m_fetchLinkEntities,
    //            out m_positiveFetchFilter,
    //            out m_negativeFetchFilter);
    //    }

    //    public override List<Relation> GetRelations()
    //    {
    //        List<Relation> relations = new List<Relation>();
    //        relations.Add(m_sharedToUserRelation);
    //        relations.Add(m_sharedToTeamRelation);

    //        return relations;
    //    }

    //    public override void UpdateAlias(string alias)
    //    {
    //        UpdateQueryConditionAlias(alias);
    //        UpdateFetchConditionAlias(alias);
    //    }

    //    private void UpdateFetchConditionAlias(string alias)
    //    {
    //        foreach (condition cond in m_fetchTeamConditions)
    //        {
    //            cond.entityname = m_sharedToTeamRelation.ParentEntityAlias;
    //        }

    //        foreach (condition cond in m_fetchUserConditions)
    //        {
    //            cond.entityname = m_sharedToUserRelation.ParentEntityAlias;
    //        }
    //    }

    //    private void UpdateQueryConditionAlias(string alias)
    //    {
    //        foreach (ConditionExpression cond in m_queryTeamConditions)
    //        {
    //            cond.EntityName = m_sharedToTeamRelation.ParentEntityAlias;
    //        }

    //        foreach (ConditionExpression cond in m_queryUserConditions)
    //        {
    //            cond.EntityName = m_sharedToUserRelation.ParentEntityAlias;
    //        }
    //    }

    //    public override FilterExpression GetQueryFilter()
    //    {
    //        return m_isSharedTrue ? m_positiveQueryFilter : m_negativeQueryFilter;
    //    }

    //    public override filter GetFetchFilter()
    //    {
    //        return m_isSharedTrue ? m_positiveFetchFilter : m_negativeFetchFilter;
    //    }

    //    public override void UpdateRelation(Relation relation)
    //    {
    //        if(relation.Name.Equals(m_sharedToUserRelation.Name))
    //        {
    //            m_sharedToUserRelation = relation;
    //        }
    //        else if(relation.Name.Equals(m_sharedToTeamRelation.Name))
    //        {
    //            m_sharedToTeamRelation = relation;
    //        }

    //        UpdateAlias(null);
    //    }
    //}
}
