using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Log;
using Microsoft.Xrm.Sdk;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.QueryPC;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;
using NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle;
using NextLabs.CRMEnforcer.Common.Enums;

namespace NextLabs.CRMEnforcer.Enforces
{
    class Delete : EnforceBase, IEnforce
    {
        private IOrganizationService m_service = null;
        private string m_strCurrentEntity = null;
        private AttributeCollection m_userAttrs = null;
        private MemoryCache<SecureEntity> m_userAttrsStruct = null;
        private MemoryCache<SecureEntity> m_entityAttrsStruct = null;
        private Guid m_InitUserId = Guid.Empty;
        private MemoryCache<N1Relationship> m_n1RelationshipCache = null;
        private MemoryCache<NNRelationship> m_nnRelationshipCache = null;
        private EntityReference m_currentRecordEntity = null;
        private string m_strDefaultDenyMessage = null;
        private ParameterCollection m_shareParam = null;

        private EnforceResult m_enforceResult = null;
        //private List<Guid> m_teamIds = null;
        private Guid m_ownerId = Guid.Empty;
        private AttributeCollection m_entityAttrs = null;
        private List<Guid> m_userTeams;
        private Dictionary<string, List<Guid>> m_sharedRecords;
        private ConditionFactory m_conditionFactory;
        public Delete(IOrganizationService service,
                                            string strCurrentEntity,
                                            AttributeCollection userAttrCollection,
                                            MemoryCache<SecureEntity> userEntityCache,
                                            MemoryCache<SecureEntity> secureEntityCache,
                                            NextLabsCRMLogs log,
                                            Guid initUserId,
                                            MemoryCache<N1Relationship> n1RelationshipCache,
                                            MemoryCache<NNRelationship> nnRelationshipCache,
                                            string strDefaultDenyMessage,
                                            EntityReference currentRecordEntity,
                                            ParameterCollection shareParam,
                                            List<Guid> userTeams,
                                            Dictionary<string,List<Guid>> sharedRecords,
                                            ConditionFactory conditionFactory,
                                            string userHostIP) : base(log, userHostIP)
        {
            m_service = service;
            m_strCurrentEntity = strCurrentEntity;
            m_userAttrs = userAttrCollection;
            m_userAttrsStruct = userEntityCache;
            m_entityAttrsStruct = secureEntityCache;
            m_InitUserId = initUserId;
            m_n1RelationshipCache = n1RelationshipCache;
            m_nnRelationshipCache = nnRelationshipCache;
            m_strDefaultDenyMessage = strDefaultDenyMessage;
            m_currentRecordEntity = currentRecordEntity;
            m_shareParam = shareParam;
            m_userTeams = userTeams;
            m_sharedRecords = sharedRecords;
            m_conditionFactory = conditionFactory;
        }
        public EnforceResult DoEnforce(bool bIgnoreInherit)
        {
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("{0} Start DoEnforce for entity {2} ,Ignore inherit {1}", this.GetType().ToString(), bIgnoreInherit, m_strCurrentEntity));
            if (m_enforceResult == null)
            {
                ColumnSet columnSetGetAllAttribute = EntityDataHelp.GetEntityColumn(m_strCurrentEntity, m_entityAttrsStruct);
                m_entityAttrs = EntityDataHelp.RertieveSingle(m_service, m_strCurrentEntity, m_currentRecordEntity.Id, columnSetGetAllAttribute);

                bool? isOwner = EntityDataHelp.AppendIsOwnerFakeAttribute(m_entityAttrs, m_strCurrentEntity, m_entityAttrsStruct, m_InitUserId, m_userTeams);
                bool? isShared = EntityDataHelp.AppendIsSharedFakeAttribute(m_entityAttrs, m_strCurrentEntity, m_entityAttrsStruct, m_currentRecordEntity.Id, m_sharedRecords);

                CEUser ceUser = GetCEUser(m_userAttrs, m_userAttrsStruct);

                CEResource ceResource = GetCEResource(m_entityAttrs, m_entityAttrsStruct, m_strCurrentEntity);
                CEHost ceHost = GetCEHost();
                CEApp ceApp = GetCEApp();
                CQuery query = new CQuery();
                m_enforceResult = query.QueryJavaPCEva(m_log, Common.Constant.Policy.Action.Delete, m_strCurrentEntity, ceResource, ceUser, ceHost, ceApp);
                m_log.WriteLog(LogLevel.Debug, string.Format("PDP return {0} on {1} action for entity {2}", m_enforceResult.Decision, Common.Constant.Policy.Action.Delete, m_strCurrentEntity));
                if (m_enforceResult.Decision == CEResponse.CEAllow)
                {
                    Retrieve retrieveEnforcer = new Retrieve(m_service,
                                                                            m_strCurrentEntity,
                                                                            m_userAttrs,
                                                                            m_userAttrsStruct,
                                                                            m_entityAttrsStruct,
                                                                            m_log,
                                                                            m_InitUserId,
                                                                            m_n1RelationshipCache,
                                                                            m_nnRelationshipCache,
                                                                            m_strDefaultDenyMessage,
                                                                            m_currentRecordEntity.Id,
                                                                            columnSetGetAllAttribute,
                                                                            new ParameterCollection(),
                                                                            m_userTeams,
                                                                            m_sharedRecords,
                                                                            m_conditionFactory,
                                                                            m_userHostIP);
                    EnforceResult retrieveEnforceResult = retrieveEnforcer.DoEnforce(false);
                    if (retrieveEnforceResult.Decision.Equals(CEResponse.CEAllow))
                    {
                        m_log.WriteLog(Common.Enums.LogLevel.Debug, "Start anaysis obligation from delete action for entity " + m_strCurrentEntity);
                        if (!bIgnoreInherit)
                        {
                            InheritPoliciesFromHanlde inheritPolicyFromHanlde = new InheritPoliciesFromHanlde(m_strCurrentEntity,
                                                                                                                                                    m_enforceResult.ListObligations,
                                                                                                                                                    m_entityAttrsStruct,
                                                                                                                                                       m_entityAttrs,
                                                                                                                                                        m_userAttrsStruct,
                                                                                                                                                       m_userAttrs,
                                                                                                                                                       m_conditionFactory,
                                                                                                                                                       m_log,
                                                                                                                                                       m_service,
                                                                                                                                                       m_n1RelationshipCache,
                                                                                                                                                       m_nnRelationshipCache,
                                                                                                                                                       m_strDefaultDenyMessage,
                                                                                                                                                       m_InitUserId,
                                                                                                                                                       new QueryExpression(),
                                                                                                                                                      m_shareParam,
                                                                                                                                                      m_userTeams,
                                                                                                                                                      m_sharedRecords,
                                                                                                                                                       m_userHostIP);
                            EnforceResult inheritEnforceResult = inheritPolicyFromHanlde.GetEnforceResult(m_entityAttrs, Common.Constant.MessageName.Delete);
                            m_log.WriteLog(LogLevel.Debug, "InheritPoliciesFromHanlde return final decision " + inheritEnforceResult.Decision);
                            if (inheritEnforceResult.Decision.Equals(CEResponse.CEAllow))
                            {
                                m_log.WriteLog(Common.Enums.LogLevel.Debug, "Inherit from  return Allow, Start anaysis obligation");
                                ApplySecurityFilterBasedonParentAttributesHandle applySecurityBaseOnParentHandle = 
                                    new ApplySecurityFilterBasedonParentAttributesHandle(m_service,
                                                                                                                    m_enforceResult.ListObligations,
                                                                                                                    m_entityAttrsStruct,
                                                                                                                    m_conditionFactory,
                                                                                                                    m_strCurrentEntity,
                                                                                                                    m_n1RelationshipCache,
                                                                                                                    m_nnRelationshipCache,
                                                                                                                    m_InitUserId,
                                                                                                                    m_userTeams,
                                                                                                                    m_sharedRecords,
                                                                                                                    m_log);
                                if (!applySecurityBaseOnParentHandle.GetEnforceResult(m_entityAttrs).Decision.Equals(CEResponse.CEAllow))
                                {
                                    m_enforceResult.Decision = CEResponse.CEDontCare;
                                }
                            }
                            else
                            {
                                m_enforceResult.Decision = CEResponse.CEDontCare;
                            }
                        }
                    }
                    else
                    {
                        m_enforceResult.Decision = CEResponse.CEDontCare;
                    }

                }
            }
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("Enforce result {0} , Obligation count {1}", m_enforceResult.Decision, m_enforceResult.ListObligations.Count));
            return m_enforceResult;
        }

        public void ExecuteEnforceResult()
        {
            if (m_enforceResult != null)
            {
                if (m_enforceResult.Decision.Equals(CEResponse.CEAllow))
                {
                   //do allow
                }
                else if (!m_enforceResult.Decision.Equals(CEResponse.CEAllow))
                {
                    DisplayAlertMessageHandle displayMessage = null;
                    if (m_enforceResult.Decision.Equals(CEResponse.CEDeny))
                    {
                        displayMessage = new DisplayAlertMessageHandle(m_enforceResult.ListObligations, Common.Constant.MessageName.Update);
                    }
                    else
                    {
                        displayMessage = new DisplayAlertMessageHandle(m_strDefaultDenyMessage, Common.Constant.MessageName.Update);
                    }
                    displayMessage.DoObligation();
                }
            }
            else
            {
                throw new Exceptions.EnforceNotExcuted();
            }
        }
    }
}
