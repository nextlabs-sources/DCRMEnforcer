using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    class InheritAttributeFrom:InheritFrom
    {
        private Dictionary<string,ApplySecurityFilter> m_filters = new Dictionary<string, ApplySecurityFilter>();
        private Dictionary<string, OphanedChildDecision> m_relationVSophaned = new Dictionary<string, OphanedChildDecision>();
        public void AddRelation(Relation relation)
        {
            if (relation == null)
            {
                return;
            }
            m_join.AddRelation(relation);

            OphanedChildDecision ophanedChild = relation.OrphanedAllow ? OphanedChildDecision.Allow : OphanedChildDecision.Deny;
            if (!m_relationVSophaned.ContainsKey(relation.Name))
            {
                m_relationVSophaned[relation.Name] = ophanedChild;
            }
            else
            {
                OphanedChildDecision oldOphaned = m_relationVSophaned[relation.Name];
                if (ophanedChild != OphanedChildDecision.Conflict)
                {
                    if (oldOphaned != ophanedChild)
                    {
                        m_relationVSophaned[relation.Name] = OphanedChildDecision.Conflict;
                    }
                }
            }
        }

        public void AddParentSecurityFilter(string relationshipName,ApplySecurityFilter securityFilter)
        {
            if (securityFilter == null || string.IsNullOrWhiteSpace(relationshipName))
            {
                return;
            }
            m_filters[relationshipName] = securityFilter;
        }

        public override void AddChildRelationFromVirtualCondition()
        {
            foreach (KeyValuePair<string,ApplySecurityFilter> item in m_filters)
            {
                Relation parentRelation = m_join.GetRelation(item.Key);
                if (parentRelation == null)
                {
                    continue;
                }
                foreach (Condition.Condition condition in item.Value.Conditions)
                {
                    List<Relation> relations = condition.GetRelations();
                    if (relations == null)
                    {
                        continue;
                    }

                    parentRelation.AddChildRelations(relations);
                }
            }
        }

        public void CreateEntityItems(out List<FetchLinkEntityType> linkentities, out filter obligationFilter)
        {
            AddChildRelationFromVirtualCondition();
            linkentities = m_join.ToFetchLinkEntities();
            obligationFilter = CreateTotalFetchFilter();
        }

        public override filter CreateTotalFetchFilter()
        {
            filter obligationFilter = new filter();
            obligationFilter.type = filterType.and;
            obligationFilter.Items = new List<object>();
            foreach (KeyValuePair<string,ApplySecurityFilter> item in m_filters)
            {
                Relation relation = m_join.GetRelation(item.Key);
                if(relation == null)
                {
                    continue;
                }

                AddEntityAliasToSecurityFilter(relation, item.Value);
                OphanedChildDecision ophanedChild = m_relationVSophaned[item.Key];
                filter filterWithSameRelation = CreateFetchFilterByRelation(relation, ophanedChild, item.Value);
                if (filterWithSameRelation != null)
                {
                    obligationFilter.Items.Add(filterWithSameRelation);
                }
            }

            return obligationFilter;
        }

        public override FilterExpression CreateTotalQueryFilter()
        {
            FilterExpression obligationFilter = new FilterExpression(LogicalOperator.And);
            foreach (KeyValuePair<string, ApplySecurityFilter> item in m_filters)
            {
                Relation relation = m_join.GetRelation(item.Key);
                if (relation == null)
                {
                    continue;
                }
                AddEntityAliasToSecurityFilter(relation, item.Value);
                OphanedChildDecision ophanedChild = m_relationVSophaned[item.Key];
                FilterExpression filterWithSameRelation = CreateQueryFilterByRelation(relation, ophanedChild,item.Value);
                if (filterWithSameRelation != null)
                {
                    obligationFilter.AddFilter(filterWithSameRelation);
                }
            }
            return obligationFilter;
        }

        public void CreateEntityItems(out List<LinkEntity> linkentities, out FilterExpression obligationFilter)
        {
            linkentities = m_join.ToQueryLinkEntities();
            obligationFilter = CreateTotalQueryFilter();
        }
    }
}
