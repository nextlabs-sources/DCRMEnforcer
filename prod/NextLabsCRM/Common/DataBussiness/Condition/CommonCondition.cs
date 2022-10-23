using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    class CommonCondition:Condition
    {
        private condition m_fetchCondition;
        private ConditionExpression m_queryCondition;

        public override Condition Clone()
        {
            condition fetchCond = m_fetchCondition != null ? FilterUtil.CloneCondition(m_fetchCondition) : null;
            ConditionExpression queryCond = m_queryCondition != null? FilterUtil.CloneCondition(m_queryCondition) : null;
            CommonCondition cond = new CommonCondition(fetchCond, queryCond);
            return cond;
        }
        public CommonCondition(condition fetchCond, ConditionExpression queryCond)
        {
            this.m_fetchCondition = fetchCond;
            this.m_queryCondition = queryCond;
        }

        public CommonCondition(ConditionExpression queryCondition)
        {
            condition cond = new condition();
            cond.@operator = m_queryVSfetchOperator[queryCondition.Operator];
            if (queryCondition.Values != null&&queryCondition.Values.Count>0)
            {
                cond.value = string.Join(",", queryCondition.Values[0]);
            }
            cond.attribute = queryCondition.AttributeName;

            this.m_fetchCondition = cond;
            this.m_queryCondition = queryCondition;
        }
        public override condition GetFetchCondition()
        {
            return this.m_fetchCondition;
        }

        public override ConditionExpression GetQueryCondition()
        { 
            return this.m_queryCondition;
        }

        public override void UpdateAlias(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                return;
            }
            if (m_fetchCondition != null)
            {
                m_fetchCondition.entityname = alias;
            }

            if (m_queryCondition != null)
            {
                m_queryCondition.EntityName = alias;
            }
        }
    }
}
