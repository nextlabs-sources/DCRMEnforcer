using Microsoft.Xrm.Sdk;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    public class SharedRecordResponse
    {
        private const string m_sharedRecordAttrName = "objectid";
        private const string m_sharedRecordType = "objecttypecode";
        private Dictionary<string,List<Guid>>  m_entityNameVSrecordId =  new Dictionary<string,List<Guid>>();
        public SharedRecordResponse(MemoryCache<DataModel.SecureEntity> entityMetadataCache,EntityCollection ec)
        {
            if(entityMetadataCache == null)
            {
                throw new ArgumentNullException("entity metadata cache cannot be null");
            }

            if(ec == null)
            {
                throw new ArgumentNullException("entity collection cannot be null");
            }

            InitEntityNameVSRecordIDMap(entityMetadataCache, ec);
        }

        private void InitEntityNameVSRecordIDMap(MemoryCache<DataModel.SecureEntity> entityMetadataCache, EntityCollection ec)
        {
            foreach (Entity entity in ec.Entities)
            {
                if (!entity.Attributes.Contains(m_sharedRecordAttrName) ||
                     !entity.Attributes.Contains(m_sharedRecordType))
                {
                    continue;
                }

                DataModel.SecureEntity entityMetaData =
                entityMetadataCache.Lookup(entity.GetAttributeValue<string>(m_sharedRecordType),x=>null);

                if (entityMetaData == null)
                {
                    continue;
                }

                if (!m_entityNameVSrecordId.ContainsKey(entityMetaData.Schema.LogicName))
                {
                    m_entityNameVSrecordId[entityMetaData.Schema.LogicName] = new List<Guid>();
                }

                Guid recordId = entity.GetAttributeValue<Guid>(m_sharedRecordAttrName);
                m_entityNameVSrecordId[entityMetaData.Schema.LogicName].Add(recordId);
            }
        }
        
        public List<Guid> GetRecordIDs(string entityName)
        {
            if(string.IsNullOrWhiteSpace(entityName))
            {
                return null;
            }

            if( m_entityNameVSrecordId.ContainsKey(entityName))
            {
                return m_entityNameVSrecordId[entityName];
            }

            return null;
        }

        public Dictionary<string, List<Guid>> GetRecordIDs()
        {
            return m_entityNameVSrecordId;
        }
    }
}
