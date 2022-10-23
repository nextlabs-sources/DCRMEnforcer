using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.Constant;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    class LogUserInfoReader
    {
        private IOrganizationService m_service;
        private MemoryCache<SecureEntity> m_userAttributesCache;
        private List<Guid> m_teamIDs = null;

        public List<Guid> UserTeamIDs
        {
            get { return m_teamIDs; }
        }

        public LogUserInfoReader(IOrganizationService service)
        {
            m_service = service;
        }

        public void SetSecureUserEntityCache(MemoryCache<SecureEntity> secureUserAttributeCache)
        {
            m_userAttributesCache = secureUserAttributeCache;
        }

        public SystemUser RetrieveUserFromDCRMByID(string id)
        {
            SecureEntity userEntity = m_userAttributesCache.Lookup(SystemUser.EntityLogicalName, x=>null);
            if(userEntity == null)
            {
                return null;
            }

            SystemUser user = CreateSystemUserFromDCRMByID(id, userEntity);
            if(user == null)
            {
                return null;
            }

            List<Entity> teams = null;
            List<Entity> roles = null;
            if (Util.IsTeamEnbledForUserAttribute(m_userAttributesCache))
            {
                EntityDataHelp.GetTeamsForUserAttribute(m_service, new Guid(id), m_userAttributesCache, ref m_teamIDs, ref teams);
                if (teams != null && teams.Count > 0)
                {
                    EntityDataHelp.AppendExtendAttributesToUserAttributes(user.Attributes, teams);
                }
            }

            if (Util.IsRoleEnbledForUserAttribute(m_userAttributesCache))
            {
                EntityDataHelp.GetRolesForUserAttribute(m_service, new Guid(id), m_userAttributesCache, ref roles);
                if (roles != null && roles.Count > 0)
                {
                    EntityDataHelp.AppendExtendAttributesToUserAttributes(user.Attributes, roles);
                }
            }
            return user;
        }

        public SystemUser CreateSystemUserWithNameOnly(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            QueryExpression query = new QueryExpression
            {
                EntityName = SystemUser.EntityLogicalName,
                ColumnSet = new ColumnSet(AttributeKeyName.Fullname),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = Common.Constant.AttributeKeyName.SystemuserId,
                            Operator = ConditionOperator.Equal,
                            Values = { id }
                        }
                    }
                }
            };

            SystemUser user = null;
            RetrieveMultipleRequest retrieveRequest = new RetrieveMultipleRequest();
            retrieveRequest.Query = query;
            retrieveRequest.RequestId = NXLIDProvider.CreateNewNXLGUID();
            EntityCollection ec = ((RetrieveMultipleResponse)m_service.Execute(retrieveRequest)).EntityCollection;
            if (ec.Entities.Count > 0)
            {
                user = ec.Entities[0] as SystemUser;
            }

            return user;
        }
        public SystemUser CreateSystemUserFromDCRMByID(string id, SecureEntity userEntity)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            if(userEntity == null)
            {
                return null;
            }

            string[] userColumns = new string[userEntity.Schema.Attributes.Count];
            int index = 0;
            foreach(DataModel.MetaData.Attribute attr in userEntity.Schema.Attributes)
            {
                userColumns[index] = attr.LogicName;
                index++;
            }

            QueryExpression query = new QueryExpression
            {
                EntityName = SystemUser.EntityLogicalName,
                ColumnSet = new ColumnSet(userColumns),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = Common.Constant.AttributeKeyName.SystemuserId,
                            Operator = ConditionOperator.Equal,
                            Values = { id }
                        }
                    }
                }
            };

            SystemUser user = null;
            RetrieveMultipleRequest retrieveRequest = new RetrieveMultipleRequest();
            retrieveRequest.Query = query;
            retrieveRequest.RequestId = NXLIDProvider.CreateNewNXLGUID();
            EntityCollection ec = ((RetrieveMultipleResponse)m_service.Execute(retrieveRequest)).EntityCollection;
            if (ec.Entities.Count > 0)
            {
                user = ec.Entities[0] as SystemUser;
            }
          
            return user;
        }
    }
}
