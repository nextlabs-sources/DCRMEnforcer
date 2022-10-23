using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle
{
    public class InheritPoliciesFromHanlde
    {

        private List<Dictionary<string, Common.DataModel.InheritPoliciesFromModel>> m_dirObligation = null;
        private IOrganizationService m_service = null;
        private AttributeCollection m_userAttr = null;
        private MemoryCache<SecureEntity> m_entityCache = null;
        private MemoryCache<SecureEntity> m_userCache = null;
        private GeneralSetting m_generalSetting = null;
        private NextLabsCRMLogs m_log = null;
        private string m_strCurrentEntityName = null;
        private MemoryCache<N1Relationship> m_n1RelationshipCache = null;
        private MemoryCache<NNRelationship> m_nnRelationshipCache = null;
        private ConditionFactory m_conditionFactory;
        private string m_strDefaultDenyMessage;

        private Guid m_initUserID = Guid.Empty;
        private object m_objectQuery = null;
        private ParameterCollection m_sharedParam = null;
        private List<Guid> m_userTeams;
        private Dictionary<string, List<Guid>> m_sharedRecords;
        private string m_userHostIP;
        public InheritPoliciesFromHanlde(string strCurrentEntityName,
            List<CEObligation> lisObs,
            MemoryCache<SecureEntity> entityCache,
            AttributeCollection entityAttrCollenction,
            MemoryCache<SecureEntity> userCache,
            AttributeCollection userAttrCollection,
            ConditionFactory conditionFactory,
            NextLabsCRMLogs log,
            IOrganizationService service,
            MemoryCache<N1Relationship> n1RelationshipCache,
            MemoryCache<NNRelationship> nnRelationshipCache,
            string strDefaultDenyMessage,
            Guid initUserID,
            object objectQuery,
            ParameterCollection sharedParam,
            List<Guid> userTeams,
            Dictionary<string, List<Guid>> sharedRecords,
            string userHostIP)
        {
            m_service = service;
            m_userAttr = userAttrCollection;
            m_entityCache = entityCache;
            m_userCache = userCache;
            m_log = log;
            m_strCurrentEntityName = strCurrentEntityName;
            m_n1RelationshipCache = n1RelationshipCache;
            m_nnRelationshipCache = nnRelationshipCache;

            m_conditionFactory = conditionFactory;

            m_dirObligation = Common.DataModel.InheritPoliciesFromModel.GetCurrentObligation(lisObs);
            m_strDefaultDenyMessage = strDefaultDenyMessage;
            m_initUserID = initUserID;
            m_objectQuery = objectQuery;
            m_sharedParam = sharedParam;
            m_userTeams = userTeams;
            m_sharedRecords = sharedRecords;
            m_userHostIP = userHostIP;
        }


        public void GetFilter(Common.DataBussiness.Obligation.InheritFromCollection inheritFromCollection, Guid userid)
        {
            Dictionary<string, EnforceResult> dictEnforceResult = new Dictionary<string, EnforceResult>();
            List<Common.DataBussiness.Obligation.InheritFrom> lisInheritFrom = new List<Common.DataBussiness.Obligation.InheritFrom>();
            //HashSet<string> parentNames = new HashSet<string>();
            foreach (Dictionary<string, Common.DataModel.InheritPoliciesFromModel> dirPartObligation in m_dirObligation)
            {
                Common.DataBussiness.Obligation.InheritPolicyFrom partInherit = new Common.DataBussiness.Obligation.InheritPolicyFrom();
                Dictionary<string, Common.DataBussiness.Obligation.ApplySecurityFilter> dirApplySecurityFilter = new Dictionary<string, Common.DataBussiness.Obligation.ApplySecurityFilter>();
                foreach (string strKey in dirPartObligation.Keys)
                {
                    Common.DataModel.InheritPoliciesFromModel inheritPoliciesFromModel = dirPartObligation[strKey];
                    EnforceResult parentEntityEnforceResult = null;
                    if (dictEnforceResult.ContainsKey(inheritPoliciesFromModel.ParentEntityLogicName)
                        && dictEnforceResult[inheritPoliciesFromModel.ParentEntityLogicName] != null)
                    {
                        parentEntityEnforceResult = dictEnforceResult[inheritPoliciesFromModel.ParentEntityLogicName];
                    }
                    else
                    {
                        Enforces.IEnforce enforceInstance = new Enforces.RetrieveMultiple(m_service,
                                                                                                                        inheritPoliciesFromModel.ParentEntityLogicName,
                                                                                                                        m_userAttr,
                                                                                                                        m_userCache,
                                                                                                                        m_entityCache,
                                                                                                                        m_log,
                                                                                                                        m_initUserID,
                                                                                                                        m_n1RelationshipCache,
                                                                                                                        m_nnRelationshipCache,
                                                                                                                        m_objectQuery,
                                                                                                                        m_strDefaultDenyMessage,
                                                                                                                        m_sharedParam,
                                                                                                                        m_userTeams,
                                                                                                                        m_sharedRecords,
                                                                                                                        m_conditionFactory,
                                                                                                                        m_userHostIP);
                        parentEntityEnforceResult = enforceInstance.DoEnforce(false);
                        dictEnforceResult.Add(inheritPoliciesFromModel.ParentEntityLogicName, parentEntityEnforceResult);
                    }
                    ApplySecurityFilterHandle filterCreatebyEnforce = null;
                    if (parentEntityEnforceResult.Decision == CEResponse.CEAllow)
                    {
                        filterCreatebyEnforce = new ApplySecurityFilterHandle(parentEntityEnforceResult.ListObligations, m_entityCache,
                            m_conditionFactory, inheritPoliciesFromModel.ParentEntityLogicName, m_log);
                    }
                    else
                    {
                        filterCreatebyEnforce = new ApplySecurityFilterHandle(inheritPoliciesFromModel.ParentEntityLogicName, m_entityCache, m_conditionFactory, m_log);
                    }
                    if (dirApplySecurityFilter.ContainsKey(inheritPoliciesFromModel.RelationshipLogicName))
                    {
                        dirApplySecurityFilter[inheritPoliciesFromModel.RelationshipLogicName] =
                            dirApplySecurityFilter[inheritPoliciesFromModel.RelationshipLogicName].And(filterCreatebyEnforce.GetFilter(userid, null, null));
                    }
                    else
                    {
                        dirApplySecurityFilter.Add(inheritPoliciesFromModel.RelationshipLogicName, filterCreatebyEnforce.GetFilter(userid, null, null));
                    }

                    Common.DataBussiness.Obligation.Relation relation = Common.DataBussiness.Obligation.RelationFactory.CreateRelation(m_n1RelationshipCache, m_nnRelationshipCache, m_strCurrentEntityName, inheritPoliciesFromModel.RelationshipLogicName, inheritPoliciesFromModel.OrphanedChildDecision == Decision.Allow ? true : false);
                    partInherit.AddRelation(relation);
                    
                }
                foreach (KeyValuePair<string, Common.DataBussiness.Obligation.ApplySecurityFilter> item in dirApplySecurityFilter)
                {
                    partInherit.AddParentSecurityFilter(item.Key, item.Value);
                }
                lisInheritFrom.Add(partInherit);
            }
            inheritFromCollection.Add(lisInheritFrom);
        }

        private bool IsMatchObligation(Dictionary<string, Common.DataModel.InheritPoliciesFromModel> dictObligations, AttributeCollection sourceAttrs, string strMessage, EnforceResultCache resultCache)
        {
            bool bResult = true;

            foreach (KeyValuePair<string, Common.DataModel.InheritPoliciesFromModel> item in dictObligations)
            {
                if (!IsMatchPartObligation(item.Value, sourceAttrs, strMessage, resultCache))
                {
                    m_log.WriteLog(LogLevel.Debug, "Don't match obligation index " + item.Key);
                    bResult = false;
                    break;
                }
                else
                {
                    m_log.WriteLog(LogLevel.Debug, "Match obligation index " + item.Key);
                }
            }

            return bResult;
        }
        private bool IsMatchPartObligation(Common.DataModel.InheritPoliciesFromModel inheritPoliciesFromModel, AttributeCollection sourceAttrs, string strMessage, EnforceResultCache resultCache)
        {
            bool bResult = true;

            Common.DataBussiness.Obligation.Relation relation = Common.DataBussiness.Obligation.RelationFactory.CreateRelation(m_n1RelationshipCache, m_nnRelationshipCache, m_strCurrentEntityName, inheritPoliciesFromModel.RelationshipLogicName, inheritPoliciesFromModel.OrphanedChildDecision == Decision.Allow ? true : false);
            bool bAttributeExctise = sourceAttrs.Keys.ToList<string>().Exists(dir => { return dir.Equals(relation.CurrentAttribute, StringComparison.OrdinalIgnoreCase); });
            if (bAttributeExctise)
            {
                List<Guid> lisParentEntityId = EntityDataHelp.GetParentEntityIds(m_service, inheritPoliciesFromModel.ParentEntityLogicName, relation, sourceAttrs[relation.CurrentAttribute], m_log);
                if (lisParentEntityId.Count > 0)
                {
                    foreach (Guid parentEntityId in lisParentEntityId)
                    {
                        if (resultCache.GetCache(parentEntityId) == null)
                        {
                            Enforces.IEnforce enforceInstance = null;

                            if( strMessage.Equals(Common.Constant.MessageName.Update) ||
                                strMessage.Equals(Common.Constant.MessageName.Delete))
                            {
                                //init is owner fake attribute
                                if (m_userTeams == null && 
                                    Util.IsIsOwnerEnabled(inheritPoliciesFromModel.ParentEntityLogicName, m_entityCache))
                                {
                                    m_userTeams = EntityDataHelp.GetUserTeamIDs(m_service, m_initUserID);
                                    if (m_userTeams == null || m_userTeams.Count == 0)
                                    {
                                        throw new Exceptions.InvalidUserException(m_initUserID,
                                            @"NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle.InheritPoliciesFromHanlde.IsMatchPartObligation:
                                            Any user must belong to one team at least");
                                    }
                                }

                                //init is shared fake attribute
                                if (m_sharedRecords == null &&
                                    Util.IsIsSharedEnabled(inheritPoliciesFromModel.ParentEntityLogicName, m_entityCache))
                                {
                                    //will not be null but empty or filled
                                    m_sharedRecords = EntityDataHelp.GetSharedRecordIDs(m_service, m_entityCache, m_initUserID);
                                }
                            }
                            switch (strMessage)
                            {
                                case Common.Constant.MessageName.Update:
                                    enforceInstance = new Enforces.Update(m_service,
                                                                                                           inheritPoliciesFromModel.ParentEntityLogicName,
                                                                                                            m_userAttr,
                                                                                                            m_userCache,
                                                                                                            m_entityCache,
                                                                                                            m_log,
                                                                                                            m_initUserID,
                                                                                                            m_n1RelationshipCache,
                                                                                                            m_nnRelationshipCache,
                                                                                                            m_strDefaultDenyMessage,
                                                                                                             new Entity(inheritPoliciesFromModel.ParentEntityLogicName, parentEntityId),
                                                                                                            m_sharedParam,
                                                                                                            m_userTeams,
                                                                                                            m_sharedRecords,
                                                                                                            m_conditionFactory,
                                                                                                            m_userHostIP);
                                    break;
                                case Common.Constant.MessageName.Delete:
                                    enforceInstance = new Enforces.Delete(m_service,
                                                                                                          inheritPoliciesFromModel.ParentEntityLogicName,
                                                                                                           m_userAttr,
                                                                                                           m_userCache,
                                                                                                           m_entityCache,
                                                                                                           m_log,
                                                                                                           m_initUserID,
                                                                                                           m_n1RelationshipCache,
                                                                                                           m_nnRelationshipCache,
                                                                                                           m_strDefaultDenyMessage,
                                                                                                            new EntityReference(inheritPoliciesFromModel.ParentEntityLogicName, parentEntityId),
                                                                                                           m_sharedParam,
                                                                                                           m_userTeams,
                                                                                                            m_sharedRecords,
                                                                                                            m_conditionFactory,
                                                                                                            m_userHostIP);
                                    break;
                            }
                            resultCache.SetCache(parentEntityId, enforceInstance.DoEnforce(true));
                        }
                        EnforceResult currentEnforceResult = resultCache.GetCache(parentEntityId);
                        if (!currentEnforceResult.Decision.Equals(CEResponse.CEAllow))
                        {
                            m_log.WriteLog(LogLevel.Debug, string.Format("{0} enforce don't return allow", strMessage));
                            bResult = false;
                            break;
                        }
                    }
                }
                else
                {
                    m_log.WriteLog(LogLevel.Debug, string.Format("Cannot find parent attribute record with relationship {1} on {0}, check orphane chaild ", strMessage, relation.Name));
                    if (inheritPoliciesFromModel.OrphanedChildDecision.Equals(Decision.Deny))
                    {
                        bResult = false;
                    }
                }
            }
            else
            {
                m_log.WriteLog(LogLevel.Debug, string.Format("Cannot find relation attribute{0} on relationship {2} on record's attribute on {1}, check orphane chaild ", relation.CurrentAttribute, strMessage, relation.Name));
                if (inheritPoliciesFromModel.OrphanedChildDecision.Equals(Decision.Deny))
                {
                    bResult = false;
                }
            }

            return bResult;
        }
        public EnforceResult GetEnforceResult(AttributeCollection sourceAttrs, string strMessage)
        {
            m_log.WriteLog(LogLevel.Debug, string.Format("Start excute GetEnforceResult method on {0} , Obligation count {1}", this.GetType(), m_dirObligation.Count));
            EnforceResult result = new EnforceResult();
            if (m_dirObligation.Count > 0)
            {
                EnforceResultCache resultCache = new EnforceResultCache();
                result.Decision = CEResponse.CEDontCare;
                foreach (Dictionary<string, Common.DataModel.InheritPoliciesFromModel> item in m_dirObligation)
                {
                    if (IsMatchObligation(item, sourceAttrs, strMessage, resultCache))
                    {
                        m_log.WriteLog(LogLevel.Debug, "Match one Inherit from obligation , set decision Allow");
                        result.Decision = CEResponse.CEAllow;
                        break;
                    }
                }
            }
            else
            {
                result.Decision = CEResponse.CEAllow;
            }
            return result;
        }
        class EnforceResultCache
        {
            private Dictionary<Guid, EnforceResult> m_dictEnforceResultCache = null;

            public EnforceResultCache()
            {
                m_dictEnforceResultCache = new Dictionary<Guid, EnforceResult>();
            }
            public EnforceResult GetCache(Guid id)
            {
                EnforceResult result = null;
                if (m_dictEnforceResultCache.ContainsKey(id))
                {
                    result = m_dictEnforceResultCache[id];
                }
                return result;
            }
            public void SetCache(Guid id, EnforceResult er)
            {
                if (m_dictEnforceResultCache.ContainsKey(id))
                {
                    m_dictEnforceResultCache[id] = er;
                }
                else
                {
                    m_dictEnforceResultCache.Add(id, er);
                }
            }
        }
    }
}
