using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Common.QueryPC;
using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;
using NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;

namespace NextLabs.CRMEnforcer.Enforces
{

    class RetrieveMultiple : EnforceBase, IEnforce
    {
        private IOrganizationService m_service = null;
        private string m_strCurrentEntity = null;
        private AttributeCollection m_userAttrs = null;
        private MemoryCache<SecureEntity> m_userAttrsStruct = null;
        private MemoryCache<SecureEntity> m_entityAttrsStruct = null;
        private EnforceResult m_enforceResult = null;
        private Guid m_initUserId = Guid.Empty;
        private MemoryCache<N1Relationship> m_n1RelationshipCache = null;
        private MemoryCache<NNRelationship> m_nnRelationshipCache = null;
        private object m_objectQuery = null;
        private ParameterCollection m_sharedParam = null;

        private string m_strDefaultDenyMessage = null;
        private List<Guid> m_userTeams = null;
        private Dictionary<string, List<Guid>> m_sharedRecords = null;

        private ConditionFactory m_conditionFactory;
        public RetrieveMultiple(IOrganizationService service, 
                                            string strCurrentEntity, 
                                            AttributeCollection userAttrCollection,
                                            MemoryCache<SecureEntity> userEntityCache,
                                            MemoryCache<SecureEntity> secureEntityCache,
                                            NextLabsCRMLogs log,
                                            Guid initUserId,
                                            MemoryCache<N1Relationship> n1RelationshipCache,
                                            MemoryCache<NNRelationship> nnRelationshipCache,
                                            object objectQuery,
                                            string strDefaultDenyMessage,
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
            m_initUserId = initUserId;
            m_n1RelationshipCache = n1RelationshipCache;
            m_nnRelationshipCache = nnRelationshipCache;
            m_objectQuery = objectQuery;
            m_strDefaultDenyMessage = strDefaultDenyMessage;
            m_sharedParam = sharedParam;
            m_userTeams = userTeams;
            m_sharedRecords = sharedRecords;
            m_conditionFactory = conditionFactory;

        }

        public EnforceResult DoEnforce(bool bIgnoreInherit)
        {
            if (m_enforceResult == null)
            {
                CEUser ceUser = GetCEUser(m_userAttrs, m_userAttrsStruct);

                CEResource ceResource = GetCEResource(new AttributeCollection(), m_entityAttrsStruct, m_strCurrentEntity);
                CEHost ceHost = GetCEHost();
                CEApp ceApp = GetCEApp();
                CQuery query = new CQuery();
                m_enforceResult = query.QueryJavaPCEva(m_log, Common.Constant.Policy.Action.View, m_strCurrentEntity, ceResource, ceUser, ceHost, ceApp);
            }
            return m_enforceResult;
        }
        public void ExecuteEnforceResult()
        {
            if (m_enforceResult != null)
            {
                if (m_enforceResult.Decision == CEResponse.CEAllow)
                {
                    ApplySecurityFilterHandle applySecurityFilterHandle = 
                        new ApplySecurityFilterHandle(m_enforceResult.ListObligations, m_entityAttrsStruct, m_conditionFactory, m_strCurrentEntity, m_log);
                    ApplySecurityFilter ApplySecurityFilterObligationResultCollection = applySecurityFilterHandle.GetFilter(m_initUserId,null,null);

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
                                                                                                        m_initUserId,
                                                                                                        m_objectQuery,
                                                                                                        m_sharedParam,
                                                                                                        m_userTeams,
                                                                                                        m_sharedRecords,
                                                                                                        m_userHostIP);
                    inheritPoliciesFromHanlde.GetFilter(inheritFromObligationResultCollection, m_initUserId);
                    ApplySecurityFilterBasedonParentAttributesHandle applySecurityFilterBasedonParentAttributesHandle =
                        new ApplySecurityFilterBasedonParentAttributesHandle(m_service,
                                                                                                        m_enforceResult.ListObligations,
                                                                                                        m_entityAttrsStruct,
                                                                                                        m_conditionFactory,
                                                                                                        m_strCurrentEntity,
                                                                                                        m_n1RelationshipCache,
                                                                                                        m_nnRelationshipCache,
                                                                                                        m_initUserId,
                                                                                                        m_userTeams,
                                                                                                        m_sharedRecords,
                                                                                                        m_log);

                    applySecurityFilterBasedonParentAttributesHandle.GetFilter(inheritFromObligationResultCollection);

                    m_conditionFactory.InitializeLazyCondition();
                    ReplaceQueryParam(m_objectQuery, ApplySecurityFilterObligationResultCollection, inheritFromObligationResultCollection, m_log);
                    //Pass share param to Post Event

                    if (MaskFieldModel.IsMaskFieldExist(m_enforceResult.ListObligations))
                    {
                        m_sharedParam.Add(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_Key, m_enforceResult.ToString());
                    }

                }
                else
                {
                    ApplySecurityFilterHandle applySecurityFilterHandleInvaild = new ApplySecurityFilterHandle(m_strCurrentEntity, m_entityAttrsStruct, m_conditionFactory, m_log);
                    Common.DataBussiness.Obligation.ApplySecurityFilter ApplySecurityFilterObligationResultCollection = applySecurityFilterHandleInvaild.GetFilter(m_initUserId,null,null);

                    m_conditionFactory.InitializeLazyCondition();
                    ReplaceQueryParam(m_objectQuery,
                        ApplySecurityFilterObligationResultCollection,
                        new Common.DataBussiness.Obligation.InheritFromCollection(),
                        m_log);
                    if (m_enforceResult.Decision.Equals(CEResponse.CEDeny))
                    {
                        DisplayAlertMessageHandle displayAlertMessageHandle = new DisplayAlertMessageHandle(m_enforceResult.ListObligations, Common.Constant.MessageName.Retrievemultiple);
                        displayAlertMessageHandle.DoObligation();
                    }
                }
            }
            else
            {
                throw new Exceptions.EnforceNotExcuted();
            }
        }

