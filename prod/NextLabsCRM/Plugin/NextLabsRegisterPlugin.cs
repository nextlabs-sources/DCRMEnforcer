using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK;
using System;
using System.Xml;
using System.Text;  //for login cloudAz web consle
using System.Net;   //for login cloudAz web consle
using System.Net.Http;   //for login cloudAz web consle
using NextLabs.CRMEnforcer.Common;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using NextLabs.CRMEnforcer.Common.DataBussiness.IO;
using NextLabs.CRMEnforcer.Common.DataBussiness.LogonCC;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Log;
using System.Xml.Schema;
using System.Collections.Generic;
using NextLabs.CRMEnforcer.Common.Constant;


namespace NextLabs.CRMEnforcer.Plugin
{
    public class SelfHandler : IPlugin
    {
        private static IStringSerialize m_xmlSerializer = new XMLSerializeHelper();

        private static NextLabs.JavaPC.RestAPISDK.JavaPC m_javaPc = null;
        public static NextLabs.JavaPC.RestAPISDK.JavaPC NLJavaPC
        {
            get { return m_javaPc; }
        }

        private void FillSecureEntityWithMetaDataInfo(Common.DataModel.SecureEntity securedEntity, Common.DataModel.MetaData.Entity entityMetaData)
        {
            securedEntity.Schema.DisplayName = entityMetaData.DisplayName;
            securedEntity.Schema.PluralName = entityMetaData.PluralName;

            int count = securedEntity.Schema.Attributes.Count;

            for (int index = 0; index < count; index++)
            {
                bool bFound = false;
                foreach (Common.DataModel.MetaData.Attribute attrMetaData in entityMetaData.Attributes)
                {
                    if (securedEntity.Schema.Attributes[index].LogicName.Equals(attrMetaData.LogicName))
                    {
                        securedEntity.Schema.Attributes[index] = attrMetaData;
                        bFound = true;
                        break;
                    }
                }
                if (!bFound)
                {
                    securedEntity.Schema.Attributes[index] = null;
                }
            }
            int removeCount = securedEntity.Schema.Attributes.RemoveAll(x => x == null);
        }

        private string UpdateSecureEntityXML(string xml, IOrganizationService service)
        {
            Common.DataModel.SecureEntity securedEntity =
                (Common.DataModel.SecureEntity)m_xmlSerializer.Deserialize(typeof(Common.DataModel.SecureEntity), xml);

            EntityMetaDataReader entityMetaDataReader = new EntityMetaDataReader(service);
            Common.DataModel.MetaData.Entity entityMetaData =
                entityMetaDataReader.Read(securedEntity.Schema.LogicName);

            FillSecureEntityWithMetaDataInfo(securedEntity, entityMetaData);

            string xmlUpdated = (string)m_xmlSerializer.Serialize(typeof(Common.DataModel.SecureEntity), securedEntity);
            return xmlUpdated;
        }

        private void RegisterEntity(string logicName, bool secured, IOrganizationService service, NextLabsCRMLogs log)
        {
            if (!secured)
            {
                UnregisterOnLoadHookMessage(logicName, service, log);
            }
            else
            {
                RegisterOnLoadHookMessage(logicName, service, log);
            }
            Dictionary<string, List<string>> dictMessageEventMap = GetMessageEventMap(logicName);
            foreach (string strMessage in dictMessageEventMap.Keys)
            {
                foreach (string strEventName in dictMessageEventMap[strMessage])
                {
                    RegisterEntity(logicName, strMessage, strEventName, secured, service, log);
                }
            }
        }

        private void RegisterEntity(
            string logicName, 
            string message,
            string stage,
            bool secured, 
            IOrganizationService service, 
            NextLabsCRMLogs log)
        {

            string strStepName = string.Format(Common.Constant.DsiplayMessage.PlugInRegisterStepNameFormat, message, logicName, stage);
            Guid sdkMessageId = GetSdkMessageId(message, service);
            Guid sdkMessageFilterId = GetSdkMessageFilterId(logicName, sdkMessageId, service);
            if (sdkMessageFilterId != Guid.Empty)
            {
                // if no , unstall 
                UnregiterSdkMessageProcessingStep(strStepName, service, log);
                if (!secured)
                {
                    return;
                }

                SdkMessageProcessingStep registerStep = new SdkMessageProcessingStep();
                registerStep.Name = strStepName;

                registerStep.EventHandler = new EntityReference(PluginType.EntityLogicalName, GetNextLabsPlugType(service));
                registerStep.SdkMessageId = new EntityReference(SdkMessage.EntityLogicalName, sdkMessageId);
                registerStep.SdkMessageFilterId = new EntityReference(SdkMessageFilter.EntityLogicalName, sdkMessageFilterId);

                Guid userId = GetUserId(Common.Constant.GeneralValue.SystemUserFullname, service);
                registerStep.ImpersonatingUserId = new EntityReference(SystemUser.EntityLogicalName, userId);

                registerStep.Rank = Common.Constant.GeneralValue.StepRank;
                registerStep.Stage = new OptionSetValue((int)Enum.Parse(typeof(emStepStage), stage));

                registerStep.Mode = new OptionSetValue((int)emStepMode.em_Synchronous);   // Synchronous
                registerStep.SupportedDeployment = new OptionSetValue((int)emStepDeployment.em_Server);   // server only

                Guid newStepGuid = service.Create(registerStep);
            }
        }

