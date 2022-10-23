using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    public class ApplySecurityFilter
    {
        private string m_entityName;

        private FilterExpression m_queryFilter = new FilterExpression(LogicalOperator.And);
        private filter m_fetchFilter = new filter();

        private List<Condition.Condition> m_conditions = new List<Condition.Condition>();
        private Dictionary<string, Relation> m_relationMap = new Dictionary<string, Relation>();

        public ApplySecurityFilter()
        {
            m_fetchFilter.type = filterType.and;
            m_fetchFilter.Items = new List<object>();
        }

        private ApplySecurityFilter(string entityName, FilterExpression queryFilter,filter fetchFilter, 
            List<Condition.Condition> conditions)
        {
            m_queryFilter = queryFilter;
            m_fetchFilter = fetchFilter;
            m_entityName = entityName;
            m_conditions = conditions;

            foreach (Condition.Condition cond in m_conditions)
            {
                AppendRelationByCondition(cond);
             }
        }

        public string EntityName
        {
            get { return m_entityName; }
            set { m_entityName = value; }
        }

        public string EntityAlias
        {
            get;set;
        }
        public FilterExpression QueryFilter
        {
            get { return m_queryFilter; }
        }

        public filter FetchFilter
        {
            get { return m_fetchFilter; }
        }

        public List<FetchLinkEntityType> FetchLinkEntites
        {
            get
            {
                EntityOutJoin join = new EntityOutJoin();
                foreach(Relation relation in m_relationMap.Values)
                {
                    join.AddRelation(relation);
                }

                List<FetchLinkEntityType> linkEntities =  join.ToFetchLinkEntities();

                foreach (Condition.Condition cond in m_conditions)
                {
                    List<Relation> relations = cond.GetRelations();
                    if (relations == null || relations.Count == 0)
                    {
                        continue;
                    }
                    cond.UpdateAlias(null);
                }
                return linkEntities;
            }
        }

        public List<LinkEntity> QueryLinkEntites
        {
            get
            {
                EntityOutJoin join = new EntityOutJoin();
                foreach (Relation relation in m_relationMap.Values)
                {
                    join.AddRelation(relation);
                }

                List<LinkEntity> linkEntities =  join.ToQueryLinkEntities();

                foreach(Condition.Condition cond in m_conditions)
                {
                    List<Relation> relations = cond.GetRelations();
                    if(relations == null || relations.Count == 0)
                    {
                        continue;
                    }

                    cond.UpdateAlias(null);
                }
                return linkEntities;
            }
        }

        private void AppendRelationByCondition(Condition.Condition cond)
        {
            List<Relation> relations = cond.GetRelations();
            if (relations == null)
            {
                return;
            }
            foreach (Relation relation in relations)
            {
                if (m_relationMap.ContainsKey(relation.Name))
                {
                    Relation existedRelation = m_relationMap[relation.Name];
                    cond.UpdateRelation(existedRelation);
                }
                else
                {
                    m_relationMap[relation.Name] = relation;
                }
            }
        }
        public void AddCondition(Condition.Condition obligationCondition)
        {
            if (obligationCondition == null)
            {
                return;
            }
            
            if(obligationCondition.GetQueryFilter() != null)
            {
                    //positive is owner condition
                m_queryFilter.AddFilter(obligationCondition.GetQueryFilter());
                m_fetchFilter.Items.Add(obligationCondition.GetFetchFilter());
            }
            else if(obligationCondition.GetQueryCondition() != null)
            {      //common condition
                m_queryFilter.AddCondition(obligationCondition.GetQueryCondition());
                m_fetchFilter.Items.Add(obligationCondition.GetFetchCondition());
            }
            else //lazy initial conditions
            {
                obligationCondition.AttachParent(m_queryFilter);
                obligationCondition.AttachParent(m_fetchFilter);
            }
            /*else
            {
                throw new Exception("condition must contain filter for virtual condition or condition for common condition.");
            }*/

            m_conditions.Add(obligationCondition);
            AppendRelationByCondition(obligationCondition);
        }

        public List<Condition.Condition> Conditions
        { get { return m_conditions; } }

        public List<Relation> Relations
        {
            get { return m_relationMap.Values.ToList<Relation>(); }
        }
        public ApplySecurityFilter And(ApplySecurityFilter source)
        {
            if(source == null)
            {
                return this;
            }

            FilterExpression topQueryFilter = new FilterExpression(LogicalOperator.And);
            topQueryFilter.AddFilter(m_queryFilter);
            topQueryFilter.AddFilter(source.m_queryFilter);

            filter topFetchFilter = new filter();
            topFetchFilter.type = filterType.and;
            topFetchFilter.Items = new List<object>();

            if(m_fetchFilter != null)
            {
                topFetchFilter.Items.Add(m_fetchFilter);
            }

            if (source.m_fetchFilter != null)
            {
                topFetchFilter.Items.Add(source.m_fetchFilter);
            }

            List<Condition.Condition> topCondtions = new List<Condition.Condition>();
            topCondtions.AddRange(this.m_conditions);
            topCondtions.AddRange(source.m_conditions);

     
            return new ApplySecurityFilter(this.m_entityName,topQueryFilter, topFetchFilter, topCondtions);
        }

        public ApplySecurityFilter Or(ApplySecurityFilter source)
        {
            if (source == null)
            {
                return this;
            }

            FilterExpression topQueryFilter = new FilterExpression(LogicalOperator.Or);
            topQueryFilter.AddFilter(m_queryFilter);
            topQueryFilter.AddFilter(source.m_queryFilter);

            filter topFetchFilter = new filter();
            topFetchFilter.type = filterType.or;
            topFetchFilter.Items = new List<object>();

            if(m_fetchFilter != null)
            {
                topFetchFilter.Items.Add(m_fetchFilter);
            }

            if (source.m_fetchFilter != null)
            {
                topFetchFilter.Items.Add(source.m_fetchFilter);
            }

            List<Condition.Condition> topCondtions = new List<Condition.Condition>();
            topCondtions.AddRange(this.m_conditions);
            topCondtions.AddRange(source.m_conditions);

            return new ApplySecurityFilter(this.m_entityName, topQueryFilter, topFetchFilter, topCondtions);
        }
    }
}
