using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;
using Microsoft.Xrm.Sdk;
using NextLabs.CRMEnforcer.Common.Enums;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle
{
    class ApplySecurityFilterBasedonParentAttributesHandle
    {

        List<Dictionary<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel>> m_lisGetedObligation = null;
        private MemoryCache<N1Relationship> m_n1RelationshipCache = null;
        private MemoryCache<NNRelationship> m_nnRelationshipCache = null;
        private MemoryCache<SecureEntity> m_secureEntityCache = null;
        private IOrganizationService m_service = null;
        private string m_strCurrentEntityName = null;
        private Log.NextLabsCRMLogs m_log = null;

        private ConditionFactory m_conditionFactory;

        private Dictionary<string, List<Guid>> m_sharedRecords;
        private List<Guid> m_userTeams;
        private Guid m_userID;

        public ApplySecurityFilterBasedonParentAttributesHandle(IOrganizationService service, List<CEObligation> lisObligations,
            MemoryCache<SecureEntity> secureEntityCache, ConditionFactory conditionFactory,
            string strCurrentEntityName, MemoryCache<N1Relationship> n1RelationshipCache,
            MemoryCache<NNRelationship> nnRelationshipCache,
            Guid userID,
            List<Guid> userTeams,
            Dictionary<string, List<Guid>> sharedRecords,
            Log.NextLabsCRMLogs log)
        {
            m_secureEntityCache = secureEntityCache;
            m_lisGetedObligation = Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel.GetCurrentObligation(lisObligations);
            Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel.CheckAndVefiyObligation(m_lisGetedObligation, secureEntityCache, strCurrentEntityName);
            m_strCurrentEntityName = strCurrentEntityName;
            m_n1RelationshipCache = n1RelationshipCache;
            m_nnRelationshipCache = nnRelationshipCache;
            m_conditionFactory = conditionFactory;
            m_service = service;
            m_userTeams = userTeams;
            m_sharedRecords = sharedRecords;
            m_userID = userID;
            m_log = log;
        }

        public void GetFilter(Common.DataBussiness.Obligation.InheritFromCollection inheritFromCollection)
        {
            List<Common.DataBussiness.Obligation.InheritFrom> lisInheritpart = new List<InheritFrom>();
            foreach (Dictionary<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> Obligations in m_lisGetedObligation)
            {
                Common.DataBussiness.Obligation.InheritAttributeFrom inheritAttributeFromPart = new Common.DataBussiness.Obligation.InheritAttributeFrom();
                Dictionary<string, Common.DataBussiness.Obligation.ApplySecurityFilter> dirApplySecurityFilter = new Dictionary<string, Common.DataBussiness.Obligation.ApplySecurityFilter>();
                foreach (KeyValuePair<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> keyValuePair in Obligations)
                {
                    Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel obligation = keyValuePair.Value;
                    Common.DataBussiness.Obligation.ApplySecurityFilter applySecurityFilterPart = new Common.DataBussiness.Obligation.ApplySecurityFilter();

                    applySecurityFilterPart.EntityName = obligation.ParentEntityName;
                    //ConditionExpression conditionPart = new ConditionExpression();
                    //conditionPart.AttributeName = obligation.Filter.Field;
                    //conditionPart.Values.Add(obligation.Filter.Value);
                    //conditionPart.Operator = obligation.Filter.Operator;

                    Condition.Condition condition = m_conditionFactory.Create(obligation.ParentEntityName, obligation.Filter, null, null);
                    applySecurityFilterPart.AddCondition(condition);
                    if (dirApplySecurityFilter.ContainsKey(obligation.RelationshipName))
                    {
                        dirApplySecurityFilter[obligation.RelationshipName] = dirApplySecurityFilter[obligation.RelationshipName].And(applySecurityFilterPart);
                    }
                    else
                    {
                        dirApplySecurityFilter.Add(obligation.RelationshipName, applySecurityFilterPart);
                    }
                    Common.DataBussiness.Obligation.Relation relation = Common.DataBussiness.Obligation.RelationFactory.CreateRelation(m_n1RelationshipCache, m_nnRelationshipCache, m_strCurrentEntityName, obligation.RelationshipName, obligation.OrphanedChildRecords == Common.Enums.Decision.Allow ? true : false);

                    inheritAttributeFromPart.AddRelation(relation);
                }
                foreach (KeyValuePair<string, Common.DataBussiness.Obligation.ApplySecurityFilter> item in dirApplySecurityFilter)
                {
                    inheritAttributeFromPart.AddParentSecurityFilter(item.Key, item.Value);
                }
                lisInheritpart.Add(inheritAttributeFromPart);
            }
            inheritFromCollection.Add(lisInheritpart);
        }



        private bool IsMatchObligation(Dictionary<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> dictObligations, 
            AttributeCollection sourceAttrs, 
            RecordAttrCache recordAttrCache)
        {
            bool bResult = true;

            foreach (KeyValuePair<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> item in dictObligations)
            {
                if (!IsMatchPartObligation(item.Value, sourceAttrs, recordAttrCache))
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

        private bool IsMatchPartObligation(
            Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel applySecurityFilterBasedonParentAttributesModel, 
            AttributeCollection sourceAttrs, 
            RecordAttrCache recordAttrCache)
        {
            bool bResult = true;
            Common.DataBussiness.Obligation.Relation relation = Common.DataBussiness.Obligation.RelationFactory.CreateRelation(m_n1RelationshipCache,m_nnRelationshipCache ,m_strCurrentEntityName, applySecurityFilterBasedonParentAttributesModel.RelationshipName, applySecurityFilterBasedonParentAttributesModel.OrphanedChildRecords == Decision.Allow ? true : false);
            bool bAttributeExctise = sourceAttrs.Keys.ToList<string>().Exists(dir => { return dir.Equals(relation.CurrentAttribute, StringComparison.OrdinalIgnoreCase); });
            if (bAttributeExctise)
            {
                List<Guid> lisParentEntityId = EntityDataHelp.GetParentEntityIds(m_service, applySecurityFilterBasedonParentAttributesModel.ParentEntityName, relation, sourceAttrs[relation.CurrentAttribute], m_log);
                if (lisParentEntityId.Count > 0)
                {
                    foreach (Guid parentEntityId in lisParentEntityId)
                    {
                        AttributeCollection parentEntityAttr = recordAttrCache.GetCache(parentEntityId);
                        if (parentEntityAttr == null)
                        {
                            parentEntityAttr = EntityDataHelp.RertieveSingle(m_service, applySecurityFilterBasedonParentAttributesModel.ParentEntityName, parentEntityId, EntityDataHelp.GetEntityColumn(applySecurityFilterBasedonParentAttributesModel.ParentEntityName, m_secureEntityCache));

                            //init is owner fake attribute
                            if (m_userTeams == null && 
                                Util.IsIsOwnerEnabled(applySecurityFilterBasedonParentAttributesModel.ParentEntityName, m_secureEntityCache))
                            {
                                m_userTeams = EntityDataHelp.GetUserTeamIDs(m_service, m_userID);
                                if (m_userTeams == null || m_userTeams.Count == 0)
                                {
                                    throw new Exceptions.InvalidUserException(m_userID,
                                        @"NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle.ApplySecurityFilterBasedonParentAttributesHandle.IsMatchPartObligation:
                                        Any user must belong to one team at least");
                                }
                            }

                            EntityDataHelp.AppendIsOwnerFakeAttribute(parentEntityAttr,
                                applySecurityFilterBasedonParentAttributesModel.ParentEntityName,
                                m_secureEntityCache, m_userID, m_userTeams);

                            //init is shared fake attribute
                            if (m_sharedRecords == null &&
                                Util.IsIsSharedEnabled(applySecurityFilterBasedonParentAttributesModel.ParentEntityName, m_secureEntityCache))
                            {
                                //will not be null but empty or filled
                                m_sharedRecords = EntityDataHelp.GetSharedRecordIDs(m_service, m_secureEntityCache, m_userID);
                            }
                            
                            EntityDataHelp.AppendIsSharedFakeAttribute(parentEntityAttr,
                               applySecurityFilterBasedonParentAttributesModel.ParentEntityName,
                               m_secureEntityCache, parentEntityId, m_sharedRecords);

                            recordAttrCache.SetCache(parentEntityId, parentEntityAttr);
                        }
                        object objAttributeValue = null;
                        foreach (KeyValuePair<string, object> item in parentEntityAttr)
                        {
                            if (item.Key.Equals(applySecurityFilterBasedonParentAttributesModel.Filter.Field, StringComparison.OrdinalIgnoreCase))
                            {
                                objAttributeValue = item.Value;
                                break;
                            }
                        }
                        bool bPartMatch = applySecurityFilterBasedonParentAttributesModel.Filter.IsMatch(applySecurityFilterBasedonParentAttributesModel.Filter.Field, EntityDataHelp.GetValueOfValue(objAttributeValue), m_log);
                        m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Compare Security Filter Field {0} Result {1}", applySecurityFilterBasedonParentAttributesModel.Filter.Field, bPartMatch));
                        if (!bPartMatch)
                        {
                            bResult = false;
                            break;
                        }
                    }
                }
                else
                {
                    m_log.WriteLog(LogLevel.Debug, string.Format("Cannot find parent attribute record with relationship {0}, check orphane chaild ", relation.Name));
                    if (applySecurityFilterBasedonParentAttributesModel.OrphanedChildRecords.Equals(Decision.Deny))
                    {
                        bResult = false;
                    }
                }
            }
            else
            {
                m_log.WriteLog(LogLevel.Debug, string.Format("Cannot find relation attribute{0} on relationship {1} on record's attribute, check orphane chaild ", relation.CurrentAttribute, relation.Name));
                if (applySecurityFilterBasedonParentAttributesModel.OrphanedChildRecords.Equals(Decision.Deny))
                {
                    bResult = false;
                }
            }

            return bResult;
        }

        public EnforceResult GetEnforceResult(AttributeCollection sourceAttrs)
        {
            m_log.WriteLog(LogLevel.Debug, string.Format("Start excute GetEnforceResult method on {0} , Obligation count {1}", this.GetType(), m_lisGetedObligation.Count));
            EnforceResult result = new EnforceResult();
            if (m_lisGetedObligation.Count > 0)
            {
                RecordAttrCache recordAttrCache = new RecordAttrCache();
                result.Decision = CEResponse.CEDontCare;
                foreach (Dictionary<string, Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel> item in m_lisGetedObligation)
                {
                    if (IsMatchObligation(item, sourceAttrs, recordAttrCache))
                    {
                        m_log.WriteLog(LogLevel.Debug, "Match one Apply Security Filter Base On Parent Attribute obligation , set decision Allow");
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
        class RecordAttrCache
        {
            private Dictionary<Guid, AttributeCollection> m_dictRecordAttrCache = null;

            public RecordAttrCache()
            {
                m_dictRecordAttrCache = new Dictionary<Guid, AttributeCollection>();
            }
            public AttributeCollection GetCache(Guid id)
            {
                AttributeCollection result = null;
                if (m_dictRecordAttrCache.ContainsKey(id))
                {
                    result = m_dictRecordAttrCache[id];
                }
                return result;
            }
            public void SetCache(Guid id, AttributeCollection attr)
            {
                if (m_dictRecordAttrCache.ContainsKey(id))
                {
                    m_dictRecordAttrCache[id] = attr;
                }
                else
                {
                    m_dictRecordAttrCache.Add(id, attr);
                }
            }
        }
    }

}
