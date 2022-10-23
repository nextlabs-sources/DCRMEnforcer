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
    //class IsOwnerCondition : Condition
    //{
    //    private bool m_isOwner;
    //    private Relation m_ownerIsUserRelation;
    //    private Relation m_ownerIsTeamRelation;

    //    private List<LinkEntity> m_queryLinkEntities;
    //    private List<FetchLinkEntityType> m_fetchLinkEntities;

    //    private Guid m_userid;
    //    private string m_entityName;

    //    private List<ConditionExpression> m_negativeQueryUserConditions = new List<ConditionExpression>();
    //    private List<ConditionExpression> m_negativeQueryTeamConditions = new List<ConditionExpression>();
    //    private List<condition> m_negativeFetchUserConditions = new List<condition>();
    //    private List<condition> m_negativeFetchTeamConditions = new List<condition>();

    //    private ConditionExpression m_positiveQueryUserCondition;
    //    private condition m_positiveFetchUserCondition;

    //    private FilterExpression m_negativeQueryFilter;
    //    private filter m_negativeFetchFilter;

    //    private const string m_isOwnerAttribute = "nxl_isowner";

    //    public delegate void RecordQueryConditionDelegate(ConditionExpression userCondition, ConditionExpression teamCondition);
    //    public delegate void RecordFetchConditionDelegate(condition userCondition, condition teamCondition);
    //    public delegate void RecordRelationDelegate(Relation userRelation, Relation teamRelation);

    //    private void RecordQueryCondition(ConditionExpression userCondition, ConditionExpression teamCondition)
    //    {
    //        if (userCondition != null)
    //        {
    //            m_negativeQueryUserConditions.Add(userCondition);
    //        }

    //        if (teamCondition != null)
    //        {
    //            m_negativeQueryTeamConditions.Add(teamCondition);
    //        }
    //    }

    //    private void RecordFetchCondition(condition userCondition, condition teamCondition)
    //    {
    //        if (userCondition != null)
    //        {
    //            m_negativeFetchUserConditions.Add(userCondition);
    //        }

    //        if (teamCondition != null)
    //        {
    //            m_negativeFetchTeamConditions.Add(teamCondition);
    //        }
    //    }

    //    private void RecordRelation(Relation userRelation, Relation teamRelation)
    //    {
    //        if (userRelation != null)
    //        {
    //            m_ownerIsUserRelation = userRelation;
    //        }

    //        if (teamRelation != null)
    //        {
    //            m_ownerIsTeamRelation = teamRelation;
    //        }
    //    }

    //    public IsOwnerCondition(bool isOwnerTrue,string entityName, Guid? userid = null)
    //    {
    //        this.m_isOwner = isOwnerTrue;
    //        if(m_isOwner)
    //        {
    //            m_positiveQueryUserCondition = CreatePositiveQueryCondition();
    //            m_positiveFetchUserCondition = CreatePositiveFetchCondition();
    //        }
    //        else
    //        {
    //            if(string.IsNullOrEmpty(entityName))
    //            {
    //                throw new ArgumentNullException("entity name cannot be null when the isowner condition is a negative condition.");
    //            }
    //            if(userid == null)
    //            {
    //                throw new ArgumentNullException("user id cannot be null when the isowner condition is a negative condition.");
    //            }
    //            m_userid = userid.Value;
    //            m_entityName = entityName;

    //            CreateNegativeEntityItem(
    //                entityName,
    //                m_userid,
    //                RecordQueryCondition,
    //                RecordRelation,
    //                out m_queryLinkEntities,
    //                out m_negativeQueryFilter);

    //            CreateNegativeEntityItem(
    //                entityName,
    //                m_userid,
    //                RecordFetchCondition,
    //                RecordRelation,
    //                out m_fetchLinkEntities,
    //                out m_negativeFetchFilter);
    //        }
    //    }

    //    public override List<Relation> GetRelations()
    //    {
    //        if (m_isOwner)
    //        {
    //            return null;
    //        }

    //        List<Relation> relations = new List<Relation>();
    //        relations.Add(m_ownerIsUserRelation);
    //        relations.Add(m_ownerIsTeamRelation);

    //        return relations;
    //    }


    //    public override void UpdateAlias(string alias)
    //    {
    //        if (!m_isOwner)
    //        {
    //            UpdateQueryConditionAlias();
    //            UpdateFetchConditionAlias();
    //        }
    //        else
    //        {
    //            m_positiveQueryUserCondition.EntityName = alias;
    //            m_positiveFetchUserCondition.entityname = alias;
    //        }
    //    }


    //    private void UpdateFetchConditionAlias()
    //    {
    //        foreach (condition cond in m_negativeFetchTeamConditions)
    //        {
    //            cond.entityname = m_ownerIsTeamRelation.ParentEntityAlias;
    //        }

    //        foreach (condition cond in m_negativeFetchUserConditions)
    //        {
    //            cond.entityname = m_ownerIsUserRelation.ParentEntityAlias;
    //        }
    //    }

    //    private void UpdateQueryConditionAlias()
    //    {
    //        foreach (ConditionExpression cond in m_negativeQueryTeamConditions)
    //        {
    //            cond.EntityName = m_ownerIsTeamRelation.ParentEntityAlias;
    //        }

    //        foreach (ConditionExpression cond in m_negativeQueryUserConditions)
    //        {
    //            cond.EntityName = m_ownerIsUserRelation.ParentEntityAlias;
    //        }
    //    }

    //    public override FilterExpression GetQueryFilter()
    //    {
    //        return m_isOwner ? null : m_negativeQueryFilter;
    //    }

    //    public override filter GetFetchFilter()
    //    {
    //        return m_isOwner ? null : m_negativeFetchFilter;
    //    }
    //    public override ConditionExpression GetQueryCondition()
    //    {
    //        return m_positiveQueryUserCondition;
    //    }

    //    public override condition GetFetchCondition()
    //    {
    //        return m_positiveFetchUserCondition;
    //    }

    //    private ConditionExpression CreatePositiveQueryCondition()
    //    {
    //        ConditionExpression cond = new ConditionExpression();
    //        cond.AttributeName = "ownerid";
    //        cond.Operator = ConditionOperator.EqualUserOrUserTeams;
    //        return cond;
    //    }

    //    private condition CreatePositiveFetchCondition()
    //    {
    //        condition cond = new condition();
    //        cond.attribute = "ownerid";
    //        cond.@operator = @operator.equseroruserteams;
    //        return cond;
    //    }


        

    //    private ConditionExpression GetRecordEqualQueryCondition(string recordIdAttribute,Guid recordId)
    //    {
    //        ConditionExpression cond = new ConditionExpression();
    //        cond.AttributeName = recordIdAttribute;
    //        cond.Operator = ConditionOperator.Equal;
    //        cond.Values.Add(recordId);
    //        return cond;
    //    }

    //    private condition GetRecordEqualFetchCondition(string recordIdAttribute, Guid recordId)
    //    {
    //        condition cond = new condition();
    //        cond.attribute = recordIdAttribute;
    //        cond.@operator = @operator.eq;
    //        cond.value = recordId.ToString();

    //        return cond;
    //    }

        

    //    static private void CreateRelations(string entityname,
    //        out Relation ownerIsUserRelation, out Relation ownerIsTeamRelation)
    //    {
    //        const string userRelationshipNamePrefix = "isowneruser_";
    //        const string teamRelationshipNamePrefix = "isownerteam_";

    //        /*  select * from entity
    //           leftjoin systemuser as SU on SU.systemuserid = entity.ownerid
    //           leftjoin teammembership as TS on TS.teamid = entity.ownerid
    //           where (SU.systemuserid is null and TS.systemuserid is null) or
    //           (SU.systemuserid <> userid and TS.systemuserid <> userid)*/

    //        ownerIsUserRelation = new N1Relation(
    //            userRelationshipNamePrefix+entityname + "teammembership",
    //            entityname,
    //            "systemuser",
    //            "ownerid",
    //            "systemuserid"
    //            );

    //        ownerIsTeamRelation  = new N1Relation(
    //            teamRelationshipNamePrefix+entityname + "teammembership",
    //            entityname,
    //            "teammembership",
    //            "ownerid",
    //            "teamid"
    //            );
    //    }

    //    static private FilterExpression CreateNegativeQueryFilter(Guid userid,
    //        string userEntityAlias,
    //        string teamEntityAlias,
    //        RecordQueryConditionDelegate conditionRecorder)
    //    {
    //        FilterExpression notEqualFilter = new FilterExpression(LogicalOperator.And);
    //        ConditionExpression notUserIdCond = new ConditionExpression();
    //        notUserIdCond.EntityName = userEntityAlias;
    //        notUserIdCond.AttributeName = "systemuserid";
    //        notUserIdCond.Operator = ConditionOperator.NotEqual;
    //        notUserIdCond.Values.Add(userid);

    //        ConditionExpression notTeamUserIdCond = new ConditionExpression();
    //        notTeamUserIdCond.EntityName = teamEntityAlias;
    //        notTeamUserIdCond.AttributeName = "systemuserid";
    //        notTeamUserIdCond.Operator = ConditionOperator.NotEqual;
    //        notTeamUserIdCond.Values.Add(userid);

    //        notEqualFilter.AddCondition(notUserIdCond);
    //        notEqualFilter.AddCondition(notTeamUserIdCond);

    //        FilterExpression nullFilter = new FilterExpression(LogicalOperator.And);
    //        ConditionExpression nullUserIdCond = new ConditionExpression();
    //        nullUserIdCond.EntityName = userEntityAlias;
    //        nullUserIdCond.AttributeName = "systemuserid";
    //        nullUserIdCond.Operator = ConditionOperator.Null;

    //        ConditionExpression nullTeamUserIdCond = new ConditionExpression();
    //        nullTeamUserIdCond.EntityName = teamEntityAlias;
    //        nullTeamUserIdCond.AttributeName = "systemuserid";
    //        nullTeamUserIdCond.Operator = ConditionOperator.Null;

    //        nullFilter.AddCondition(nullUserIdCond);
    //        nullFilter.AddCondition(nullTeamUserIdCond);

    //        FilterExpression topFilter = new FilterExpression(LogicalOperator.Or);
    //        topFilter.AddFilter(notEqualFilter);
    //        topFilter.AddFilter(nullFilter);

    //        if (conditionRecorder != null)
    //        {
    //            conditionRecorder(notUserIdCond, notTeamUserIdCond);
    //            conditionRecorder(nullUserIdCond, nullTeamUserIdCond);
    //        }

    //        return topFilter;
    //    }

    //    static private filter CreateNegativeFetchFilter(Guid userid, string userEntityAlias, string teamEntityAlias,
    //        RecordFetchConditionDelegate conditionRecorder)
    //    {
    //        filter notEqualFilter = new filter();
    //        notEqualFilter.type = filterType.and;
    //        notEqualFilter.Items = new List<object>();

    //        condition notUserIdCond = new condition();
    //        notUserIdCond.entityname = userEntityAlias;
    //        notUserIdCond.attribute = "systemuserid";
    //        notUserIdCond.@operator = @operator.neq;
    //        notUserIdCond.value = userid.ToString();

    //        condition notTeamUserIdCond = new condition();
    //        notTeamUserIdCond.entityname = teamEntityAlias;
    //        notTeamUserIdCond.attribute = "systemuserid";
    //        notTeamUserIdCond.@operator = @operator.neq;
    //        notTeamUserIdCond.value = userid.ToString();

    //        notEqualFilter.Items.Add(notUserIdCond);
    //        notEqualFilter.Items.Add(notTeamUserIdCond);



    //        filter nullFilter = new filter();
    //        nullFilter.type = filterType.and;
    //        nullFilter.Items = new List<object>();

    //        condition nullUserIdCond = new condition();
    //        nullUserIdCond.entityname = userEntityAlias;
    //        nullUserIdCond.attribute = "systemuserid";
    //        nullUserIdCond.@operator = @operator.@null;

    //        condition nullTeamUserIdCond = new condition();
    //        nullTeamUserIdCond.entityname = teamEntityAlias;
    //        nullTeamUserIdCond.attribute = "systemuserid";
    //        nullTeamUserIdCond.@operator = @operator.@null;

    //        nullFilter.Items.Add(nullUserIdCond);
    //        nullFilter.Items.Add(nullTeamUserIdCond);

    //        filter topFilter = new filter();
    //        topFilter.type = filterType.or;
    //        topFilter.Items = new List<object>();
    //        topFilter.Items.Add(notEqualFilter);
    //        topFilter.Items.Add(nullFilter);

    //        if (conditionRecorder != null)
    //        {
    //            conditionRecorder(notUserIdCond, notTeamUserIdCond);
    //            conditionRecorder(nullUserIdCond, nullTeamUserIdCond);
    //        }
    //        return topFilter;
    //    }

    //    static public void CreateNegativeEntityItem(string entityName, Guid userid,
    //        RecordQueryConditionDelegate conditionRecorder,
    //        RecordRelationDelegate relationRecorder,
    //        out List<LinkEntity> linkentities,
    //        out FilterExpression negativeObligationFilter)
    //    {
    //        Relation sharedToUserRelation = null;
    //        Relation sharedToTeamRelation = null;
    //        CreateRelations(entityName, out sharedToUserRelation, out sharedToTeamRelation);

    //        EntityOutJoin outJoin = new EntityOutJoin();
    //        outJoin.AddRelation(sharedToUserRelation);
    //        outJoin.AddRelation(sharedToTeamRelation);
    //        linkentities = outJoin.ToQueryLinkEntities();

    //        if (relationRecorder != null)
    //        {
    //            relationRecorder(sharedToUserRelation, sharedToTeamRelation);
    //        }

    //        negativeObligationFilter = CreateNegativeQueryFilter(userid,
    //                                                                                    sharedToUserRelation.ParentEntityAlias,
    //                                                                                    sharedToTeamRelation.ParentEntityAlias,
    //                                                                                    conditionRecorder);

    //    }


    //    static public void CreateNegativeEntityItem(string entityName, Guid userid,
    //        RecordFetchConditionDelegate conditionRecorder,
    //        RecordRelationDelegate relationRecorder,
    //        out List<FetchLinkEntityType> linkentities,
    //        out filter negativeObligationFilter)
    //    {
    //        linkentities = new List<FetchLinkEntityType>();

    //        Relation sharedToUserRelation = null;
    //        Relation sharedToTeamRelation = null;
    //        CreateRelations(entityName,  out sharedToUserRelation, out sharedToTeamRelation);

    //        EntityOutJoin outJoin = new EntityOutJoin();
    //        outJoin.AddRelation(sharedToUserRelation);
    //        outJoin.AddRelation(sharedToTeamRelation);
    //        linkentities = outJoin.ToFetchLinkEntities();

    //        if (relationRecorder != null)
    //        {
    //            relationRecorder(sharedToUserRelation, sharedToTeamRelation);
    //        }

    //        negativeObligationFilter = CreateNegativeFetchFilter(userid,
    //                                                                                    sharedToUserRelation.ParentEntityAlias,
    //                                                                                    sharedToTeamRelation.ParentEntityAlias,
    //                                                                                    conditionRecorder);
    //    }


    //    public QueryExpression CreateQueryExpressionByRecord(string entityName, string recordIdAttribute, Guid recordId)
    //    {
    //        QueryExpression qe = new QueryExpression();
    //        qe.EntityName = entityName;
    //        qe.ColumnSet = new ColumnSet("ownerid");
    //        qe.Criteria.FilterOperator = LogicalOperator.And;
    //        qe.Criteria.AddCondition(GetQueryCondition());
    //        qe.Criteria.AddCondition(GetRecordEqualQueryCondition(recordIdAttribute, recordId));
    //        return qe;
    //    }
    //    public FetchExpression CreateFetchExpressionByRecord(string entityName, string recordIdAttribute, Guid recordId)
    //    {
    //        FetchType fetch = new FetchType();
    //        fetch.Items = new List<object>();

    //        FetchEntityType entity = new FetchEntityType();
    //        entity.Items = new List<object>();
    //        fetch.Items.Add(entity);

    //        FetchAttributeType owneridAttr = new FetchAttributeType();
    //        owneridAttr.name = "ownerid";
    //        entity.Items.Add(owneridAttr);

    //        filter criteria = new filter();
    //        criteria.type = filterType.and;
    //        criteria.Items = new List<object>();
    //        criteria.Items.Add(GetFetchCondition());
    //        criteria.Items.Add(GetRecordEqualFetchCondition(recordIdAttribute, recordId));

    //        entity.Items.Add(criteria);

    //        IStringSerialize serializer = new XMLSerializeHelper();
    //        string fetchXML = serializer.SerializeWithoutNamespace(typeof(FetchType), fetch);

    //        FetchExpression fe = new FetchExpression(fetchXML);

    //        return fe;
    //    }
    //}
}
