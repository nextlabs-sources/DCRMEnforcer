using Microsoft.Xrm.Sdk;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NextLabs.CRMEnforcer.Enforces
{
    interface IEnforce
    {
        EnforceResult DoEnforce(bool bIgnoreInherit);
        void ExecuteEnforceResult();
    }
    public class EnforceBase
    {
        //protected IOrganizationService m_service = null;
        //public IOrganizationService Service { set { m_service = value; } }
        protected NextLabsCRMLogs m_log = null;
        protected string m_userHostIP = string.Empty;
        //public NextLabsCRMLogs Log { set { m_log = value; } }
        //protected string m_strCurrentEntityName = null;
        //public string CurrentEntityName { set { m_strCurrentEntityName = value; } }
        //protected AttributeCollection m_userAttrs = null;
        //public AttributeCollection UserAttributes { set { m_userAttrs = value; } }
        //protected MemoryCache<SecureEntity> m_userEntityCache = null;
        //public MemoryCache<SecureEntity> UserStructCache { set { m_userEntityCache = value; } }
        //protected MemoryCache<SecureEntity> m_entityEntityCache = null;
        //public MemoryCache<SecureEntity> EntityStructCache { set { m_entityEntityCache = value; } }
        public EnforceBase(NextLabsCRMLogs log, string userHostIP)
        {
            m_log = log;
            m_userHostIP = userHostIP;
        }
        private CEAttres TransformToCEAttrs(AttributeCollection attrsCollection, SecureEntity entityStruct,string strEntityName)
        {
            CEAttres resultCEArrtes = new CEAttres();
            foreach (KeyValuePair<string, object> item in attrsCollection)
            {
                if (item.Value != null&&!string.IsNullOrEmpty(item.Key))
                {
                    CEAttribute ceAttr = Common.DataBussiness.EntityDataHelp.TransformToCEAttribute(entityStruct, strEntityName, item.Key, item.Value, m_log);
                    if(ceAttr == null)
                    {
                        continue;
                    }
                    resultCEArrtes.Add_Attre(ceAttr);
                }
                else
                {
                    m_log.WriteLog(Common.Enums.LogLevel.Warning, string.Format("Cannot transform to CEAttr, Entity {0} attribute key {1}", strEntityName, item.Key));
                }
            }
            return resultCEArrtes;
        }
        protected CEResource GetCEResource(AttributeCollection attrsCollection, MemoryCache<SecureEntity> entityCache, string strEntityName)
        {
            SecureEntity entityStruct = entityCache.Lookup(strEntityName, x => null);
            if (entityStruct == null)
            {
                throw new Exceptions.InvalidCacheException(strEntityName);
            }
            CEResource result = new CEResource();
            result.SourceName = "dcrm_"+ strEntityName.ToLower();
            result.SourceType = "dcrm_" + strEntityName.ToLower();
            CEAttres ceAttrGeted = TransformToCEAttrs(attrsCollection, entityStruct, strEntityName);
            ceAttrGeted.Add_Attre("crm_object", strEntityName.ToLower(), CEAttributeType.XACML_String);
            result.Attres = ceAttrGeted;
            return result;
        }

        private void AppendPrefixToAttributeMetaDataLogicName(string prefix, SecureEntity metadata)
        {
            if(metadata == null || metadata.Schema == null || metadata.Schema.Attributes == null)
            {
                return;
            }

            foreach(Common.DataModel.MetaData.Attribute attr in metadata.Schema.Attributes)
            {
                if(attr.LogicName != null)
                {
                    attr.LogicName = prefix + attr.LogicName;
                }
            }
        }
        protected CEUser GetCEUser(AttributeCollection attrsCollection, MemoryCache<SecureEntity> entityCache)//, string strEntityName= SystemUser.EntityLogicalName)
        {
            CEUser result = new CEUser();
            List<string> entityNames = entityCache.GetKeys();

            SecureEntity systemUserMetadata = entityCache.Lookup(SystemUser.EntityLogicalName,x=>null);
            if(systemUserMetadata == null)
            {
                throw new Exceptions.InvalidCacheException(SystemUser.EntityLogicalName);
            }

            SecureEntity cloned = systemUserMetadata.Clone();
            foreach (SecureEntity entityStruct in entityCache.GetValues())
            {
                if (!entityStruct.Schema.LogicName.Equals(SystemUser.EntityLogicalName, StringComparison.OrdinalIgnoreCase))
                {
                    SecureEntity auxEntityMetaData = entityStruct.Clone();
                    AppendPrefixToAttributeMetaDataLogicName(entityStruct.Schema.LogicName + "-", auxEntityMetaData);
                    cloned.MergeAttributes(auxEntityMetaData);
                }
            }
            result.Attres = TransformToCEAttrs(attrsCollection, cloned, SystemUser.EntityLogicalName);
              
            bool bfind = false;
            for(int i=0;i< result.Attres.get_count();i++)
            {
                string strKey = string.Empty;
                string strValue = string.Empty;
                CEAttributeType cetype;
                result.Attres.Get_Attre(i, out strKey, out strValue,out cetype);
                if (strKey.Equals(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.InternalEmailaddress, StringComparison.OrdinalIgnoreCase)&&!string.IsNullOrEmpty(strValue))
                {
                    result.Sid = strValue;
                    result.Name = strValue;
                    bfind = true;
                    break;
                }
                else if (strKey.Equals(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.DomainName, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(strValue))
                {
                    result.Sid = strValue;
                    result.Name = strValue;
                    bfind = true;
                    break;
                }
                else  if (strKey.Equals(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.Fullname, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(strValue))
                {
                    result.Sid = strValue;
                    result.Name = strValue;
                    bfind = true;
                    break;
                }
            }
            if(!bfind)
            {
                result.Sid = NextLabs.CRMEnforcer.Common.Constant.Policy.UserUnknown;
                result.Name = NextLabs.CRMEnforcer.Common.Constant.Policy.UserUnknown;
            }
            return result;
        }
        protected CEHost GetCEHost()
        {
            m_log.WriteLog(Common.Enums.LogLevel.Debug, string.Format("GetCEHost IP {0}", m_userHostIP));
            CEHost result = new CEHost(m_userHostIP, m_userHostIP, null);
            return result;
        }
        protected CEApp GetCEApp()
        {
            string strPath = "EMDCRM";
            string strName = "EMDCRM";

            CEApp result = new CEApp(strName, strPath, null, null);
            return result;
        }
    }
}