        private bool ReplaceQueryParam(object paramQuery,
                                                       Common.DataBussiness.Obligation.ApplySecurityFilter applySecurityFilterObligationResultColl,
                                                       Common.DataBussiness.Obligation.InheritFromCollection inheritFromCollectionObligationResultColl,
                                                       NextLabsCRMLogs log)
        {
            bool bResult = false;
            FetchExpression fetchExpressSource = null;
            QueryExpression queryExpressSource = null;
            if (paramQuery != null)
            {
                if (paramQuery is FetchExpression)
                {
                    fetchExpressSource = paramQuery as FetchExpression;
                    filter inheritObFilter = null;
                    List<FetchLinkEntityType> lisLinks = null;
                    inheritFromCollectionObligationResultColl.CreateEntityItems(out lisLinks, out inheritObFilter);
                    string strSourceFetchXml = fetchExpressSource.Query;
                    log.WriteLog(LogLevel.Information, string.Format("Fetch XML expression before updated is : {0}", fetchExpressSource.Query));

                    IStringSerialize xmlSericaliehelp = new XMLSerializeHelper();

                    FetchType fetchInstance = xmlSericaliehelp.Deserialize(typeof(FetchType), strSourceFetchXml) as FetchType;
                    int fetchEntityIndex = FilterUtil.FindTopEntityIndex(fetchInstance);
                    FetchEntityType fetchEntity = (FetchEntityType)fetchInstance.Items[fetchEntityIndex];
                    int fetchFilterIndex = FilterUtil.FindTopFilterIndex(fetchEntity);
                    if (fetchFilterIndex > -1)
                    {
                        filter fetchTopFilter = (filter)fetchEntity.Items[fetchFilterIndex];

                        List<filter> allObFilters = new List<filter>();
                        allObFilters.Add(applySecurityFilterObligationResultColl.FetchFilter);
                        allObFilters.Add(inheritObFilter);
                        FilterUtil.Merge(allObFilters, filterType.and, ref fetchTopFilter);
                        fetchEntity.Items[fetchFilterIndex] = fetchTopFilter;
                    }
                    else
                    {
                        filter topFilter = new filter();
                        topFilter.type = filterType.and;
                        topFilter.Items = new List<object>();
                        topFilter.Items.Add(applySecurityFilterObligationResultColl.FetchFilter);
                        topFilter.Items.Add(inheritObFilter);
                        fetchEntity.Items.Add(topFilter);
                    }
                    fetchEntity.Items.AddRange(lisLinks);
                    fetchEntity.Items.AddRange(applySecurityFilterObligationResultColl.FetchLinkEntites);
					//for fix an issue, it make product cannot publish(pop errro don't targgitr plug-in) and view
                    //fetchInstance.distinct = true;
                    fetchExpressSource.Query = xmlSericaliehelp.SerializeWithoutNamespace(typeof(FetchType), fetchInstance);          
                    bResult = true;
                    log.WriteLog(LogLevel.Information, string.Format("Fetch XML expression is updated as : {0}", fetchExpressSource.Query));
                }
                else if (paramQuery is QueryExpression)
                {
                    queryExpressSource = paramQuery as QueryExpression;
                    FilterExpression FilterExpressSource = queryExpressSource.Criteria;
                    List<LinkEntity> lisLinks = null;
                    FilterExpression FilterExpressInherited = null;
                    inheritFromCollectionObligationResultColl.CreateEntityItems(out lisLinks, out FilterExpressInherited);
                    FilterUtil.Merge(new List<FilterExpression>() { applySecurityFilterObligationResultColl.QueryFilter, FilterExpressInherited }, LogicalOperator.And, ref FilterExpressSource);

                    for (int index = FilterExpressSource.Filters.Count - 1; index >= 0; index--)
                    {
                        FilterUtil.CompressQueryFilter(FilterExpressSource, FilterExpressSource.Filters[index]);
                    }

                    queryExpressSource.Criteria = FilterExpressSource;

                    queryExpressSource.LinkEntities.AddRange(lisLinks);
                    queryExpressSource.LinkEntities.AddRange(applySecurityFilterObligationResultColl.QueryLinkEntites);
					//for fix an issue, it make product cannot publish(pop errro don't targgitr plug-in) and view
                    //queryExpressSource.Distinct = true;
#if DEBUG
                    //FetchExpression fe = Util.QueryExpressionToFetchExpression(m_service, queryExpressSource);
                    //log.WriteLog(LogLevel.Information, string.Format("Query XML expression is updated as : {0}", fe.Query));
#endif
                    bResult = true;
                }
            }
            return bResult;
        }
    }
}
