using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using NextLabs.CRMEnforcer.Common.DataBussiness.IO;
using NextLabs.CRMEnforcer.Common.DataModel;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Cache
{
    public enum CacheType
    {
        SecureEntity,
        GeneralSettings,
        SecureUserAttribute,
        LogLevel,
        N1Relationship,
        NNRelationship,
        LoginUserInfo,
        LoginUserTeamID,
    }

    public enum RefreshReason
    {
        Init,
        Timeout,
        StillWait
    }
    static class CacheTier
    {
        private static DateTime m_dtLastUpdate = DateTime.Now;
        private static int m_defaultRefreshInterval = 1 * 60; //second
        private static RegularCache<DataModel.SecureEntity> m_secureEntityCache = new RegularCache<DataModel.SecureEntity>(new XMLSerializeHelper());
        private static RegularCache<DataModel.GeneralSetting> m_generalSettingCache = new RegularCache<DataModel.GeneralSetting>(new XMLSerializeHelper());
        private static UserAttributeCache m_secureUserAttribtesCache = new UserAttributeCache(new XMLSerializeHelper());
        private static RegularCache<DataModel.N1Relationship> m_n1Cache = new RegularCache<DataModel.N1Relationship>(new XMLSerializeHelper());
        private static RegularCache<DataModel.NNRelationship> m_nnCache = new RegularCache<DataModel.NNRelationship>(new XMLSerializeHelper());
        private static RegularCache<DataModel.LogSettings> m_logLevelCache = new RegularCache<DataModel.LogSettings>(new XMLSerializeHelper());

        private static MemoryCache<SystemUser> m_loginUserInformation = new MemoryCache<SystemUser>();
        private static MemoryCache<List<Guid>> m_loginUserTeamIDs = new MemoryCache<List<Guid>>();

        private static Dictionary<DataModel.RecordType, IStringUpdatable> m_regularCaches; 
        private static Dictionary<CacheType, object> m_caches;

        private static System.Threading.ReaderWriterLock m_rwLock = new System.Threading.ReaderWriterLock();
        private static bool bInitialized = false;

        static CacheTier()
        {
            m_secureUserAttribtesCache.AddOrUpdate(SystemUser.EntityLogicalName, CreateDefaultUserMetaDataEnforced());

            m_regularCaches = new Dictionary<DataModel.RecordType, IStringUpdatable>()
            {
                {DataModel.RecordType.Entity,m_secureEntityCache },
                {DataModel.RecordType.GeneralSetting, m_generalSettingCache },
                {DataModel.RecordType.UserAttribute, m_secureUserAttribtesCache },
                {DataModel.RecordType.LogLevel, m_logLevelCache },
                {DataModel.RecordType.N1Relationship, m_n1Cache},
                {DataModel.RecordType.NNRelationship, m_nnCache}
            };

            m_caches = new Dictionary<CacheType, object>()
            {
                {CacheType.SecureEntity,m_secureEntityCache },
                {CacheType.GeneralSettings, m_generalSettingCache },
                {CacheType.SecureUserAttribute, m_secureUserAttribtesCache },
                {CacheType.LogLevel, m_logLevelCache },
                {CacheType.N1Relationship, m_n1Cache},
                {CacheType.NNRelationship, m_nnCache},
                {CacheType.LoginUserInfo, m_loginUserInformation},
                {CacheType.LoginUserTeamID,m_loginUserTeamIDs }
            };
        }

        static public DataModel.SecureEntity CreateDefaultUserMetaDataEnforced()
        {
            DataModel.SecureEntity userMetaData = new DataModel.SecureEntity();
            userMetaData.Secured = true;
            userMetaData.Schema = new DataModel.MetaData.Entity();
            userMetaData.Schema.Attributes = new List<DataModel.MetaData.Attribute>();
            userMetaData.Schema.PrimaryIDName = "systemuserid";
            userMetaData.Schema.LogicName = Common.Constant.EntityName.Systemuser;

            DataModel.MetaData.Attribute domainName = new DataModel.MetaData.Attribute();
            domainName.LogicName = Common.Constant.AttributeKeyName.DomainName;
            domainName.DisplayName = "User Name";
            domainName.DataType = "String";
            userMetaData.Schema.Attributes.Add(domainName);

            DataModel.MetaData.Attribute email = new DataModel.MetaData.Attribute();
            email.LogicName = Common.Constant.AttributeKeyName.InternalEmailaddress;
            email.DisplayName = "Primary Email";
            email.DataType = "String";
            userMetaData.Schema.Attributes.Add(email);

            DataModel.MetaData.Attribute name = new DataModel.MetaData.Attribute();
            name.LogicName = Common.Constant.AttributeKeyName.Fullname;
            name.DisplayName = "Full Name";
            name.DataType = "String";
            userMetaData.Schema.Attributes.Add(name);

            return userMetaData;
        }
        /// <summary>
        /// cause the cache in dic is fixed, need not lock
        /// </summary>
        /// <param name="cacheType"></param>
        /// <returns></returns>
        static public object LookupCache(CacheType cacheType)
        {
            object cache = null;
            if (!m_caches.TryGetValue(cacheType, out cache))
            {
                return null;
            }

            return cache;
        }

        static public RefreshReason NeedRefresh()
        {
            DataModel.GeneralSetting generalSetting = m_generalSettingCache.Lookup(UniqueRecordName.GeneralSettingsName, x=>null);

            try
            {
                m_rwLock.AcquireReaderLock(Int32.MaxValue);

                if (!bInitialized)
                {
                    return RefreshReason.Init;
                }

                int interval = m_defaultRefreshInterval;
                if (generalSetting != null)
                {
                    interval = generalSetting.CacheRefreshInterval*60;
                }
                int intervalEscaped = (int)DateTime.Now.Subtract(m_dtLastUpdate).TotalSeconds;
                return (intervalEscaped >= interval) ? RefreshReason.Timeout : RefreshReason.StillWait;
            }
            finally
            {
                if (m_rwLock.IsReaderLockHeld)
                {
                    m_rwLock.ReleaseReaderLock();
                }
            }
        }

        static public void RefreshCache(IUserSettingsReader reader)
        {
            if(reader == null)
            {
                return;
            }

            List<DataModel.UserSetting> settings = reader.ReadAll();

            if (settings == null)
            {
                return;
            }
            foreach (DataModel.UserSetting setting in settings)
            {
                IStringUpdatable cache = null;
                if (m_regularCaches.TryGetValue(setting.DataType, out cache))
                {
                    cache.Update(setting.Name, setting.Content);
                } 
            }

            //login user cache expired with the refresh interval
            m_loginUserInformation.Clear();
            m_loginUserTeamIDs.Clear();

            GeneralSetting generalSetting =
                m_generalSettingCache.Lookup(UniqueRecordName.GeneralSettingsName, x=>null);

            if (generalSetting == null) {
                return;
            }

            GeneralSetting.FillDefaultValueIfNotAssigned(ref generalSetting);

            JPCWrapper.Update(generalSetting);

            try
            {
                m_rwLock.AcquireWriterLock(Int32.MaxValue);
                m_dtLastUpdate = DateTime.Now;
                bInitialized = true;
            }
            finally
            {
                if (m_rwLock.IsWriterLockHeld)
                {
                    m_rwLock.ReleaseWriterLock();
                }
            }
        }

        static public RefreshReason CheckAndRefreshCache(IUserSettingsReader reader)
        {
            RefreshReason reason = NeedRefresh();
            if (reason == RefreshReason.Init || reason == RefreshReason.Timeout)
            {
                RefreshCache(reader);
            }
            return reason;
        }
    }
}
