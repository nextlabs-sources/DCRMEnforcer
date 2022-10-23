using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    public class InheritFrom
    {
        protected enum OphanedChildDecision
        {
            Allow,
            Deny,
            Conflict
        }

        protected EntityOutJoin m_join = new EntityOutJoin();
        //protected List<ApplySecurityFilter> m_filters = new List<ApplySecurityFilter>();

        public EntityOutJoin OutJoin
        {
            get { return m_join; }
            set { m_join = value; }
        }

        virtual public filter CreateTotalFetchFilter()
        {
            throw new NotImplementedException();
        }

        virtual public FilterExpression CreateTotalQueryFilter()
        {
            throw new NotImplementedException();
        }

        virtual public void AddChildRelationFromVirtualCondition()
        {
            throw new NotImplementedException();
        }
        virtual protected void AddEntityAliasToSecurityFilter(Relation item, ApplySecurityFilter appFilter)
        {
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
        virtual protected filter CreateFetchFilterByRelation(Relation relation, OphanedChildDecision ophanedChild, ApplySecurityFilter appFilter)
        {
            filter parentFilter = appFilter.FetchFilter;
            filter itemFilter = new filter();
            itemFilter.Items = new List<object>();

            if (ophanedChild == OphanedChildDecision.Conflict)
            {
                itemFilter.type = filterType.and;

                filter subFilter = new filter();
                subFilter.type = filterType.and;
                subFilter.Items = new List<object>();

                condition orphanedConditionAllow = new condition();
                orphanedConditionAllow.entityname = relation.ParentEntityAlias;
                orphanedConditionAllow.attribute = relation.ParentAttribute;
                orphanedConditionAllow.@operator = @operator.@null;
                subFilter.Items.Add(orphanedConditionAllow);

                condition orphanedConditionDeny = new condition();
                orphanedConditionDeny.entityname = relation.ParentEntityAlias;
                orphanedConditionDeny.attribute = relation.ParentAttribute;
                orphanedConditionDeny.@operator = @operator.@notnull;
                subFilter.Items.Add(orphanedConditionDeny);

                itemFilter.Items.Add(subFilter);
            }
            else {

                condition orphanedCondition = new condition();
                orphanedCondition.entityname = relation.ParentEntityAlias;
                orphanedCondition.attribute = relation.ParentAttribute;
                if (ophanedChild == OphanedChildDecision.Allow)
                {
                    itemFilter.type = filterType.or;
                    orphanedCondition.@operator = @operator.@null;
                }
                else
                {
                    itemFilter.type = filterType.and;
                    orphanedCondition.@operator = @operator.@notnull;     
                }
                itemFilter.Items.Add(orphanedCondition);
            }

            if (parentFilter != null)
            {
                itemFilter.Items.Add(parentFilter);
            }
            
            return itemFilter;
        }

        virtual protected FilterExpression CreateQueryFilterByRelation(Relation relation, OphanedChildDecision ophanedChild, ApplySecurityFilter appFilter)
        {
            FilterExpression parentFilter = appFilter.QueryFilter;
            FilterExpression itemFilter = new FilterExpression();

            if (ophanedChild == OphanedChildDecision.Conflict)
            {
                ConditionExpression orphanedConditionAllow = new ConditionExpression();
                orphanedConditionAllow.EntityName = relation.ParentEntityAlias;
                orphanedConditionAllow.AttributeName = relation.ParentAttribute;
                orphanedConditionAllow.Operator = ConditionOperator.Null;

                ConditionExpression orphanedConditionDeny = new ConditionExpression();
                orphanedConditionDeny.EntityName = relation.ParentEntityAlias;
                orphanedConditionDeny.AttributeName = relation.ParentAttribute;
                orphanedConditionDeny.Operator = ConditionOperator.NotNull;

                FilterExpression subFilter = new FilterExpression(LogicalOperator.And);
                subFilter.AddCondition(orphanedConditionAllow);
                subFilter.AddCondition(orphanedConditionDeny);

                itemFilter.FilterOperator = LogicalOperator.And;
                itemFilter.AddFilter(subFilter);
            }
            else
            {
                ConditionExpression orphanedCondition = new ConditionExpression();
                orphanedCondition.EntityName = relation.ParentEntityAlias;
                orphanedCondition.AttributeName = relation.ParentAttribute;
                if (ophanedChild == OphanedChildDecision.Allow)
                {
                    itemFilter.FilterOperator = LogicalOperator.Or;
                    orphanedCondition.Operator = ConditionOperator.Null;
                }
                else
                {
                    itemFilter.FilterOperator = LogicalOperator.And;
                    orphanedCondition.Operator = ConditionOperator.NotNull;
                }

                itemFilter.AddCondition(orphanedCondition);
            }

            if (parentFilter != null)
            {
                itemFilter.AddFilter(parentFilter);
            }
            
            return itemFilter;
        }
    }
}
