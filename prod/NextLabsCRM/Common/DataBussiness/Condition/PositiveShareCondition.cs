using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    public class PositiveShareCondition: ParentKnownCondition, ILazyInitialize
    {
        private const string m_notInitExpMsg = "This PositiveShareCondition instance is not initialized";
        private string m_entityname;
        private string m_entityPrimaryId;
        private bool m_initialized = false;
        private List<Guid> m_recordIds;
        private ConditionExpression m_sharedRecordsQueryCondition;
        private condition m_sharedRecordsFetchCondition;

        public override Condition Clone()
        {
            PositiveShareCondition cond = new PositiveShareCondition(this.m_entityname, this.m_entityPrimaryId);
            cond.m_initialized = this.m_initialized;

            if (this.m_recordIds != null)
            {
                cond.m_recordIds.AddRange(m_recordIds);
            }

            cond.m_sharedRecordsQueryCondition = FilterUtil.CloneCondition(m_sharedRecordsQueryCondition);
            cond.m_sharedRecordsFetchCondition = FilterUtil.CloneCondition(m_sharedRecordsFetchCondition);

            cond.m_parentFetchFilter = this.m_parentFetchFilter;
            cond.m_parentQueryFilter = this.m_parentQueryFilter;
            return cond;
        }
        public PositiveShareCondition(string entityName, string primaryIdName)
        {
          
            if (string.IsNullOrWhiteSpace(entityName))
            {
                throw new ArgumentNullException("entity name cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(primaryIdName))
            {
                throw new ArgumentNullException("primaryId name cannot be null or empty");
            }

            m_entityname = entityName;
            m_entityPrimaryId = primaryIdName;
        }

        public bool IsInitialized()
        {
            return m_initialized;
        }

        public void InstanceInitialize(Guid userId, List<Guid> sharedRecords)
        {
            if (sharedRecords == null || sharedRecords.Count == 0)
            {
                //when ishared = allow but there is no record has been shared, that is, there is no record will be allowed
                //so need add a condtion always failed here
                m_sharedRecordsQueryCondition = CreateAlwaysFailedQueryCondition();
                m_sharedRecordsFetchCondition = CreateAlwaysFailedFetchCondition();
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
            m_sharedRecordsQueryCondition = CreateSharedRecordsQueryCondition();

            m_sharedRecordsFetchCondition = CreateSharedRecordsFetchCondition();
        }
        private ConditionExpression CreateSharedRecordsQueryCondition()
        {
            if (m_recordIds == null)
            {
                return null;
            }

            ConditionExpression cond = new ConditionExpression(m_entityPrimaryId,
                ConditionOperator.In,
                 m_recordIds.ToArray());
            return cond;
        }

        
        private condition CreateSharedRecordsFetchCondition()
        {
            if (m_recordIds == null)
            {
                return null;
            }
            condition cond = new condition();
            cond.attribute = m_entityPrimaryId;
            cond.@operator = @operator.@in;
            //cond.value = string.Join(",", m_recordIds);

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
        private  void UpdateAlias()
        {
           
        }
        public override void UpdateAlias(string alias)
        {
            if (m_sharedRecordsQueryCondition != null)
            {
                m_sharedRecordsQueryCondition.EntityName = alias;
            }

            if (m_sharedRecordsFetchCondition != null)
            {
                m_sharedRecordsFetchCondition.entityname = alias;
            }
        }

        public override ConditionExpression GetQueryCondition()
        {
            if (!m_initialized)
            {
                return null;
            }

            return m_sharedRecordsQueryCondition;
        }

        public override condition GetFetchCondition()
        {
            if (!m_initialized)
            {
                return null;
            }
            return m_sharedRecordsFetchCondition;
        }
    }
}
