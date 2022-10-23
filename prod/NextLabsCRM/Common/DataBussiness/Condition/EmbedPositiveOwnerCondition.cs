using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    /// <summary>
    /// if the target is update the requst expression for list view, the Operater EquaUserXXX is workable,
    /// but if the target is fire a indenpendant request and get the response, EqualUserXXX cannot work,
    /// because of the user is not the calling user, for the performance, there are two implementations, 
    /// one for list view, one for others
    /// </summary>
    public class EmbedPositiveOwnerCondition:Condition
    {
        private ConditionExpression m_positiveQueryUserCondition;
        private condition m_positiveFetchUserCondition;

        public override Condition Clone()
        {
            EmbedPositiveOwnerCondition cond = new EmbedPositiveOwnerCondition();
            cond.m_positiveFetchUserCondition.entityname = this.m_positiveFetchUserCondition.entityname;
            cond.m_positiveQueryUserCondition.EntityName = this.m_positiveQueryUserCondition.EntityName;
            return cond;
        }
        public EmbedPositiveOwnerCondition()
        {
            m_positiveQueryUserCondition = CreatePositiveQueryCondition();
            m_positiveFetchUserCondition = CreatePositiveFetchCondition();
        }

        private ConditionExpression CreatePositiveQueryCondition()
        {
            ConditionExpression cond = new ConditionExpression();
            cond.AttributeName = "ownerid";
            cond.Operator = ConditionOperator.EqualUserOrUserTeams;
            return cond;
        }

        private condition CreatePositiveFetchCondition()
        {
            condition cond = new condition();
            cond.attribute = "ownerid";
            cond.@operator = @operator.equseroruserteams;
            return cond;
        }

        public override void UpdateAlias(string alias)
        {
            m_positiveQueryUserCondition.EntityName = alias;
            m_positiveFetchUserCondition.entityname = alias;
        }

        public override ConditionExpression GetQueryCondition()
        {
            return m_positiveQueryUserCondition;
        }

        public override condition GetFetchCondition()
        {
            return m_positiveFetchUserCondition;
        }
    }
}
