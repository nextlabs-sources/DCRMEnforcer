using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Exceptions;
using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle
{
    public class ApplySecurityFilterHandle
    {
        private string m_strEntityName = null;
        private MemoryCache<SecureEntity> m_secureEntityCache = null;
        List<Dictionary<string, Common.DataModel.ApplySecurityFilterModel>> m_dirObligations = null;
        ConditionFactory m_condFactory = null;
        private NextLabsCRMLogs m_log = null;

        public ApplySecurityFilterHandle(List<CEObligation> lisObs, MemoryCache<SecureEntity> secureEntityCache, 
            ConditionFactory conditionFactory, string strEntityName,
            NextLabsCRMLogs log)
        {
            m_dirObligations = Common.DataModel.ApplySecurityFilterModel.GetCurrentObligation(lisObs, secureEntityCache, strEntityName);
            m_strEntityName = strEntityName;
            m_condFactory = conditionFactory;
            m_secureEntityCache = secureEntityCache;
            m_log = log;
        }
        public ApplySecurityFilterHandle(string strEntityName, MemoryCache<SecureEntity> secureEntityCache,  
            ConditionFactory conditionFactory, Dictionary<string, Common.DataModel.ApplySecurityFilterModel> dictApplySecurityFilterModel, NextLabsCRMLogs log)
        {
            m_dirObligations = new List<Dictionary<string, ApplySecurityFilterModel>>();
            m_dirObligations.Add(dictApplySecurityFilterModel);
            m_condFactory = conditionFactory;
            m_secureEntityCache = secureEntityCache;
            m_log = log;
        }

        public ApplySecurityFilterHandle(string strEntityName, MemoryCache<SecureEntity> secureEntityCache, 
            ConditionFactory conditionFactory,NextLabsCRMLogs log)
        {
            m_dirObligations = new List<Dictionary<string, ApplySecurityFilterModel>>();
            ApplySecurityFilterModel filterInvaild = new ApplySecurityFilterModel();
            filterInvaild.Field = "createdon";
            filterInvaild.Operator = ConditionOperator.OnOrBefore;
            filterInvaild.Value = "1971-01-01";
            Dictionary<string, ApplySecurityFilterModel> dirInvaildFilter = new Dictionary<string, ApplySecurityFilterModel>();

            dirInvaildFilter.Add("1", filterInvaild);
            m_dirObligations.Add(dirInvaildFilter);
            m_strEntityName = strEntityName;
            m_condFactory = conditionFactory;
            m_secureEntityCache = secureEntityCache;
            m_log = log;
        }
        public Common.DataBussiness.Obligation.ApplySecurityFilter GetFilter(Guid userid,bool? isOwner,bool? isShared)
        {
            Common.DataBussiness.Obligation.ApplySecurityFilter result = new Common.DataBussiness.Obligation.ApplySecurityFilter();
            result.EntityName = m_strEntityName;
            List<CFilterObligationParaError> ListFilterOblParaErr = new List<CFilterObligationParaError>();
            if (m_dirObligations.Count == 0)
            {
                ApplySecurityFilterModel filterInvaild = new ApplySecurityFilterModel();
                filterInvaild.Field = "createdon";
                filterInvaild.Operator = ConditionOperator.OnOrAfter;
                filterInvaild.Value = "1971-01-01";
                Dictionary<string, ApplySecurityFilterModel> dirInvaildFilter = new Dictionary<string, ApplySecurityFilterModel>();

                dirInvaildFilter.Add("1", filterInvaild);
                m_dirObligations.Add(dirInvaildFilter);
            }
            foreach (Dictionary<string, Common.DataModel.ApplySecurityFilterModel> dirObligation in m_dirObligations)
            {
                Common.DataBussiness.Obligation.ApplySecurityFilter singleAppFilter = new Common.DataBussiness.Obligation.ApplySecurityFilter();
                singleAppFilter.EntityName = m_strEntityName;
                foreach (KeyValuePair<string, Common.DataModel.ApplySecurityFilterModel> keyValuePairObligation in dirObligation)
                {
                    Common.DataModel.ApplySecurityFilterModel obligation = keyValuePairObligation.Value;

                    //ConditionExpression cond = new ConditionExpression(obligation.Field, obligation.Operator, obligation.Value);

                    Condition.Condition cond = m_condFactory.Create(m_strEntityName, obligation,isOwner,isShared);
                    if (cond != null)
                    {
                        singleAppFilter.AddCondition(cond);
                    }
                }
                result = result.Or(singleAppFilter);

            }
            return result;
        }

        public bool IsMatchFilter(AttributeCollection attributes)
        {
            bool bResult = false;
            return bResult;
        }
        private bool IsMatchFilter(AttributeCollection attributes, Dictionary<string, Common.DataModel.ApplySecurityFilterModel> dictApplySecurityFilterModel)
        {
            bool bResult = false;
            foreach (KeyValuePair<string, Common.DataModel.ApplySecurityFilterModel> item in dictApplySecurityFilterModel)
            {
                bool bPartMatch = false;
                Common.DataModel.ApplySecurityFilterModel asfModel = item.Value;
                foreach (KeyValuePair<string, object> attr in attributes)
                {
                    if (attr.Key.Equals(asfModel.Field, StringComparison.OrdinalIgnoreCase))
                    {
                        SecureEntity entityStruct = m_secureEntityCache.Lookup(m_strEntityName, x => null);
                        if(entityStruct == null)
                        {
                            throw new Exceptions.InvalidCacheException(m_strEntityName);
                        }
                        CEAttribute ceAttr = DataBussiness.EntityDataHelp.TransformToCEAttribute(entityStruct, m_strEntityName, attr.Key, attr.Value, m_log);
                        if(ceAttr == null)
                        {
                            continue;
                        }
                        switch (asfModel.Operator)
                        {
                            case ConditionOperator.Equal:
                                {
                                    if (ceAttr.Value.Equals(asfModel.Value))
                                    {
                                        bPartMatch = true;
                                    }
                                }
                                break;
                            case ConditionOperator.NotEqual:
                                {
                                    if (!ceAttr.Value.Equals(asfModel.Value))
                                    {
                                        bPartMatch = true;
                                    }
                                }
                                break;
                            case ConditionOperator.GreaterThan:
                                {
                                    object objFilterValue = Convert.ChangeType(asfModel.Value, asfModel.ValueType);
                                    object objAttrValue = Convert.ChangeType(asfModel.Value, asfModel.ValueType);

                                }
                                break;
                            case ConditionOperator.LessThan:
                                {
                                }
                                break;
                            case ConditionOperator.GreaterEqual:
                                {
                                }
                                break;
                            case ConditionOperator.LessEqual:
                                {
                                }
                                break;
                        }
                        break;
                    }
                }
            }

            return bResult;
        }
    }
}
