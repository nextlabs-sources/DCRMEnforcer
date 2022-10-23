using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    class IndependPostiveOwnerCondition:ParentKnownCondition, ILazyInitialize
    {
        private const string m_notInitExpMsg = "This PositiveOwnerCondition instance is not initialized";
        private bool m_initialized = false;

        private Guid m_userId;
        private List<Guid> m_teamIds;
        private ConditionExpression m_userOwnerQueryCondition;
        private ConditionExpression m_teamOwnerQueryCondition;

        private condition m_userOwnerFetchCondition;
        private condition m_teamOwnerFetchCondition;

        public IndependPostiveOwnerCondition()
        {

        }

        public override Condition Clone()
        {
            IndependPostiveOwnerCondition cond = new IndependPostiveOwnerCondition();
            cond.m_initialized = this.m_initialized;
            cond.m_userId = this.m_userId;

            if (m_teamIds != null)
            {
                cond.m_teamIds.AddRange(m_teamIds);
            }
            cond.m_userOwnerQueryCondition = FilterUtil.CloneCondition(m_userOwnerQueryCondition);
            cond.m_teamOwnerQueryCondition = FilterUtil.CloneCondition(m_teamOwnerQueryCondition);
            cond.m_userOwnerFetchCondition = FilterUtil.CloneCondition(m_userOwnerFetchCondition);
            cond.m_teamOwnerFetchCondition = FilterUtil.CloneCondition(m_teamOwnerFetchCondition);

            cond.m_parentFetchFilter = this.m_parentFetchFilter;
            cond.m_parentQueryFilter = this.m_parentQueryFilter;
            return cond;
        }
        public bool IsInitialized()
        {
            return m_initialized;
        }

        public void Initialize(Guid userId, List<Guid> teamIds)
        {
            //as we know every single user belongs to a team
            if (teamIds == null)
            {
                throw new ArgumentNullException("teamids cannot be null");
            }
            m_teamIds = teamIds;
            m_userId = userId;

            InitConditions();
            m_initialized = true;

            AttachParent();
        }


        private void InitConditions()
        {
            m_userOwnerQueryCondition = CreateUserOwnerQueryCondition();
            m_teamOwnerQueryCondition = CreateTeamOwnerQueryCondition();

            m_userOwnerFetchCondition = CreateUserOwnerFetchCondition();
            m_teamOwnerFetchCondition = CreateTeamOwnerFetchCondition();
        }
        private ConditionExpression CreateUserOwnerQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = "ownerid";
            cond.Operator = ConditionOperator.Equal;
            cond.Values.Add(m_userId);
            return cond;
        }

        private ConditionExpression CreateTeamOwnerQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression(
                "ownerid",
                ConditionOperator.In,
                m_teamIds.ToArray());
            return cond;
        }

        private condition CreateUserOwnerFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "ownerid";
            cond.@operator = @operator.eq;
            cond.value = m_userId.ToString();
            return cond;
        }

        private condition CreateTeamOwnerFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "ownerid";
            cond.@operator = @operator.@in;

            if (m_teamIds.Count > 0)
            {
                cond.Items = new List<global::conditionValue>();
            }

            foreach (Guid guid in m_teamIds)
            {
                conditionValue value = new conditionValue();
                value.Value = guid.ToString();
                cond.Items.Add(value);
            }
            return cond;
        }

        public override void UpdateAlias(string alias)
        {
            m_userOwnerQueryCondition.EntityName = alias;
            m_teamOwnerQueryCondition.EntityName = alias;

            m_userOwnerFetchCondition.entityname = alias;
            m_teamOwnerFetchCondition.entityname = alias;
        }

        public override filter GetFetchFilter()
        {
            if (!m_initialized)
            {
                return null;
            }
            filter targetFilter = new filter();
            targetFilter.type = filterType.or;
            targetFilter.Items = new List<object>();
            targetFilter.Items.Add(m_userOwnerFetchCondition);
            targetFilter.Items.Add(m_teamOwnerQueryCondition);

            return targetFilter;
        }
        public override FilterExpression GetQueryFilter()
        {
            if (!m_initialized)
            {
                return null;
            }
            FilterExpression filter = new FilterExpression(LogicalOperator.Or);
            filter.AddCondition(m_userOwnerQueryCondition);
            filter.AddCondition(m_teamOwnerQueryCondition);
            return filter;
        }
    }
}
