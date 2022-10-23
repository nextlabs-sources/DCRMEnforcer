using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    public abstract class ParentKnownCondition:Condition
    {
        protected FilterExpression m_parentQueryFilter;
        protected filter m_parentFetchFilter;
        
        public override void AttachParent(FilterExpression parent)
        {
            m_parentQueryFilter = parent;
             
        }

        public override void AttachParent(filter parent)
        {
            m_parentFetchFilter = parent;
        }

        protected void AttachParent()
        {
            ConditionExpression queryCondition = GetQueryCondition();
            FilterExpression queryFilter = GetQueryFilter();

            condition fetchCondition = GetFetchCondition();
            filter fetchFilter = GetFetchFilter();

            if(queryCondition != null)
            {
                m_parentQueryFilter.AddCondition(queryCondition);
            }

            if(queryFilter != null)
            {
                m_parentQueryFilter.AddFilter(queryFilter);
            }

            if(fetchCondition != null)
            {
                m_parentFetchFilter.Items.Add(fetchCondition);
            }

            if(fetchFilter != null)
            {
                m_parentFetchFilter.Items.Add(fetchFilter);
            }    
        }
    }
}
