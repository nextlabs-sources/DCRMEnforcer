using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.IO;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Condition
{
    public class ConditionFactory
    {
        private MemoryCache<SecureEntity> m_secureEntityCache;
        private IOrganizationService m_service = null;
        private Guid m_userId;
        private List<Guid> m_teamIds;
        private bool m_forListView; //all the conditions will be combined together with the original request expression from outside

        private Dictionary<string, List<Guid>> m_sharedRecordsDic;

        //<entityname,lazy conditions>
        List<ILazyInitialize>m_negativeOwnerConditions = new List<ILazyInitialize>();
        List<ILazyInitialize> m_positiveOwnerConditions = new List<ILazyInitialize>();
        Dictionary<string, List<ILazyInitialize>> m_negativeShareConditionDic = new Dictionary<string, List<ILazyInitialize>>();
        Dictionary<string, List<ILazyInitialize>> m_positiveShareConditionDic = new Dictionary<string, List<ILazyInitialize>>();
        public ConditionFactory(MemoryCache<SecureEntity> secureEntityCache, 
            IOrganizationService service,
            Guid userId, 
            List<Guid> teamIds,
            bool forListView = true,
            //bool? isShared = null,
            //bool? isOwner = null,
            Dictionary<string, List<Guid>> sharedRecords = null)
        {
            if(secureEntityCache == null)
            {
                throw new ArgumentNullException("Secure Entity Cache cannot be null.");
            }

            if(service == null)
            {
                throw new ArgumentNullException("service cannot be null.");
            }
            m_secureEntityCache = secureEntityCache;
            m_service = service;
            m_userId = userId;
            m_teamIds = teamIds;
            m_forListView = forListView;

            //m_isShared = isShared;
            //m_isOwner = isOwner;
            m_sharedRecordsDic = sharedRecords;

        }

        private void AddLazyCondition(string entityName, ILazyInitialize cond, Dictionary<string, List<ILazyInitialize>> dic)
        {
            if(!dic.ContainsKey(entityName))
            {
                dic[entityName] = new List<ILazyInitialize>();
            }

            dic[entityName].Add(cond);
        }

        private bool CheckPositiveCondition(ApplySecurityFilterModel obligationItem)
        {
            return ((obligationItem.Value.ToString().Equals("true",StringComparison.OrdinalIgnoreCase)
                            && obligationItem.Operator == Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal) ||
                        (obligationItem.Value.ToString().Equals("false", StringComparison.OrdinalIgnoreCase)
                            && obligationItem.Operator == Microsoft.Xrm.Sdk.Query.ConditionOperator.NotEqual));
        }
        public  Condition Create(string entityName,
            ApplySecurityFilterModel obligationItem,
            bool? isOwner,
            bool? isShared)
        {
            if(obligationItem == null)
            {
                return null;
            }

            if(string.IsNullOrWhiteSpace(obligationItem.Field))
            {
                return null;
            }

            SecureEntity metadata = m_secureEntityCache.Lookup(entityName, x => null);
            if(metadata == null)
            {
                return null;
            }

            List<Guid> sharedRecords = null;
            if (m_sharedRecordsDic != null && m_sharedRecordsDic.ContainsKey(entityName))
            {
                sharedRecords = m_sharedRecordsDic[entityName];
            }
            
            Condition cond = null;
            switch (obligationItem.Field)
            {
                case Constant.AttributeKeyName.NXL_ISOwner:
                    if(!metadata.Schema.OwneridExist)
                    {
                        break;
                    }
                    //nxl_isowner is set as allow
                    if(CheckPositiveCondition(obligationItem))
                    {
                        if (m_forListView)
                        {
                            //only in the original list view the, equaluserxxx can be used, in the request created by ourselves, 
                            //because the context is different, probably  equaluserxxx is unvaliable, 
                            //EmbedPositiveOwnerCondition is used for the original list view
                            cond = new EmbedPositiveOwnerCondition();
                        }
                        else if(isOwner == null)
                        {
                            //need set userid and userteam id later because the teamid will not be to get when isowner fake attribute unselected, 
                            //so it is a lazy condition
                            IndependPostiveOwnerCondition ownerAllowCond = new IndependPostiveOwnerCondition();
                            cond = ownerAllowCond;
                            m_positiveOwnerConditions.Add(ownerAllowCond);
                        }
                        else if(isOwner.Value)
                        {
                            //create always true condition
                            cond = new CommonCondition(Condition.CreateAlwaysSucceedQueryCondition());
                        }
                        else
                        {
                            //create always false condition
                            cond = new CommonCondition(Condition.CreateAlwaysFailedQueryCondition());
                        }
                    }
                    else
                    {
                        if (isOwner == null)
                        {
                            NegativeOwnerCondition ownerDenyCond = new NegativeOwnerCondition();
                            cond = ownerDenyCond;
                            m_negativeOwnerConditions.Add(ownerDenyCond);
                        }
                        else if(isOwner.Value)
                        {
                            //create always false condition
                            cond = new CommonCondition(Condition.CreateAlwaysFailedQueryCondition());
                        }
                        else
                        {
                            //create always true condition
                            cond = new CommonCondition(Condition.CreateAlwaysSucceedQueryCondition());
                        }
                    }
                    break;
                case Constant.AttributeKeyName.NXL_ISShared:

                    if (CheckPositiveCondition(obligationItem))
                    {
                        if (isShared == null)
                        {

                            PositiveShareCondition shareAllowCond = new PositiveShareCondition(entityName, metadata.Schema.PrimaryIDName);
                            cond = shareAllowCond;

                            if(sharedRecords == null)
                            {
                                //when the shardRecords is null, that means shared records need be retrieved from CRM
                                //cause there may be several same conditions, we make it as lazy condition and init it once later
                                AddLazyCondition(entityName, shareAllowCond, m_positiveShareConditionDic);
                            }
                            else
                            {
                                shareAllowCond.InstanceInitialize(m_userId, sharedRecords);
                            }
                        }
                        else if(isShared.Value)
                        {
                            //create always true condition
                            cond = new CommonCondition(Condition.CreateAlwaysSucceedQueryCondition());
                        }
                        else
                        {
                            //create always false condition
                            cond = new CommonCondition(Condition.CreateAlwaysFailedQueryCondition());
                        }
                    }
                    else
                    {
                        if (isShared == null)
                        {
                            NegativeShareCondition shareDenyCond = new NegativeShareCondition(entityName, metadata.Schema.PrimaryIDName);
                            cond = shareDenyCond;
                            if (sharedRecords == null)
                            {
                                AddLazyCondition(entityName, shareDenyCond, m_negativeShareConditionDic);
                            }
                            else
                            {
                                shareDenyCond.InstanceInitialize(m_userId, sharedRecords);
                            }
                        }
                        else if (isShared.Value)
                        {
                            //create always false condition
                            cond = new CommonCondition(Condition.CreateAlwaysFailedQueryCondition());
                        }
                        else
                        {
                            //create always true condition
                            cond = new CommonCondition(Condition.CreateAlwaysSucceedQueryCondition());
                        }
                    }
                    break;
                default:
                    ConditionExpression crmCondition = null;
                    if (obligationItem.Operator.Equals(ConditionOperator.Null) || obligationItem.Operator.Equals(ConditionOperator.NotNull))
                    {
                        crmCondition = new ConditionExpression(obligationItem.Field, obligationItem.Operator);
                    }
                    else
                    {
                        crmCondition = new ConditionExpression(obligationItem.Field, obligationItem.Operator, obligationItem.Value);
                    }
                    cond = new CommonCondition(crmCondition);
                    break;
            }

            return cond;
        }

        public void InitializeLazyCondition()
        {
            if((m_positiveOwnerConditions.Count > 0 || m_negativeOwnerConditions.Count > 0) &&
                m_teamIds == null)
            {
                UserTeamIDRequest request = new UserTeamIDRequest(m_service, m_userId);
                IRecordIDResponse response = request.Execute();
                m_teamIds = response.GetRecordIDs();
                if(m_teamIds == null || m_teamIds.Count == 0)
                {
                    throw new Exceptions.InvalidUserException(m_userId, "This user is not a memeber of any team.");
                }
            }

            if(m_teamIds != null)
            {
                if (!m_forListView)
                {
                    InitLazyConditionList(m_positiveOwnerConditions, m_userId, m_teamIds);
                }

                InitLazyConditionList(m_negativeOwnerConditions, m_userId, m_teamIds);
                InitShareCondition(m_userId,m_teamIds);
            }
            else
            {
                InitShareCondition(m_userId,null);
            }
        }

        private List<string> MergeEntityListForShareCondition()
        {
            if (m_negativeShareConditionDic.Count == 0 &&
                m_positiveShareConditionDic.Count == 0)
            {
                return null;
            }

            HashSet<string> entityNames = null;

            if (m_negativeShareConditionDic.Count > 0)
            {
                entityNames = new HashSet<string>(m_negativeShareConditionDic.Keys);
            }

            if (m_positiveShareConditionDic.Count > 0)
            {
                if (entityNames == null)
                {
                    entityNames = new HashSet<string>(m_positiveShareConditionDic.Keys);
                }
                else
                {
                    entityNames.UnionWith(m_positiveShareConditionDic.Keys);
                }
            }

            return entityNames == null ? null : entityNames.ToList<string>();

        }

        private void InitLazyConditionList(List<ILazyInitialize> conditions, Guid userId, List<Guid> recordIds)
        {
            foreach (ILazyInitialize cond in conditions)
            {
                cond.Initialize(userId,recordIds);
            }
        }
        private void InitShareCondition(Guid userId,List<Guid> teamids)
        {

            List<string> entityNames = MergeEntityListForShareCondition();
            if(entityNames == null)
            {
                return;
            }

            List<SecureEntity> entityMetaDatas = new List<SecureEntity>();
            foreach (string name in entityNames)
            {
                SecureEntity entityMetaData = this.m_secureEntityCache.Lookup(name, x => null);
                if (entityMetaData == null)
                {
                    continue;
                }

                entityMetaDatas.Add(entityMetaData);
            }
            SharedRecordRequest request = new SharedRecordRequest(m_service, m_secureEntityCache,m_userId, teamids);
            SharedRecordResponse response = request.Execute();

            foreach (string name in entityNames)
            {
                List<Guid> records = response.GetRecordIDs(name);
                if (m_negativeShareConditionDic.ContainsKey(name))
                {
                    InitLazyConditionList(m_negativeShareConditionDic[name], userId,records);
                }

                if (m_positiveShareConditionDic.ContainsKey(name))
                {
                    InitLazyConditionList(m_positiveShareConditionDic[name], userId, records);
                }
            }
        }
    }
}
