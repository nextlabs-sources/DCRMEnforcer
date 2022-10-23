using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    public class NegativeOwnerCondition: ParentKnownCondition, ILazyInitialize
    {
        private const string m_notInitExpMsg = "This NegativeOwnerCondition instance is not initialized";
        private bool m_initialized = false;

        private Guid m_userId;
        private List<Guid> m_notInTeamIds;
        private ConditionExpression m_userNotOwnerQueryCondition;
        private ConditionExpression m_teamNotOwnerQueryCondition;

        private condition m_userNotOwnerFetchCondition;
        private condition m_teamNotOwnerFetchCondition;

        public NegativeOwnerCondition()
        {

        }

        public override Condition Clone()
        {
            NegativeOwnerCondition cond = new NegativeOwnerCondition();
            cond.m_initialized = this.m_initialized;
            cond.m_userId = this.m_userId;

            if (m_notInTeamIds != null)
            {
                cond.m_notInTeamIds.AddRange(this.m_notInTeamIds);
            }
            
            cond.m_userNotOwnerQueryCondition = FilterUtil.CloneCondition(m_userNotOwnerQueryCondition);
            cond.m_teamNotOwnerQueryCondition = FilterUtil.CloneCondition(m_teamNotOwnerQueryCondition);
            cond.m_userNotOwnerFetchCondition = FilterUtil.CloneCondition(m_userNotOwnerFetchCondition);
            cond.m_teamNotOwnerFetchCondition = FilterUtil.CloneCondition(m_teamNotOwnerFetchCondition);

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
            m_notInTeamIds = teamIds;
            m_userId = userId;

            InitConditions();
            m_initialized = true;

            AttachParent();
        }


        private void InitConditions()
        {
            m_userNotOwnerQueryCondition = CreateUserNotOwnerQueryCondition();
            m_teamNotOwnerQueryCondition = CreateTeamNotOwnerQueryCondition();

            m_userNotOwnerFetchCondition = CreateUserNotOwnerFetchCondition();
            m_teamNotOwnerFetchCondition = CreateTeamNotOwnerFetchCondition();
        }
        private ConditionExpression CreateUserNotOwnerQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = "ownerid";
            cond.Operator = ConditionOperator.NotEqual;
            cond.Values.Add(m_userId);
            return cond;
        }

        private ConditionExpression CreateTeamNotOwnerQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression(
                "ownerid",
                ConditionOperator.NotIn,
                m_notInTeamIds.ToArray());
            return cond;
        }

        private condition CreateUserNotOwnerFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "ownerid";
            cond.@operator = @operator.neq;
            cond.value = m_userId.ToString();
            return cond;
        }

        private condition CreateTeamNotOwnerFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "ownerid";
            cond.@operator = @operator.notin;
            //cond.value = string.Join(",", m_notInTeamIds);

            if (m_notInTeamIds.Count > 0)
            {
                cond.Items = new List<global::conditionValue>();
            }

            foreach (Guid guid in m_notInTeamIds)
            {
                conditionValue value = new conditionValue();
                value.Value = guid.ToString();
                cond.Items.Add(value);
            }
            return cond;
        }

        public override void UpdateAlias(string alias)
        {
            m_userNotOwnerQueryCondition.EntityName = alias;
            m_teamNotOwnerQueryCondition.EntityName = alias;

            m_userNotOwnerFetchCondition.entityname = alias;
            m_teamNotOwnerFetchCondition.entityname = alias;
        }

        public override filter GetFetchFilter()
        {
            if (!m_initialized)
            {
                return null;
            }
            filter targetFilter = new filter();
            targetFilter.type = filterType.and;
            targetFilter.Items = new List<object>();
            targetFilter.Items.Add(m_userNotOwnerFetchCondition);
            targetFilter.Items.Add(m_teamNotOwnerFetchCondition);

            return targetFilter;
        }
        public override FilterExpression GetQueryFilter()
        {
            if (!m_initialized)
            {
                return null;
            }
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(m_userNotOwnerQueryCondition);
            filter.AddCondition(m_teamNotOwnerQueryCondition);
            return filter;
        }
    }
}
