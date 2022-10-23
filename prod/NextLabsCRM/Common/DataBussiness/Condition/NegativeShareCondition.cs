using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    class NegativeShareCondition: ParentKnownCondition, ILazyInitialize
    {
        private const string m_notInitExpMsg = "This NegativeShareCondition instance is not initialized";
        private string m_entityname;
        private string m_entityPrimaryId;
        private bool m_initialized = false;
        private List<Guid> m_recordIds;
        private ConditionExpression m_notSharedRecordsQueryCondition;
        private condition m_notSharedRecordsFetchCondition;

        private string m_alias;

        public override Condition Clone()
        {
            NegativeShareCondition cond = new NegativeShareCondition(this.m_entityname,this.m_entityPrimaryId);
            cond.m_initialized = this.m_initialized;

            if (this.m_recordIds != null)
            {
                cond.m_recordIds.AddRange(m_recordIds);
            }

            cond.m_notSharedRecordsQueryCondition = FilterUtil.CloneCondition(m_notSharedRecordsQueryCondition);
            cond.m_notSharedRecordsFetchCondition = FilterUtil.CloneCondition(m_notSharedRecordsFetchCondition);

            cond.m_parentFetchFilter = this.m_parentFetchFilter;
            cond.m_parentQueryFilter = this.m_parentQueryFilter;
            cond.m_alias = this.m_alias;
            return cond;
        }
        public NegativeShareCondition(string entityName, string primaryId)
        {
            if(string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entity name cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(primaryId))
            {
                throw new ArgumentNullException("primary Id name cannot be null or empty");
            }
            m_entityname = entityName;
            m_entityPrimaryId = primaryId;
        }

        public bool IsInitialized()
        {
            return m_initialized;
        }

        public void InstanceInitialize(Guid userId, List<Guid> sharedRecords)
        {
            if (sharedRecords == null || sharedRecords.Count == 0)
            {
                //when ishared = deny and there is no record has been shared, that is, there is no record will be denied
                //so adding condtion is not necessary
            }
            else
            {
                m_recordIds = sharedRecords;
                InitConditions();
            }
            m_initialized = true;
        }

        public void Initialize(Guid userId, List<Guid> sharedRecords)
        {
            InstanceInitialize(userId, sharedRecords);
            AttachParent();
        }

        public string GetEntityName()
        {
            return m_entityname;
        }

        private void InitConditions()
        {
            m_notSharedRecordsQueryCondition = CreateNotSharedRecordsQueryCondition();

            m_notSharedRecordsFetchCondition = CreateNotSharedRecordsFetchCondition();
        }
        private ConditionExpression CreateNotSharedRecordsQueryCondition()
        {
            if(m_recordIds == null)
            {
                return null;
            }
            ConditionExpression cond = new ConditionExpression(
                m_entityPrimaryId,
                ConditionOperator.NotIn,
                m_recordIds.ToArray());
            return cond;
        }

        private condition CreateNotSharedRecordsFetchCondition()
        {
            if (m_recordIds == null)
            {
                return null;
            }
            condition cond = new condition();
            cond.attribute = m_entityPrimaryId;
            cond.@operator = @operator.notin;
            //cond.value = string.Join(",", m_recordIds) ;

            if (m_recordIds.Count > 0)
            {
                cond.Items = new List<global::conditionValue>();
            }

            foreach (Guid guid in m_recordIds)
            {
                conditionValue value = new conditionValue();
                value.Value = guid.ToString();
                cond.Items.Add(value);
            }
            return cond;
        }

        private void UpdateAlias()
        {
            if (m_notSharedRecordsQueryCondition != null)
            {
                m_notSharedRecordsQueryCondition.EntityName = m_alias;
            }

            if (m_notSharedRecordsFetchCondition != null)
            {
                m_notSharedRecordsFetchCondition.entityname = m_alias;
            }
        }
        public override void UpdateAlias(string alias)
        {
            if (m_notSharedRecordsQueryCondition != null)
            {
                m_notSharedRecordsQueryCondition.EntityName = alias;
            }

            if (m_notSharedRecordsFetchCondition != null)
            {
                m_notSharedRecordsFetchCondition.entityname = alias;
            }
        }

        public override ConditionExpression GetQueryCondition()
        {
            if (!m_initialized)
            {
                return null;
            }
            return m_notSharedRecordsQueryCondition;
        }

        public override condition GetFetchCondition()
        {
            if (!m_initialized)
            {
                return null;
            }
            return m_notSharedRecordsFetchCondition;
        }

        
    }
}
