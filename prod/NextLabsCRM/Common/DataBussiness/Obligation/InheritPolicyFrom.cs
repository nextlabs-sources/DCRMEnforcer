using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    class InheritPolicyFrom:InheritFrom
    {
        private Dictionary<string,ApplySecurityFilter> m_filters = new Dictionary<string, ApplySecurityFilter>();
        private Dictionary<string, OphanedChildDecision> m_relationVSophaned = new Dictionary<string, OphanedChildDecision>();
        public void AddRelation(Relation relation)
        {
            if(relation == null)
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
                if(ophanedChild != OphanedChildDecision.Conflict)
                {
                    if(oldOphaned != ophanedChild)
                    {
                        m_relationVSophaned[relation.Name] = OphanedChildDecision.Conflict;
                    }
                }
            }
        }

        public void AddParentSecurityFilter(string relationshipName,ApplySecurityFilter securityFilter)
        {
            if (securityFilter == null)
            {
                return;
            }

            if (!m_filters.ContainsKey(relationshipName))
            {
                m_filters[relationshipName] = securityFilter;
            }
        }

        public override void AddChildRelationFromVirtualCondition()
        {
            foreach(KeyValuePair<string,ApplySecurityFilter> securityFilter in m_filters)
            {
                List<Relation> parentRelations = m_join.GetRelationByParentName(securityFilter.Value.EntityName);
                if(parentRelations == null)
                {
                    continue;
                }
                
                List<Relation> relations = securityFilter.Value.Relations;
                if(relations == null)
                {
                    continue;
                }
                    
                foreach(Relation parentRelation in parentRelations)
                {
                    parentRelation.AddChildRelations(relations);
                }
                
            }
        }
        public void CreateEntityItems(out List<FetchLinkEntityType>  linkentities,out filter obligationFilter)
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

            //<string,ApplySecurityFilter> is <relationshipname, ASF>
            foreach (KeyValuePair<string,ApplySecurityFilter> appFilter in m_filters)
            {
                Relation relation = m_join.GetRelation(appFilter.Key);
                if(relation == null)
                {
                    continue;
                }

                OphanedChildDecision ophanedChild = m_relationVSophaned[relation.Name];
                AddEntityAliasToSecurityFilter(relation, appFilter.Value);
                obligationFilter.Items.Add(CreateFetchFilterByRelation(relation, ophanedChild, appFilter.Value));  
            }

            return obligationFilter;
        }
        //private filter CreateFetchFilterByParentName(ApplySecurityFilter appFilter)
        //{
        //    List<Relation> relations = m_join.GetRelationByParentName(appFilter.EntityName);
        //    if(relations == null || relations.Count == 0)
        //    {
        //        return null;
        //    }

        //    filter relationshipFilter = new filter();
        //    relationshipFilter.type = filterType.and;
        //    relationshipFilter.Items = new List<object>();
        //    foreach (Relation item in relations)
        //    {
        //        OphanedChildDecision ophanedChild = m_relationVSophaned[item.Name];
        //        ApplySecurityFilter clonedAppFilter = appFilter.Clone();
        //        AddEntityAliasToSecurityFilter(item, clonedAppFilter);
        //        relationshipFilter.Items.Add(CreateFetchFilterByRelation(item, ophanedChild, clonedAppFilter));
        //    }

        //    return relationshipFilter;
        //}

        public void CreateEntityItems(out List<LinkEntity> linkentities, out FilterExpression obligationFilter)
        {
            linkentities = m_join.ToQueryLinkEntities();
            obligationFilter = CreateTotalQueryFilter();
        }

        public override FilterExpression CreateTotalQueryFilter()
        {
            FilterExpression obligationFilter = new FilterExpression(LogicalOperator.And);
            foreach (KeyValuePair<string,ApplySecurityFilter> appFilter in m_filters)
            {
                Relation relation = m_join.GetRelation(appFilter.Key);
                if (relation == null)
                {
                    continue;
                }

                OphanedChildDecision ophanedChild = m_relationVSophaned[relation.Name];
                AddEntityAliasToSecurityFilter(relation, appFilter.Value);
                obligationFilter.AddFilter(CreateQueryFilterByRelation(relation, ophanedChild, appFilter.Value));
              
            }

            return obligationFilter;
        }
        //private FilterExpression CreateQueryFilterByParentName(ApplySecurityFilter appFilter)
        //{
        //    List<Relation> relations = m_join.GetRelationByParentName(appFilter.EntityName);
        //    if (relations == null || relations.Count == 0)
        //    {
        //        return null;
        //    }

        //    FilterExpression relationshipFilter = new FilterExpression(LogicalOperator.And);
        //    foreach (Relation item in relations)
        //    {
        //        if (m_relationVSophaned.ContainsKey(item.Name))
        //        {
        //            OphanedChildDecision ophanedChild = m_relationVSophaned[item.Name];
        //            ApplySecurityFilter clonedAppFilter = appFilter.Clone();
        //            AddEntityAliasToSecurityFilter(item, clonedAppFilter);
        //            relationshipFilter.AddFilter(CreateQueryFilterByRelation(item, ophanedChild, clonedAppFilter));
        //        }
        //    }

        //    return relationshipFilter;
        //}


        protected override void AddEntityAliasToSecurityFilter(Relation item, ApplySecurityFilter appFilter)
        {
            //base.AddEntityAliasToSecurityFilter(item, appFilter);
            if (string.IsNullOrWhiteSpace(item.ParentEntityAlias))
            {
                return;
            }

            appFilter.EntityAlias = item.ParentEntityAlias;
            foreach (Condition.Condition express in appFilter.Conditions)
            {
                express.UpdateAlias(item.ParentEntityAlias);
            }
        }
    }
}