        public DataCollection<Entity> RetrieveAllRecordsImported(IOrganizationService service, string version)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = Common.DataModel.UserSettingField.SchemaName,

                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName =  Common.DataModel.UserSettingField.ColResolved1,
                            Operator = ConditionOperator.Equal,
                            Values = { version }
                        },
                    }
                }
            };
            query.ColumnSet.AddColumns(new string[]{
                Common.DataModel.UserSettingField.ColId,
                Common.DataModel.UserSettingField.ColDataType,
                Common.DataModel.UserSettingField.ColName
            });

            EntityCollection ec = service.RetrieveMultiple(query);

            return ec.Entities;
        }
        public void DeleteUserSettingRecords(IOrganizationService service, DataCollection<Entity> entities, NextLabsCRMLogs log)
        {
            foreach (Entity entity in entities)
            {
                object objValue = entity.Attributes[Common.DataModel.UserSettingField.ColDataType];
                if(objValue != null)
                {
                    if(objValue is OptionSetValue)
                    {
                        OptionSetValue value = (OptionSetValue)objValue;
                        if(value.Value == (int)Common.DataModel.RecordType.Entity)
                        {
                            UnregisterOnLoadHookMessage(entity.LogicalName, service, log);
                            UninstallMessageStep(entity.LogicalName, service, log);
                        }
                    }
                 }
                service.Delete(Common.DataModel.UserSettingField.SchemaName, entity.Id);
            }
        }

        private Dictionary<string, List<string>> GetMessageEventMap(string strlogicName)
        {
            Dictionary<string, List<string>> dictMessageEventMap = dictMessageEventMap = new Dictionary<string, List<string>>();
            if (strlogicName.Equals(Common.Constant.EntityName.Savedqueryvisualization, StringComparison.OrdinalIgnoreCase)
                         || strlogicName.Equals(Common.Constant.EntityName.Userqueryvisualization, StringComparison.OrdinalIgnoreCase))
            {
                dictMessageEventMap.Add(Common.Constant.MessageName.Retrievemultiple, new List<string> { emStepStage.post_operation.ToString() });
                dictMessageEventMap.Add(Common.Constant.MessageName.Retrieve, new List<string> { emStepStage.post_operation.ToString() });
            }
            else
            {
                dictMessageEventMap.Add(Common.Constant.MessageName.Retrievemultiple, new List<string> { emStepStage.Pre_operation.ToString(), emStepStage.post_operation.ToString() });
                dictMessageEventMap.Add(Common.Constant.MessageName.Retrieve, new List<string> { emStepStage.Pre_operation.ToString(), emStepStage.post_operation.ToString() });
                dictMessageEventMap.Add(Common.Constant.MessageName.Update, new List<string> { emStepStage.Pre_operation.ToString() });
                dictMessageEventMap.Add(Common.Constant.MessageName.Delete, new List<string> { emStepStage.Pre_validation.ToString() });
                dictMessageEventMap.Add(Common.Constant.MessageName.Create, new List<string> { emStepStage.Pre_operation.ToString() });
            }
            return dictMessageEventMap;
        }

        public void UninstallMessageStep(string logicName,IOrganizationService service, NextLabsCRMLogs log)
        {
            Dictionary<string, List<string>> dictMessageEventMap = GetMessageEventMap(logicName);
            foreach (string strMessage in dictMessageEventMap.Keys)
            {
                foreach (string strEventName in dictMessageEventMap[strMessage])
                {
                    string strStepName = string.Format(Common.Constant.DsiplayMessage.PlugInRegisterStepNameFormat, strMessage, logicName, strEventName);
                    Guid sdkMessageId = GetSdkMessageId(strMessage, service);
                    Guid sdkMessageFilterId = GetSdkMessageFilterId(logicName, sdkMessageId, service);
                    if (sdkMessageFilterId != Guid.Empty)
                    {
                        UnregiterSdkMessageProcessingStep(strStepName, service, log);
                    }
                }
            }
        }

        public void DeleteAllRecordsImportedByVersion(IOrganizationService service,string version, NextLabsCRMLogs log)
        {
            DataCollection<Entity> entities = RetrieveAllRecordsImported(service, version);
            DeleteUserSettingRecords(service, entities, log);
        }

        public void DeleteSettingRecord(IOrganizationService service, string name, Common.DataModel.RecordType type)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = Common.DataModel.UserSettingField.SchemaName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName =  Common.DataModel.UserSettingField.ColName,
                            Operator = ConditionOperator.Equal,
                            Values = { name }
                        },

                        new ConditionExpression
                        {
                            AttributeName =  Common.DataModel.UserSettingField.ColDataType,
                            Operator = ConditionOperator.Equal,
                            Values = { (int)type }
                        }
                    }
                }
            };
            EntityCollection ec = service.RetrieveMultiple(query);

            foreach (Entity entity in ec.Entities)
            {
                service.Delete(Common.DataModel.UserSettingField.SchemaName, entity.Id);
            }
        }

        private NextLabsCRMLogs InitializeLog(IOrganizationService orgService, ITracingService traceService, Guid userId)
        {
            //logcache and current log level
            RegularCache<Common.DataModel.LogSettings> logCache =
                (RegularCache<Common.DataModel.LogSettings>)CacheTier.LookupCache(CacheType.LogLevel);
            LogLevel curLogLevel = logCache.Lookup(Common.DataModel.UniqueRecordName.LogSettingsName,
                x => new Common.DataModel.LogSettings(Common.Enums.LogLevel.Error)).CurrentLevel;

            //current user
            MemoryCache<SystemUser> loginUserCache = (MemoryCache<SystemUser>)CacheTier.LookupCache(CacheType.LoginUserInfo);
            LogUserInfoReader logUserInfoReader = new LogUserInfoReader(orgService);

            string userName = "ID:" + userId.ToString();
            if (userId != null)
            {
                SystemUser user = loginUserCache.Lookup(userId.ToString(), x=>null);
                if(user == null)
                {
                    user = logUserInfoReader.CreateSystemUserWithNameOnly(userId.ToString());
                }

                if (user != null)
                {
                    userName = user.FullName;
                }
            }

            return new NextLabsCRMLogs(traceService, userName, curLogLevel);
        }

        private bool IsConcernedMessageAndEntity(Entity entity, string messageName, NextLabsCRMLogs log)
        {
            if (entity == null)
            {
                log.WriteLog(LogLevel.Debug, string.Format("There is no entity existing in the context of the Message {0}.", messageName));
                return false;
            }

            if (!messageName.Equals(Common.Constant.MessageName.Create, StringComparison.OrdinalIgnoreCase))
            {
                log.WriteLog(LogLevel.Debug, string.Format("Message {0} is not supported for entity {1}.", messageName, entity.LogicalName));
                return false;
            }

            if (!entity.LogicalName.Equals(Common.DataModel.UserSettingField.SchemaName, StringComparison.OrdinalIgnoreCase))
            {
                log.WriteLog(LogLevel.Debug, string.Format("Entity {0} is not the entity we concerned with", entity.LogicalName));
                return false;
            }

            return true;
        }

        public Common.DataModel.SecureEntity UpdateSystemUser(string xml)
        {
            Common.DataModel.SecureEntity securedEntity =
            (Common.DataModel.SecureEntity)m_xmlSerializer.Deserialize(typeof(Common.DataModel.SecureEntity), xml);

            if (securedEntity.Schema.LogicName.Equals(SystemUser.EntityLogicalName))
            {
                securedEntity.MergeAttributes(CacheTier.CreateDefaultUserMetaDataEnforced());
            }
            return securedEntity;
        }

        private void RemoveUnsupportedAttribute(
            List<Common.DataModel.MetaData.Attribute> supportedAttrList,
            List<Common.DataModel.MetaData.Attribute> verifiedAttrList)
        {
            if(supportedAttrList == null || supportedAttrList.Count == 0)
            {
                verifiedAttrList.Clear();
            }

            if(verifiedAttrList == null || verifiedAttrList.Count == 0)
            {
                return;
            }
            int count = verifiedAttrList.Count;
            for (int index = count-1;index >= 0; index--)
            {
                if(supportedAttrList.Find(x => x.LogicName.Equals(verifiedAttrList[index].LogicName))== null)
                {
                    verifiedAttrList.RemoveAt(index);
                }
            }
        }
        private string VerifyImportEntity(IOrganizationService service, Common.DataModel.UserSetting userSettings)
        {
            string xmlUpdated = userSettings.Content;
            //only import action will introduce version in the field Resolved1
            if (!string.IsNullOrWhiteSpace(userSettings.Resolved1))
            {
                Common.DataModel.SecureEntity securedEntity =
                (Common.DataModel.SecureEntity)m_xmlSerializer.Deserialize(typeof(Common.DataModel.SecureEntity), userSettings.Content);

                EntityMetaDataReader entityMetaDataReader = new EntityMetaDataReader(service);
                Common.DataModel.MetaData.Entity entityMetaData =
                    entityMetaDataReader.Read(securedEntity.Schema.LogicName);
                if(entityMetaData == null)
                {
                    throw new Exception(string.Format("[{0}]The entity {1} does not exist, because:{2}", ErrorCode.EntityNotExist,
                        securedEntity.Schema.LogicName,
                         "This is an entity bing imported with setting sync, it is new to the current dcrm system"));
                }

                RemoveUnsupportedAttribute(entityMetaData.Attributes, securedEntity.Schema.Attributes);

                xmlUpdated = (string)m_xmlSerializer.Serialize(typeof(Common.DataModel.SecureEntity), securedEntity);
                
             }

            return xmlUpdated;
        }
        public void Execute(IServiceProvider serviceProvider)
        {
            NextLabsCRMLogs log = null;
            try
            {
                #region prepare service
                ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                #endregion

                string failedRefreshCacheMsg = string.Empty;
                RefreshReason reason= RefreshReason.StillWait;
                try
                {
                    reason = CacheTier.CheckAndRefreshCache(new UserSettingsDCRMReader(service));
                }
                catch (Exception exp)
                {
                    failedRefreshCacheMsg = exp.Message;
                }

                log = InitializeLog(service, tracingService, context.InitiatingUserId);

                if (!string.IsNullOrEmpty(failedRefreshCacheMsg))
                {
                    string errmsg = "Failed to refresh cache.\n";
                    //log.WriteLog(LogLevel.Error, errmsg + failedRefreshCacheMsg);
                    throw new InvalidPluginExecutionException(errmsg);
                }

                Entity entity = (Entity)context.InputParameters[Common.Constant.ParameterName.Target];

                if (!IsConcernedMessageAndEntity(entity, context.MessageName, log))
                {
                    return;
                }

                Common.DataModel.UserSetting userSettings = new Common.DataModel.UserSetting();
                Common.DataModel.RecordFactory<Common.DataModel.UserSetting>.Create(entity, ref userSettings);

                //log.WriteLog(LogLevel.Error, string.Format("nxl setting record ready, datatype:{0},name:{1}, xml:{2}",
                //               userSettings.DataType,userSettings.Name, userSettings.Content));

                switch (userSettings.DataType)
                {
                    case Common.DataModel.RecordType.UserAttribute:
                        {
                            Common.DataModel.XSDSchema.CheckSecureEntityXml(userSettings.Content);
                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);

                            RegularCache<Common.DataModel.SecureEntity> userEntityCache =
                                (RegularCache<Common.DataModel.SecureEntity>)CacheTier.LookupCache(CacheType.SecureUserAttribute);

                            string xmlUpdated = VerifyImportEntity(service, userSettings);
                            Common.DataModel.SecureEntity userEntity = UpdateSystemUser(xmlUpdated);
                            userEntityCache.AddOrUpdate(userEntity.Schema.LogicName, userEntity);

                            xmlUpdated = (string)m_xmlSerializer.Serialize(typeof(Common.DataModel.SecureEntity), userEntity);
                            entity[Common.DataModel.UserSettingField.ColContent] = xmlUpdated;
                        }
                        break;
                    case Common.DataModel.RecordType.Entity:
                        {
                            Common.DataModel.XSDSchema.CheckSecureEntityXml(userSettings.Content);
                            
                            string xmlUpdated = VerifyImportEntity(service,userSettings);
                            
                            entity[Common.DataModel.UserSettingField.ColContent] = xmlUpdated;
                            RegisterEntity(userSettings.Name, userSettings.Enabled, service, log);

                            RegularCache<Common.DataModel.SecureEntity> entityCache =
                                (RegularCache<Common.DataModel.SecureEntity>)CacheTier.LookupCache(CacheType.SecureEntity);

                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                            Common.DataModel.SecureEntity secureEntity =
                                (Common.DataModel.SecureEntity)m_xmlSerializer.Deserialize(typeof(Common.DataModel.SecureEntity), xmlUpdated);

                            entityCache.AddOrUpdate(secureEntity.Schema.LogicName, secureEntity);
                        }
                        break;
                    case Common.DataModel.RecordType.LogLevel:
                        {
                            Common.DataModel.XSDSchema.CheckLogSettingsXml(userSettings.Content);
                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                            Common.DataModel.LogSettings logSettings =
                                (Common.DataModel.LogSettings)m_xmlSerializer.Deserialize(typeof(Common.DataModel.LogSettings), userSettings.Content);

                            RegularCache<Common.DataModel.LogSettings> logCache =
                                (RegularCache<Common.DataModel.LogSettings>)CacheTier.LookupCache(CacheType.LogLevel);
                            logCache.AddOrUpdate(Common.DataModel.UniqueRecordName.LogSettingsName, logSettings);
                        }
                        break;
                    case Common.DataModel.RecordType.GeneralSetting:
                        {
                            Common.DataModel.XSDSchema.CheckGeneralSettingXml(userSettings.Content);
                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                            //refresh cache immediately
                            RegularCache<Common.DataModel.GeneralSetting> generalSettingCache =
                                (RegularCache<Common.DataModel.GeneralSetting>)CacheTier.LookupCache(CacheType.GeneralSettings);

                            Common.DataModel.GeneralSetting generalSetting =
                                (Common.DataModel.GeneralSetting)m_xmlSerializer.Deserialize(typeof(Common.DataModel.GeneralSetting), userSettings.Content);
                            Common.DataModel.GeneralSetting.FillDefaultValueIfNotAssigned(ref generalSetting);

                            generalSettingCache.AddOrUpdate(Common.DataModel.UniqueRecordName.GeneralSettingsName, generalSetting);
                        }
                        break;
                    case Common.DataModel.RecordType.TestPC:
                        {
                            Common.DataModel.XSDSchema.CheckTestPCXml(userSettings.Content);
                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);

                            Common.DataModel.TestPC testPCParam =
                                (Common.DataModel.TestPC)m_xmlSerializer.Deserialize(typeof(Common.DataModel.TestPC), userSettings.Content);

                            string messageTransformed = TransfromToFriendlyMessage(TestPC(testPCParam, log));

                            if (!string.IsNullOrWhiteSpace(messageTransformed))
                            {
                                entity[Common.DataModel.UserSettingField.ColContent] = messageTransformed;
                            }
                            else
                            {
                                log.WriteLog(LogLevel.Error, "Test PC command is empty");
                            }
                        }
                        break;
                    case Common.DataModel.RecordType.TestWC:
                        {
                            Common.DataModel.XSDSchema.CheckTestWCXml(userSettings.Content);
                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                            Common.DataModel.TestWC testWCParam =
                                (Common.DataModel.TestWC)m_xmlSerializer.Deserialize(typeof(Common.DataModel.TestWC), userSettings.Content);

                            string returnedContent = "";
                            Common.DataModel.WCproxyResult returnedResult = TestWC(testWCParam, ref returnedContent, log);
                            //log.WriteLog(LogLevel.Debug, "TestWC|returnedContent : " + returnedContent);
                            if (returnedResult.Equals(Common.DataModel.WCproxyResult.OK))
                            {
                                entity[Common.DataModel.UserSettingField.ColContent] = returnedContent;

                            }
                            else if (returnedResult.Equals(Common.DataModel.WCproxyResult.Invalid_Host))
                            {
                                entity[Common.DataModel.UserSettingField.ColContent] = "Please check Policy Server Host";
                                log.WriteLog(LogLevel.Error, "Invalid Policy Server Host");
                            }
                            else if (returnedResult.Equals(Common.DataModel.WCproxyResult.Invalid_Usr_Pwd)) {
                                entity[Common.DataModel.UserSettingField.ColContent] = "Please check User Name and Password";
                                log.WriteLog(LogLevel.Error, "Invalid username/password combination");
                            }
                            else
                            {
                                entity[Common.DataModel.UserSettingField.ColContent] = "Exception when verifying cloudAz Connection.";
                                log.WriteLog(LogLevel.Error, "Exception when verifying cloudAz Connection.");
                            }
                        }
                        break;
                    case Common.DataModel.RecordType.IOCommand:
                        {
                            Common.DataModel.XSDSchema.CheckCommandXml(userSettings.Content);
                            DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                            Common.DataModel.IOCommand command = 
                                (Common.DataModel.IOCommand)m_xmlSerializer.Deserialize(typeof(Common.DataModel.IOCommand), userSettings.Content);
                            switch(command.Action)
                            {
                                //remove the data of the version imported
                                case Common.DataModel.CommandAction.Remove:
                                    DeleteAllRecordsImportedByVersion(service,command.Version,log);
                                    break;
                                case Common.DataModel.CommandAction.PublishAll:
                                    PublishAllCustomization(service);
                                    break;
                                case Common.DataModel.CommandAction.EnableSPAction:
                                    EnableSharePointAction(service, log);
                                    break;
                                case Common.DataModel.CommandAction.DisableSPAction:
                                    DisableSharePointAction(service, log);
                                    break;
                            }
                        }
                        break;
                    case Common.DataModel.RecordType.N1Relationship:
                        Common.DataModel.XSDSchema.CheckN1RelationshipXml(userSettings.Content);
                        DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                        Common.DataModel.N1Relationship n1Relationship =
                               (Common.DataModel.N1Relationship)m_xmlSerializer.Deserialize(typeof(Common.DataModel.N1Relationship), userSettings.Content);

                        RegularCache<Common.DataModel.N1Relationship> n1RelationshipCache =
                                (RegularCache<Common.DataModel.N1Relationship>)CacheTier.LookupCache(CacheType.N1Relationship);
                        n1RelationshipCache.AddOrUpdate(n1Relationship.RelatedEntity, n1Relationship);

                        break;
                    case Common.DataModel.RecordType.NNRelationship:
                        Common.DataModel.XSDSchema.CheckNNRelationshipXml(userSettings.Content);
                        DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                        Common.DataModel.NNRelationship nnRelationship =
                               (Common.DataModel.NNRelationship)m_xmlSerializer.Deserialize(typeof(Common.DataModel.NNRelationship), userSettings.Content);

                        RegularCache<Common.DataModel.NNRelationship> nnRelationshipCache =
                                (RegularCache<Common.DataModel.NNRelationship>)CacheTier.LookupCache(CacheType.NNRelationship);
                        nnRelationshipCache.AddOrUpdate(nnRelationship.RelatedEntity, nnRelationship);
                        break;
                    case Common.DataModel.RecordType.ImportPM:
                        DeleteSettingRecord(service, userSettings.Name, userSettings.DataType);
                        //log.WriteLog(LogLevel.Debug, @"ImportPM case");
                        string gsfield = QueryRecordField(service, Common.DataModel.UniqueRecordName.GeneralSettingsName, Common.DataModel.RecordType.GeneralSetting, Common.DataModel.UserSettingField.ColContent, log);
                        Common.DataModel.GeneralSetting gsobject =
                            (Common.DataModel.GeneralSetting)m_xmlSerializer.Deserialize(typeof(Common.DataModel.GeneralSetting), gsfield);
                        string ccUsername = gsobject.wcUsername;
                        string ccPassword = gsobject.wcPassword;
                        string ccHostAddr = gsobject.OAuthServerHost;
                        //new NxlCommon(ccHostAddr, ccUsername, ccPassword).SyncPolicyModel(userSettings.Content, log);
                        string strResponse = "Sync Policy Model Error";
                        new NxlCommon(ccHostAddr, ccUsername, ccPassword).SyncPolicyModel(userSettings.Content, log, ref strResponse);
                        entity[Common.DataModel.UserSettingField.ColContent] = strResponse;
                        break;
                    default:
                        break;
                }
            }
            catch (InvalidPluginExecutionException exp)
            {
                log.WriteLog(LogLevel.Error, string.Format("An exception occurred in the NextLabsRegisterPlugin plug-in:{0}", exp.Message));
                throw (exp);
            }
            catch (XmlSchemaException exp)
            {
                log.WriteLog(LogLevel.Error, string.Format("Xml content format is invalid:{0}", exp.Message));
                throw (exp);
            }
            catch(Exceptions.EntityNotExistException exp)
            {
                log.WriteLog(LogLevel.Error, string.Format("Import failed:{0}", exp.Message));
                throw (exp);
            }
            catch (Exception exp)
            {
                log.WriteLog(LogLevel.Error, string.Format("An Unexpected exception occurred in the NextLabsRegisterPlugin plug-in:{0}", exp.Message));
            //    throw new InvalidPluginExecutionException("An Unexpected exception occurred in the NextLabsRegisterPlugin plug-in.", exp);
            }
        }

        private void UnregiterSdkMessageProcessingStep(string sdkMessageProcessingStep, IOrganizationService service, NextLabsCRMLogs log)
        {
            log.WriteLog(LogLevel.Debug, string.Format("'UnregiterSdkMessageProcessingStep' started:{0}", sdkMessageProcessingStep));
            QueryExpression query = new QueryExpression
            {
                EntityName = SdkMessageProcessingStep.EntityLogicalName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "name",
                            Operator = ConditionOperator.Equal,
                            Values = { sdkMessageProcessingStep }
                        }
                    }
                }
            };
            EntityCollection ecStep = service.RetrieveMultiple(query);
            foreach (SdkMessageProcessingStep step in ecStep.Entities)
            {
                try
                {
                    service.Delete(SdkMessageProcessingStep.EntityLogicalName, step.Id);
                }
                catch (Exception ex)
                {
                    log.WriteLog(LogLevel.Debug, string.Format("'UnregiterSdkMessageProcessingStep' failed:{0} will retry:{1}.", sdkMessageProcessingStep, ex.Message));
                    //if delete faild , we will sleep and try again
                    System.Threading.Thread.Sleep(1000);
                    service.Delete(SdkMessageProcessingStep.EntityLogicalName, step.Id);
                    log.WriteLog(LogLevel.Debug, string.Format("'UnregiterSdkMessageProcessingStep' retried successfully:{0}", sdkMessageProcessingStep));
                }
            }
            log.WriteLog(LogLevel.Debug, string.Format("'UnregiterSdkMessageProcessingStep' ended:{0}", sdkMessageProcessingStep));
        }

        private Guid GetNextLabsPlugType(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = PluginType.EntityLogicalName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "typename",
                            Operator = ConditionOperator.Equal,
                            Values = { "NextLabs.CRMEnforcer.Plugin.EventHandler" }
                        }
                    }
                }
            };
            EntityCollection plugType = service.RetrieveMultiple(query);
            if (plugType.Entities.Count < 1)
            {
                return Guid.Empty;
            }

            return plugType.Entities[0].Id;
        }

        private Guid GetSdkMessageId(string message, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = SdkMessage.EntityLogicalName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "name",
                            Operator = ConditionOperator.Equal,
                            Values = { message }
                        }
                    }
                }
            };
            EntityCollection sdkMessageId = service.RetrieveMultiple(query);
            if (sdkMessageId.Entities.Count < 1)
            {
                return Guid.Empty;
            }

            return sdkMessageId.Entities[0].Id;
        }

        private Guid GetSdkMessageFilterId(string entityName, Guid sdkMessageId, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = SdkMessageFilter.EntityLogicalName,
                ColumnSet = new ColumnSet("availability", "iscustomprocessingstepallowed"),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "primaryobjecttypecode",
                            Operator = ConditionOperator.Equal,
                            Values = { entityName }
                        },
                        new ConditionExpression
                        {
                            AttributeName = "sdkmessageid",
                            Operator = ConditionOperator.Equal,
                            Values = { sdkMessageId }
                        }
                    }
                }
            };
            EntityCollection sdkMessageFilterId = service.RetrieveMultiple(query);

            foreach (SdkMessageFilter filter in sdkMessageFilterId.Entities)
            {
                if (filter.IsCustomProcessingStepAllowed == false || filter.Availability == 1)  // Client only
                {
                    continue;
                }

                return filter.Id;
            }

            return Guid.Empty;
        }

        private Guid GetUserId(string fullname, IOrganizationService service)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = SystemUser.EntityLogicalName,
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName = "fullname",
                            Operator = ConditionOperator.Equal,
                            Values = { fullname }
                        }
                    }
                }
            };

            EntityCollection userId = EntityDataHelp.RetrieveMultiple(service, query);
            if (userId.Entities.Count < 1)
            {
                return Guid.Empty;
            }

            return userId.Entities[0].Id;
        }

        public void UnregisterOnLoadHookMessage(string entityName, IOrganizationService service, NextLabsCRMLogs log)
        {
            log.WriteLog(LogLevel.Debug, string.Format("'UnregisterOnLoadHookMessage' started, entity name:{0}", entityName));

            // Query form from SystemForm
            QueryByAttribute query = new QueryByAttribute(SystemForm.EntityLogicalName);
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("objecttypecode");
            query.Values.AddRange(entityName);
            EntityCollection retrieved = service.RetrieveMultiple(query);
            //bool bNeedPublich = false;
            foreach (SystemForm sysform in retrieved.Entities)
            {
                /*
                 * 2: main
                 * 5: mobile
                 * 6: quick
                 * 7: quickCreate
                 */
                if (sysform.Type.Value != 2)
                {
                    continue;
                }
                //bNeedPublich = true;
                //Console.WriteLine(sf.FormXml);
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(sysform.FormXml);
                //  remove lib and event
                XmlNode handlerOnloadFunction = xml.SelectSingleNode("/form/events/event[@name='onload']/Handlers/Handler[@functionName='" + NextLabsPluginConstants.OnLoadFunction + "']");
                if (handlerOnloadFunction != null)
                {
                    handlerOnloadFunction.ParentNode.ParentNode.RemoveChild(handlerOnloadFunction.ParentNode);
                }

                XmlNode handlerOnSaveFunction = xml.SelectSingleNode("/form/events/event[@name='onsave']/Handlers/Handler[@functionName='" + NextLabsPluginConstants.OnSaveFunction + "']");
                if (handlerOnSaveFunction != null)
                {
                    handlerOnSaveFunction.ParentNode.ParentNode.RemoveChild(handlerOnSaveFunction.ParentNode);
                }

                XmlNode lib = (XmlElement)xml.SelectSingleNode("/form/formLibraries/Library[@name='" + NextLabsPluginConstants.OnLoadLib + "']");
                if (lib != null)
                {
                    XmlNode libs = lib.ParentNode;
                    libs.RemoveChild(lib);
                    if (libs.ChildNodes.Count == 0)
                    {
                        libs.ParentNode.RemoveChild(libs);
                    }
                }
                sysform.FormXml = xml.OuterXml;
                //Console.WriteLine(sf.FormXml);
                //update and publish modify
                UpdateRequest update = new UpdateRequest
                {
                    Target = sysform
                };

                try
                {
                    UpdateResponse response = (UpdateResponse)service.Execute(update);
                }catch(Exception exp)
                {
                    log.WriteLog(LogLevel.Debug, string.Format("'Failed to unregister OnLoad hook', entity name:{0}, reason:{1}", entityName,exp.Message));
                }
                
            }
            /*if (bNeedPublich)
            {
                PublishAllXmlRequest publish = new PublishAllXmlRequest();
                PublishAllXmlResponse publishAllResponse = (PublishAllXmlResponse)service.Execute(publish);
            }
            else
            {
                log.WriteLog(LogLevel.Debug, "There is no change. Publish is not required.");
            }*/
        }

        public void RegisterOnLoadHookMessage(string entityName, IOrganizationService service, NextLabsCRMLogs log)
        {
            log.WriteLog(LogLevel.Debug, string.Format("'RegisterOnLoadHookMessage' started, entity name:{0}", entityName));
            // Query form from SystemForm
            QueryByAttribute query = new QueryByAttribute(SystemForm.EntityLogicalName);
            query.ColumnSet = new ColumnSet(true);
            query.Attributes.AddRange("objecttypecode");
            query.Values.AddRange(entityName);
            EntityCollection retrieved = service.RetrieveMultiple(query);
            //bool bNeedPublich = false;
            foreach (SystemForm sysform in retrieved.Entities)
            {
                /*
                 * 2: main
                 * 5: mobile
                 * 6: quick
                 * 7: quickCreate
                 */
                //for fix bug 39096 we will ignore quickCreate view
                if (sysform.Type.Value != 2)
                {

                    continue;
                }
                //bNeedPublich = true;
                //Console.WriteLine(sf.FormXml);
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(sysform.FormXml);
                XmlNode form = xml.SelectSingleNode("/form");

                // add lib and event
                XmlElement libs = (XmlElement)xml.SelectSingleNode("/form/formLibraries");
                if (libs == null)
                {
                    libs = xml.CreateElement("formLibraries");
                    form.AppendChild(libs);
                }

                XmlElement lib = (XmlElement)xml.SelectSingleNode("/form/formLibraries/Library[@name='" + NextLabsPluginConstants.OnLoadLib + "']");
                if (lib == null)
                {
                    lib = xml.CreateElement("Library");
                    libs.AppendChild(lib);
                }
                lib.SetAttribute("name", NextLabsPluginConstants.OnLoadLib);
                lib.SetAttribute("libraryUniqueId", "{" + System.Guid.NewGuid().ToString() + "}");
                XmlElement events = (XmlElement)xml.SelectSingleNode("/form/events");
                if (events == null)
                {
                    events = xml.CreateElement("events");
                    form.AppendChild(events);
                }
                XmlElement evtOnload = (XmlElement)xml.SelectSingleNode("/form/events/event[@name='onload']");
                if (evtOnload == null)
                {
                    evtOnload = xml.CreateElement("event");
                    evtOnload.SetAttribute("name", "onload");
                    evtOnload.SetAttribute("application", "true");
                    evtOnload.SetAttribute("active", "true");
                    events.AppendChild(evtOnload);
                }
                XmlElement evtOnsave = (XmlElement)xml.SelectSingleNode("/form/events/event[@name='onsave']");
                if (evtOnsave == null)
                {
                    evtOnsave = xml.CreateElement("event");
                    evtOnsave.SetAttribute("name", "onsave");
                    evtOnsave.SetAttribute("application", "true");
                    evtOnsave.SetAttribute("active", "true");
                    events.AppendChild(evtOnsave);
                }
                XmlElement onloadhandlers = (XmlElement)xml.SelectSingleNode("/form/events/event[@name='onload']/Handlers");
                if (onloadhandlers == null)
                {
                    onloadhandlers = xml.CreateElement("Handlers");
                    evtOnload.AppendChild(onloadhandlers);
                }
                XmlElement onsavehandlers = (XmlElement)xml.SelectSingleNode("/form/events/event[@name='onsave']/Handlers");
                if (onsavehandlers == null)
                {
                    onsavehandlers = xml.CreateElement("Handlers");
                    evtOnsave.AppendChild(onsavehandlers);
                }
                XmlElement onloadhandler = (XmlElement)xml.SelectSingleNode("/form/events/event[@name='onload']/Handlers/Handler[@functionName='" +
                    NextLabsPluginConstants.OnLoadFunction + "']");
                if (onloadhandler == null)
                {
                    onloadhandler = xml.CreateElement("Handler");
                    onloadhandler.SetAttribute("functionName", NextLabsPluginConstants.OnLoadFunction);
                    onloadhandler.SetAttribute("libraryName", NextLabsPluginConstants.OnLoadLib);
                    onloadhandler.SetAttribute("handlerUniqueId", "{" + System.Guid.NewGuid().ToString() + "}");
                    onloadhandler.SetAttribute("enabled", "true");
                    onloadhandler.SetAttribute("parameters", "");
                    onloadhandler.SetAttribute("passExecutionContext", "true");

                    onloadhandlers.AppendChild(onloadhandler);
                }
                XmlElement onsavehandler = (XmlElement)xml.SelectSingleNode("/form/events/event[@name='onsave']/Handlers/Handler[@functionName='" +
                    NextLabsPluginConstants.OnSaveFunction + "']");
                if (onsavehandler == null)
                {
                    onsavehandler = xml.CreateElement("Handler");
                    onsavehandler.SetAttribute("functionName", NextLabsPluginConstants.OnSaveFunction);
                    onsavehandler.SetAttribute("libraryName", NextLabsPluginConstants.OnLoadLib);
                    onsavehandler.SetAttribute("handlerUniqueId", "{" + System.Guid.NewGuid().ToString() + "}");
                    onsavehandler.SetAttribute("enabled", "true");
                    onsavehandler.SetAttribute("parameters", "");
                    onsavehandler.SetAttribute("passExecutionContext", "true");

                    onsavehandlers.AppendChild(onsavehandler);
                }
                sysform.FormXml = xml.OuterXml;
                //Console.WriteLine(sf.FormXml);
                // update and publish modify
                UpdateRequest update = new UpdateRequest
                {
                    Target = sysform
                };

                try
                {
                    UpdateResponse response = (UpdateResponse)service.Execute(update);
                }
                catch(Exception exp)
                {
                    log.WriteLog(LogLevel.Warning, string.Format("Failed to register onload hook, entity name:{0}, reason:{1}", entityName,exp.Message));
                }
            }
            /*if (bNeedPublich)
            {
                PublishAllXmlRequest publish = new PublishAllXmlRequest();

                do
                {
                    //frequently execute the publish will lead to database deadlock and get "Generic SQL Error",
                    //when this issue happen, wait for a while and try again.
                    try
                    {
                        PublishAllXmlResponse publishAllResponse = (PublishAllXmlResponse)service.Execute(publish);
                        break;
                    }
                    catch (Exception exp)
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                } while (true);
            }
            else
            {
                log.WriteLog(LogLevel.Debug, "There is no change. Publish is not required.");
            }*/
        }

        private void CreateSPActionRecords(IOrganizationService service)
        {
            string[] actions = new string[] { "view", "update", "delete","new","checkin","checkout","update_property" };
            foreach(string action in actions)
            {
                Entity record = new Entity(Common.Constant.EntityName.NXLSPAction);
                record.Attributes.Add(Common.Constant.AttributeKeyName.NXL_SPAction_Name, action);
                service.Create(record);
            }         
        }

        private DataCollection<Entity> RetrieveAllSPActionRecords(IOrganizationService service)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = Common.Constant.EntityName.NXLSPAction,
            };
            query.ColumnSet.AddColumns(new string[]{
                Common.Constant.AttributeKeyName.NXL_SPAction_Id
            });

            EntityCollection ec = service.RetrieveMultiple(query);

            return ec.Entities;
        }
        private void DeleteAllSPActionRecords(IOrganizationService service, DataCollection<Entity> entities)
        {
            foreach(Entity e in entities)
            {
                try
                {
                    service.Delete(Common.Constant.EntityName.NXLSPAction, e.Id);
                }
                catch(Exception exp)
                {
                    //if delete faild , we will sleep and try again
                    System.Threading.Thread.Sleep(1000);
                    service.Delete(Common.Constant.EntityName.NXLSPAction, e.Id);                   
                }
            }
        }
        private void EnableSharePointAction(IOrganizationService service, NextLabsCRMLogs log)
        {
            RegisterEntity(
                Common.Constant.EntityName.NXLSPAction,
                Common.Constant.MessageName.Retrievemultiple,
                emStepStage.post_operation.ToString(),
                true,
                service,
                log);

            RegisterEntity(
                Common.Constant.EntityName.NXLSPAction,
                Common.Constant.MessageName.Retrievemultiple,
                emStepStage.Pre_operation.ToString(),
                true,
                service,
                log);
            CreateSPActionRecords(service);
        }

        private void DisableSharePointAction(IOrganizationService service, NextLabsCRMLogs log)
        {
            RegisterEntity(
                Common.Constant.EntityName.NXLSPAction,
                Common.Constant.MessageName.Retrievemultiple,
                emStepStage.post_operation.ToString(),
                false,
                service,
                log);

            RegisterEntity(
                Common.Constant.EntityName.NXLSPAction,
                Common.Constant.MessageName.Retrievemultiple,
                emStepStage.Pre_operation.ToString(),
                false,
                service,
                log);

            DataCollection<Entity> entities = RetrieveAllSPActionRecords(service);
            DeleteAllSPActionRecords(service, entities);
        }
        private void PublishAllCustomization(IOrganizationService service)
        {
            PublishAllXmlRequest publish = new PublishAllXmlRequest();

            do
            {
                //frequently execute the publish will lead to database deadlock and get "Generic SQL Error",
                //when this issue happen, wait for a while and try again.
                try
                {
                    PublishAllXmlResponse publishAllResponse = (PublishAllXmlResponse)service.Execute(publish);
                    break;
                }
                catch (Exception exp)
                {
                    System.Threading.Thread.Sleep(1000);
                }
            } while (true);
        }

        private Common.DataModel.WCproxyResult TestWC(Common.DataModel.TestWC param, ref string returnedToken, NextLabsCRMLogs log) {

            Common.DataModel.WCproxyResult result = Common.DataModel.WCproxyResult.Failed;

            if (string.IsNullOrEmpty(param.OAuthServerHost)) {
                return result;
            }
            string strAuthUrl = string.Empty;
            string strlt = string.Empty;
            string strExecution = string.Empty;
            string strConsoleUrl = @"https://" + Util.GetCCHost(param.OAuthServerHost) + @"/console";
            HttpStatusCode statusCode = 0;

            try
            {
                try
                {
                    LogonWebConsole.GetAuthUrlAndParameter(strConsoleUrl, out strAuthUrl, out strlt, out strExecution);
                    strAuthUrl = @"https://" + Util.GetCCHost(param.OAuthServerHost) + strAuthUrl;
                }
                catch (Exception ex)
                {
                    result = Common.DataModel.WCproxyResult.Invalid_Host;
                    log.WriteLog(LogLevel.Debug, "TestWC Invalid_Host: " + result);
                    return result;
                }

                string url = LogonWebConsole.GetAutologinUrl(strAuthUrl, param.wcUsername, LogonWebConsole.UrlEncode(param.wcPassword), strlt, LogonWebConsole.UrlEncode(strExecution), out statusCode);
                log.WriteLog(LogLevel.Debug, "url: " + url + ", statusCode: " + statusCode);
                returnedToken = url;

                if (!string.IsNullOrEmpty(returnedToken))
                {
                    if (returnedToken.StartsWith("http") &&
                        statusCode == HttpStatusCode.Redirect)
                    {   // statusCode == Redirect
                        result = Common.DataModel.WCproxyResult.OK;
                    }
                }
                else 
                {
                    if (statusCode == HttpStatusCode.OK)
                    {
                        result = Common.DataModel.WCproxyResult.Invalid_Usr_Pwd;
                        log.WriteLog(LogLevel.Debug, "returnedToken IsNullOrEmpty, result : " + result);
                    }
                }

            }
            catch (Exception ex)
            {
                result = Common.DataModel.WCproxyResult.HttpFaild_Unknown;
                log.WriteLog(LogLevel.Debug, "TestWC exception : " + result);
            }
            return result;
        }

        private string QueryRecordField(IOrganizationService service, string name, Common.DataModel.RecordType type, string field, NextLabsCRMLogs log) {
            string strQuery = @"";
            QueryExpression query = new QueryExpression
            {
                EntityName = Common.DataModel.UserSettingField.SchemaName,
                ColumnSet = new ColumnSet(field),
                Criteria =
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                    {
                        new ConditionExpression
                        {
                            AttributeName =  Common.DataModel.UserSettingField.ColName,
                            Operator = ConditionOperator.Equal,
                            Values = { name }
                        },

                        new ConditionExpression
                        {
                            AttributeName =  Common.DataModel.UserSettingField.ColDataType,
                            Operator = ConditionOperator.Equal,
                            Values = { (int)type }
                        }
                    }
                }
            };
            EntityCollection ec = service.RetrieveMultiple(query);
            strQuery = ec.Entities[0].GetAttributeValue<string>(field);
            return strQuery;
        }

        private PCResult TestPC(Common.DataModel.TestPC param, NextLabsCRMLogs log)
        {
            PCResult result = PCResult.Failed;
            try
            {
                if (!string.IsNullOrEmpty(param.PolicyControllerHost))
                {
                    NextLabs.JavaPC.RestAPISDK.JavaPC javaPC = null;
                    NextLabs.JavaPC.RestAPISDK.JavaPC.DelegateLog delegateLog = null;
#if DEBUG
                    delegateLog = new NextLabs.JavaPC.RestAPISDK.JavaPC.DelegateLog((msg) => { log.WriteLog(LogLevel.Debug, msg); });
#endif
                    //log.WriteLog(LogLevel.Error, Util.GetJPCHost(param.PolicyControllerHost));
                    string jpcHostAddress = Util.GetJPCHost(param.PolicyControllerHost) + ":" + Util.GetJPCPort();
                    //log.WriteLog(LogLevel.Error, jpcHostAddress);
                    //workaround with local jpc
                    if (jpcHostAddress.IndexOf("qalab01") != -1)
                    {
                        jpcHostAddress = @"http:\\" + Util.GetJPCHost(param.PolicyControllerHost) + ":58080";
                    }
                    else if (param.IsHttps)
                    //if (param.IsHttps)
                    {
                        jpcHostAddress = @"https:\\" + jpcHostAddress;
                    }
                    else
                    {
                        jpcHostAddress = @"http:\\" + jpcHostAddress;
                    }

                    if (!string.IsNullOrEmpty(param.OAuthServerHost))
                    {
                        string oauthHostAddress = @"https:\\" + Util.GetCCHost(param.OAuthServerHost) + ":" + Util.GetCCPort();
                        //log.WriteLog(LogLevel.Error, oauthHostAddress);
                        javaPC = new NextLabs.JavaPC.RestAPISDK.JavaPC(NextLabs.JavaPC.RestAPISDK.RequestDataType.Json, jpcHostAddress, oauthHostAddress, param.ClientID, param.ClientPassword, delegateLog, true, 3600);
                    }
                    else
                    {
                        javaPC = new NextLabs.JavaPC.RestAPISDK.JavaPC(NextLabs.JavaPC.RestAPISDK.RequestDataType.Json, jpcHostAddress, delegateLog);
                    }
                    if (javaPC.Authorizationed.Equals(PCResult.OK))
                    {
                        result = javaPC.CheckPCState();
                    }
                    else
                    {
                        result = javaPC.Authorizationed;
                    }
                }
                else
                {
                    result = PCResult.Failed;
                }
            }
            catch (Exception ex)
            {
                result = PCResult.HttpFaild_Unknow;
            }
            return result;
        }
        private string TransfromToFriendlyMessage(PCResult pcResult)
        {
            string result = string.Empty;
            switch (pcResult)
            {
                case PCResult.OK:
                    {
                        result = Common.Constant.TestConnactionResultMessage.Success;
                    }
                    break;
                case PCResult.InvalidClient:
                case PCResult.HttpFaild_Unauthorized:
                    {
                        result = Common.Constant.TestConnactionResultMessage.AuthenticatieFailed;
                    }
                    break;
                case PCResult.ResponseStatusAbnormal:
                    {
                        result = Common.Constant.TestConnactionResultMessage.PolicyControllerStatusError;
                    }
                    break;
                case PCResult.InvalidOauthServer:
                    {
                        result = Common.Constant.TestConnactionResultMessage.ConnectToOauthHostFaild;
                    }
                    break;
                case PCResult.HttpFaild_Unknow:
                    {
                        result = Common.Constant.TestConnactionResultMessage.Unknow;
                    }
                    break;
                default:
                    {
                        result = Common.Constant.TestConnactionResultMessage.ConnectToRemoteHostFaild;
                    }
                    break;
            }
            return result;
        }
        public class NextLabsPluginConstants
        {
            public const string DefaultActionForInternalException = "setting_defaultaction";
            public const string DefaultActionForInternalExceptionAllow = "allow";
            public const string DefaultActionForInternalExceptionDeny = "deny";
            public const string HintForInternalException = "setting_hint";
            public const string HintForDefaultDenyMessage = "setting_hintDefaultDeny";
            public const string CRMLogLevel = "setting_loglevel";
            public const string CacheExpiryTime = "setting_CacheExpiryTime";

            public const string ClientId = "JPCUsername";
            public const string ClientSecurity = "JPCPassword";

            public const string OnLoadLib = "nxl_warningonload.js";
            public const string OnLoadFunction = "maskOnLoad";
            public const string OnSaveFunction = "remaskOnSave";
        }
    }
}
