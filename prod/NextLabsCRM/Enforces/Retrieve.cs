using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.QueryPC;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Log;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.DataBussiness.IO;
using NextLabs.CRMEnforcer.Common.DataModel.MetaData;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;
using NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using NextLabs.CRMEnforcer.Common.Enums;

namespace NextLabs.CRMEnforcer.Enforces
{
    class Retrieve : EnforceBase, IEnforce
    {
        private IOrganizationService m_service = null;
        private string m_strCurrentEntity = null;
        private AttributeCollection m_userAttrs = null;
        private MemoryCache<SecureEntity> m_userAttrsStruct = null;
        private MemoryCache<SecureEntity> m_entityAttrsStruct = null;
        private EnforceResult m_enforceResult = null;
        private Guid m_InitUserId = Guid.Empty;
        private MemoryCache<N1Relationship> m_n1RelationshipCache = null;
        private MemoryCache<NNRelationship> m_nnRelationshipCache = null;
        private Guid m_currentRecordID = Guid.Empty;
        private ColumnSet m_currentColumnSet = null;
        private ParameterCollection m_sharedParam = null;
        private Guid m_ownerId = Guid.Empty;
        private AttributeCollection m_entityAttrs = null;
        private ColumnSet m_columnSet = null;
        private List<Guid> m_userTeamIds = null;
        private string m_strDefaultDenyMessage = null;
        private Dictionary<string, List<Guid>> m_sharedRecords;
        private ConditionFactory m_conditionFactory;
        public Retrieve(IOrganizationService service,
                                            string strCurrentEntity,
                                            AttributeCollection userAttrCollection,
                                            MemoryCache<SecureEntity> userEntityCache,
                                            MemoryCache<SecureEntity> secureEntityCache,
                                            NextLabsCRMLogs log,
                                            Guid initUserId,
                                            MemoryCache<N1Relationship> n1RelationshipCache,
                                            MemoryCache<NNRelationship> nnRelationshipCache,
                                            string strDefaultDenyMessage,
                                            Guid currentRecordID,
                                            ColumnSet currentColumnset,
                                            ParameterCollection sharedParam,
                                            List<Guid> userTeams,
                                            Dictionary<string, List<Guid>> sharedRecords,
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
            m_currentRecordID = currentRecordID;
            m_columnSet = currentColumnset;
            m_sharedParam = sharedParam;
            m_userTeamIds = userTeams;
            m_sharedRecords = sharedRecords;
            m_conditionFactory = conditionFactory;
        }
        public EnforceResult DoEnforce(bool bIgnoreInherit)
        {
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("{0} Start DoEnforce for entity {2} ,Ignore inherit {1}", this.GetType().ToString(), bIgnoreInherit,m_strCurrentEntity));
            if (m_enforceResult == null)
            {
                ColumnSet columnSetGetAllAttribute = Common.DataBussiness.EntityDataHelp.GetEntityColumn(m_strCurrentEntity, m_entityAttrsStruct);
                m_entityAttrs = Common.DataBussiness.EntityDataHelp.RertieveSingle(m_service, m_strCurrentEntity, m_currentRecordID, columnSetGetAllAttribute);

                bool? isOwner = EntityDataHelp.AppendIsOwnerFakeAttribute(m_entityAttrs, m_strCurrentEntity, m_entityAttrsStruct, m_InitUserId, m_userTeamIds);
                bool? isShared = EntityDataHelp.AppendIsSharedFakeAttribute(m_entityAttrs, m_strCurrentEntity, m_entityAttrsStruct, m_currentRecordID, m_sharedRecords);

                CEUser ceUser = GetCEUser(m_userAttrs, m_userAttrsStruct);

                CEResource ceResource = GetCEResource(m_entityAttrs, m_entityAttrsStruct, m_strCurrentEntity);
                CEHost ceHost = GetCEHost();
                CEApp ceApp = GetCEApp();
                CQuery query = new CQuery();
                m_enforceResult = query.QueryJavaPCEva(m_log, Common.Constant.Policy.Action.View, m_strCurrentEntity, ceResource, ceUser, ceHost, ceApp);
                m_log.WriteLog(LogLevel.Debug, string.Format("PDP return {0} on {1} action for entity {2}", m_enforceResult.Decision, Common.Constant.Policy.Action.View, m_strCurrentEntity));
                if (m_enforceResult.Decision.Equals(CEResponse.CEAllow))
                {
                    SecureEntity metadata = m_entityAttrsStruct.Lookup(m_strCurrentEntity, x => null);
                    if (metadata == null)
                    {
                        throw new Exceptions.InvalidCacheException(m_strCurrentEntity);
                    }
                    QueryExpression totalExpression = new QueryExpression(m_strCurrentEntity);
                    totalExpression.ColumnSet = m_columnSet;
                    totalExpression.Criteria.AddCondition(metadata.Schema.PrimaryIDName, ConditionOperator.In, m_currentRecordID.ToString());
                
                    ApplySecurityFilterHandle applySecurityFilterHandle = new ApplySecurityFilterHandle(m_enforceResult.ListObligations,
                                                                                                                                                m_entityAttrsStruct,
                                                                                                                                                m_conditionFactory,
                                                                                                                                                m_strCurrentEntity,
                                                                                                                                                m_log);
                    ApplySecurityFilter ApplySecurityFilterObligationResultCollection = applySecurityFilterHandle.GetFilter(m_InitUserId,isOwner,isShared);

                    InheritFromCollection inheritFromObligationResultCollection = new InheritFromCollection();
                    InheritPoliciesFromHanlde inheritPoliciesFromHanlde = new InheritPoliciesFromHanlde(m_strCurrentEntity,
                                                                                                        m_enforceResult.ListObligations,
                                                                                                        m_entityAttrsStruct,
                                                                                                        new AttributeCollection(),
                                                                                                        m_userAttrsStruct,
                                                                                                        m_userAttrs,
                                                                                                        m_conditionFactory,
                                                                                                        m_log,
                                                                                                        m_service,
                                                                                                        m_n1RelationshipCache,
                                                                                                        m_nnRelationshipCache,
                                                                                                        m_strDefaultDenyMessage,
                                                                                                        m_InitUserId,
                                                                                                        totalExpression,
                                                                                                        m_sharedParam,
                                                                                                        m_userTeamIds,
                                                                                                        m_sharedRecords,
                                                                                                        m_userHostIP);
                    inheritPoliciesFromHanlde.GetFilter(inheritFromObligationResultCollection, m_InitUserId);
                    ApplySecurityFilterBasedonParentAttributesHandle applySecurityFilterBasedonParentAttributesHandle =
                        new ApplySecurityFilterBasedonParentAttributesHandle(m_service,
                                                                                                        m_enforceResult.ListObligations,
                                                                                                        m_entityAttrsStruct,
                                                                                                        m_conditionFactory,
                                                                                                        m_strCurrentEntity,
                                                                                                        m_n1RelationshipCache,
                                                                                                        m_nnRelationshipCache,
                                                                                                        m_InitUserId,
                                                                                                        m_userTeamIds,
                                                                                                        m_sharedRecords,
                                                                                                        m_log);

                    applySecurityFilterBasedonParentAttributesHandle.GetFilter(inheritFromObligationResultCollection);

                    m_conditionFactory.InitializeLazyCondition();

                    QueryExpression totalExpress = GetTotalFilter(totalExpression, ApplySecurityFilterObligationResultCollection, inheritFromObligationResultCollection);

                    EntityCollection totalEntityResult = Common.DataBussiness.EntityDataHelp.RetrieveMultiple(m_service, totalExpress);

                    if (totalEntityResult.Entities.Count > 0)
                    {
                        m_enforceResult.Decision = CEResponse.CEAllow;
                    }
                    else
                    {
                        m_enforceResult.Decision = CEResponse.CEDontCare;
                    }
                    m_log.WriteLog(LogLevel.Debug, string.Format("Build FetchXML and query result decision {0}", m_enforceResult.Decision));
                }
            }
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("{2} Enforce final result {0} for entity {1}", m_enforceResult.Decision, m_strCurrentEntity,this.GetType()));
            return m_enforceResult;

        }
        public void ExecuteEnforceResult()
        {
            if (m_enforceResult != null)
            {
                Common.DataModel.Share.ExcuteObligationResult shareExcuteObligationResult = new Common.DataModel.Share.ExcuteObligationResult(m_enforceResult.Decision.ToString());
                if (m_enforceResult.Decision == CEResponse.CEAllow)
                {
                    MaskFieldHandles maskFieldHandle = new MaskFieldHandles(m_enforceResult.ListObligations, m_entityAttrsStruct, m_strCurrentEntity, m_log);
                    List<Common.DataModel.Share.MaskField> lisMaskField = shareExcuteObligationResult.MaskFields;
                    if (m_columnSet.Columns.Count > 0)
                    {
                        maskFieldHandle.DoObligation(m_entityAttrs, ref m_columnSet, ref lisMaskField);
                    }
                    else
                    {
                        //this code only for mobile, it column count is 0, so we need get column from attribute
                        ColumnSet tempColumn = new ColumnSet();
                        foreach(var item in m_entityAttrs)
                        {
                            tempColumn.AddColumn(item.Key);
                        }
                        maskFieldHandle.DoObligation(m_entityAttrs, ref tempColumn, ref lisMaskField);
                    }
                }
                else
                {
                    //do deny
                    if (m_enforceResult.Decision == CEResponse.CEDeny)
                    {
                        shareExcuteObligationResult.Decision = CEResponse.CEDeny.ToString();
                        DisplayAlertMessageHandle displayMessage = new DisplayAlertMessageHandle(m_enforceResult.ListObligations, Common.Constant.MessageName.Retrieve);
                        shareExcuteObligationResult.DisplayMessages.Add(displayMessage.GetDisplayMessage());
                        FormViewClearColumn(ref m_columnSet, m_strCurrentEntity);
                    }
                    else
                    {
                        shareExcuteObligationResult.Decision = CEResponse.CEDontCare.ToString();
                        shareExcuteObligationResult.DisplayMessages.Add(m_strDefaultDenyMessage);
                        foreach (string item in m_columnSet.Columns)
                        {
                            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("Retrieve::ExecuteEnforceResult before FormViewClearColumn: {0} in columnSet.Columns ", item.ToString()));
                        }

                        FormViewClearColumn(ref m_columnSet, m_strCurrentEntity);
                        foreach (string item in m_columnSet.Columns)
                        {
                            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("Retrieve::ExecuteEnforceResult after FormViewClearColumn: {0} in columnSet.Columns ", item.ToString()));
                        }
                    }
                }
                if (shareExcuteObligationResult.MaskFields != null && shareExcuteObligationResult.MaskFields.Count > 0)
                {
                    m_sharedParam.Add(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_Key, shareExcuteObligationResult.ToString());
                }
                RecordEnforceResult(m_service, m_currentRecordID, shareExcuteObligationResult.ToString());
            }
            else
            {
                throw new Exceptions.EnforceNotExcuted();
            }
        }
        private QueryExpression GetTotalFilter(QueryExpression totalExpression,
                                                ApplySecurityFilter applySecurityFilterObligationResultColl,
                                                InheritFromCollection inheritFromCollectionObligationResultColl)
        {
            FilterExpression FilterExpressSource = totalExpression.Criteria;
            List<LinkEntity> lisLinks = null;
            FilterExpression FilterExpressInherited = null;
            inheritFromCollectionObligationResultColl.CreateEntityItems(out lisLinks, out FilterExpressInherited);
            FilterUtil.Merge(new List<FilterExpression>() { applySecurityFilterObligationResultColl.QueryFilter, FilterExpressInherited }, LogicalOperator.And, ref FilterExpressSource);
            totalExpression.Criteria = FilterExpressSource;
            totalExpression.LinkEntities.AddRange(lisLinks);
            totalExpression.LinkEntities.AddRange(applySecurityFilterObligationResultColl.QueryLinkEntites);
            return totalExpression;
        }



        private void RecordEnforceResult(IOrganizationService service,Guid currentRecordId, string strJson)
        {
            Microsoft.Xrm.Sdk.Entity notifyEntity = new Microsoft.Xrm.Sdk.Entity("nxl_nxlnotices");
            notifyEntity.Attributes.Add(new KeyValuePair<string, object>("nxl_id", currentRecordId.ToString()));
            notifyEntity.Attributes.Add(new KeyValuePair<string, object>("nxl_isread", false.ToString()));
            notifyEntity.Attributes.Add(new KeyValuePair<string, object>("nxl_message", strJson));
            notifyEntity.Attributes.Add(new KeyValuePair<string, object>("nxl_notifyuser", m_InitUserId.ToString()));
            notifyEntity.Attributes.Add(new KeyValuePair<string, object>("nxl_entity", m_strCurrentEntity));
            service.Create(notifyEntity);
        }

        private void FormViewClearColumn(ref ColumnSet colSet, string strEntityName)
        {

            colSet.AllColumns = false;
            if (strEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.OpportunityEntity, StringComparison.OrdinalIgnoreCase))
            {
                bool bprocessid = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.processid);
                bool btraversedpath = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.traversedpath);
                colSet.Columns.Clear();
                if (bprocessid)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.processid);
                }
                if (btraversedpath)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.traversedpath);
                }
            }
            else if (strEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.kbarticleEntity, StringComparison.OrdinalIgnoreCase))
            {
                bool bstatecode = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.statecode);
                bool bkbarticletemplateid = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.kbarticletemplateid);
                colSet.Columns.Clear();
                if (bstatecode)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.statecode);
                }
                if (bkbarticletemplateid)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.kbarticletemplateid);
                }
            }
            else if (strEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.productEntity, StringComparison.OrdinalIgnoreCase))
            {
                bool biskit = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.iskit);
                colSet.Columns.Clear();
                if (biskit)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.iskit);
                }
            }
            else if (strEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.Incident, StringComparison.OrdinalIgnoreCase))
            {
                bool bprocessid = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.processid);
                colSet.Columns.Clear();
                if (bprocessid)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.processid);
                }
            }
            else
            {
                bool bstatecode = colSet.Columns.Contains(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.statecode);
                colSet.Columns.Clear();
                if (bstatecode)
                {
                    colSet.AddColumn(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.statecode);
                }
            }
        }
    }
}
