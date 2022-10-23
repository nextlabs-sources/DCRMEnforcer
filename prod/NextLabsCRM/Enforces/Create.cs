using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.QueryPC;
using NextLabs.CRMEnforcer.Log;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Enforces
{
    class Create : EnforceBase, IEnforce
    {
        private IOrganizationService m_service = null;
        private string m_strCurrentEntity = null;
        private AttributeCollection m_userAttrs = null;
        private MemoryCache<SecureEntity> m_userAttrsStruct = null;
        private MemoryCache<SecureEntity> m_entityAttrsStruct = null;
        private Guid m_InitUserId = Guid.Empty;
        private Entity m_currentRecordEntity = null;
        private string m_strDefaultDenyMessage = null;

        private EnforceResult m_enforceResult = null;
        public Create(IOrganizationService service,
                                            string strCurrentEntity,
                                            AttributeCollection userAttrCollection,
                                            MemoryCache<SecureEntity> userEntityCache,
                                            MemoryCache<SecureEntity> secureEntityCache,
                                            NextLabsCRMLogs log,
                                            Guid initUserId,
                                            string strDefaultDenyMessage,
                                            Entity currentRecordEntity,
                                            string userHostIP) :base(log, userHostIP)
        {
            m_service = service;
            m_strCurrentEntity = strCurrentEntity;
            m_userAttrs = userAttrCollection;
            m_userAttrsStruct = userEntityCache;
            m_entityAttrsStruct = secureEntityCache;
            m_InitUserId = initUserId;
            m_strDefaultDenyMessage = strDefaultDenyMessage;
            m_currentRecordEntity = currentRecordEntity;
        }

        public EnforceResult DoEnforce(bool bIgnoreInherit)
        {
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("{0} Start DoEnforce ,Ignore inherit {1}", this.GetType().ToString(), bIgnoreInherit.ToString()));
            if (m_enforceResult == null)
            {
                ColumnSet columnSetGetAllAttribute = EntityDataHelp.GetEntityColumn(m_strCurrentEntity, m_entityAttrsStruct);
                AttributeCollection recordAttrs = EntityDataHelp.RertieveSingle(m_currentRecordEntity.Attributes, columnSetGetAllAttribute);

                EntityDataHelp.AppendIsOwnerFakeAttribute(m_service, recordAttrs, m_strCurrentEntity, m_entityAttrsStruct, m_InitUserId);
                EntityDataHelp.AppendIsSharedFakeAttribute(m_service, recordAttrs, m_strCurrentEntity, m_entityAttrsStruct, m_InitUserId, m_currentRecordEntity.Id);

                CEUser ceUser = GetCEUser(m_userAttrs, m_userAttrsStruct);

                CEResource ceResource = GetCEResource(recordAttrs, m_entityAttrsStruct, m_strCurrentEntity);
                CEHost ceHost = GetCEHost();
                CEApp ceApp = GetCEApp();
                CQuery query = new CQuery();
                m_enforceResult = query.QueryJavaPCEva(m_log, Common.Constant.Policy.Action.Create, m_strCurrentEntity, ceResource, ceUser, ceHost, ceApp);
            }
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("Enforce result {0} , Obligation count {1}", m_enforceResult.Decision, m_enforceResult.ListObligations.Count));
            return m_enforceResult;
        }

        public void ExecuteEnforceResult()
        {
            if(m_enforceResult!=null)
            {
                if(m_enforceResult.Decision!=CEResponse.CEAllow)
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
