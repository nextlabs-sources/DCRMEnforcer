using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.DataBussiness.Obligation;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using NextLabs.CRMEnforcer.Common.DataBussiness.IO;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Common.QueryPC;
using NextLabs.CRMEnforcer.Enforces;
using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle;
using NextLabs.CRMEnforcer.Common.DataBussiness.Condition;
using NextLabs.CRMEnforcer.Common.DataBussiness.Sharepoint;
using static NextLabs.CRMEnforcer.Common.DataBussiness.Sharepoint.SPActionHelper;
using System.Web;

namespace NextLabs.CRMEnforcer.Plugin
{
    public class EventHandler : IPlugin
    {
        public void DoListAction(IOrganizationService service,
            EnforceResult enforcerResult,
			string[] arrayInputColumn,
            EntityCollection outputEntities,
            MemoryCache<SecureEntity> secureEntityCache,
            string strEntityName,
            Guid initUserId,
            List<Guid> userTeams,
            Dictionary<string, List<Guid>> sharedRecords,
            NextLabsCRMLogs log)
        {
            if (outputEntities.Entities.Count > 0)
            {
                MaskFieldHandles maskFieldHandle = new MaskFieldHandles(enforcerResult.ListObligations, secureEntityCache, strEntityName, log);
                if (maskFieldHandle.NeedMaskField())
                {
                    List<string> lisEntityId = new List<string>();
                    List<string> lisColumn = new List<string>();
                    foreach (Entity item in outputEntities.Entities)
                    {
                        lisEntityId.Add(item.Id.ToString());
                        log.WriteLog(LogLevel.Debug, string.Format("Output entity id {0}", item.Id.ToString()));
                    }
                    log.WriteLog(LogLevel.Debug, string.Format("{0},Out put entities count {1}", strEntityName, outputEntities.Entities.Count));
                    SecureEntity metadata = secureEntityCache.Lookup(strEntityName, x => null);
                    if (metadata == null)
                    {
                        throw new Exceptions.InvalidCacheException(strEntityName);
                    }
                    QueryExpression queryExpression = new QueryExpression(strEntityName);
                    queryExpression.Criteria.AddCondition(metadata.Schema.PrimaryIDName, ConditionOperator.In, lisEntityId.ToArray());
                    
                    queryExpression.ColumnSet = Common.DataBussiness.EntityDataHelp.GetEntityColumn(strEntityName, secureEntityCache);
                    EntityCollection enforcedAttributesEntites = Common.DataBussiness.EntityDataHelp.RetrieveMultiple(service, queryExpression);

                    List<Entity> mapedEntities = MapEntityWithEnforcedAttributesToOriginalEntity(strEntityName,enforcedAttributesEntites, outputEntities,log);
                    for (int index = 0; index < mapedEntities.Count; index++)
                    {
                        Entity enforcedAttributesEntity = mapedEntities[index];
                        log.WriteLog(LogLevel.Debug, string.Format("Start maskfield for reocrd {0}", index));
                        maskFieldHandle.DoObligation(enforcedAttributesEntity.Attributes, arrayInputColumn, outputEntities[index].Attributes, metadata);
                        EntityDataHelp.AppendIsOwnerFakeAttribute(enforcedAttributesEntity.Attributes, enforcedAttributesEntity.LogicalName, secureEntityCache, initUserId, userTeams);
                        EntityDataHelp.AppendIsSharedFakeAttribute(enforcedAttributesEntity.Attributes, enforcedAttributesEntity.LogicalName, secureEntityCache, enforcedAttributesEntity.Id, sharedRecords);
                        maskFieldHandle.DoObligation(enforcedAttributesEntity.Attributes, arrayInputColumn, outputEntities[index].Attributes, metadata);
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Information, "don't contain mask field obligation  , ignore Mask Field obligation ");
                }
            }
            else
            {
                log.WriteLog(LogLevel.Information, "Entity reocrd count is 0 , ignore Mask Field obligation ");
            }
        }

        /// <summary>
        /// This method is to resolve the problem that:
        /// In the original Entity collection, there probably are some duplicated records sharing the same id 
        /// caused by N:N relationship enforced by our plugin, we need map the entities we retrieved by ourselves 1:1 to the original entities
        /// otherwise, the entity record will not be masked if it is duplicated but not the first one.
        /// </summary>
        /// <param name="enforcedAttributesEntities">in this collection, every id is unique</param>
        /// <param name="originalEntities">in this collection, some records may share the same id</param>
        /// <returns>a 1:1 list corresponding to the original entity collection</returns>
        List<Entity> MapEntityWithEnforcedAttributesToOriginalEntity(
            string entityName,
            EntityCollection enforcedAttributesEntities, 
            EntityCollection originalEntities,
            NextLabsCRMLogs log)
        {
            List<Entity> maped = new List<Entity>();
            for (int i = 0; i < originalEntities.Entities.Count; i++)
            {
                Entity originalEntity = originalEntities.Entities[i];
                Guid originalRecordID = originalEntity.Id;
                if (originalRecordID.Equals(Guid.Empty))
                {
                    //try to get the id from attribute
                    originalRecordID = originalEntity.GetAttributeValue<Guid>(entityName+"id");
                }

                log.WriteLog(LogLevel.Debug, string.Format("original record id in NxN relationship is {0}", originalEntities.Entities[i].Id));
                bool bFound = false;
                for (int j = 0; j < enforcedAttributesEntities.Entities.Count; j++)
                {
                    Entity enforcedEntity = enforcedAttributesEntities.Entities[j];
                    if (enforcedEntity.Id == originalRecordID)
                    {
                        maped.Add(enforcedEntity);
                        bFound = true;
                        break;
                    }
                }

                if(!bFound)
                {
                    //will never reach here in this context
                    throw new Exception(
                        string.Format("there is no corresponding record with enforced attributes for the original output records, id: {0} ",
                        originalRecordID));                  
                }
            }
            return maped;
        }
        public void GetCRMUserAction(IPluginExecutionContext context, ref emCRMUserAction emAction)
        {
            emAction = emCRMUserAction.emUNKNOWN_ACTION;

            if (context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase))
            {
                emAction = emCRMUserAction.em_CREATE_ACTION;
                return;
            }

            if (context.MessageName.Equals("Delete", StringComparison.OrdinalIgnoreCase))
            {
                emAction = emCRMUserAction.em_DELETE_ACTION;
                return;
            }

            if (context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase))
            {
                emAction = emCRMUserAction.em_EDIT_ACTION;
                return;
            }

            if (context.MessageName.Equals("RetrieveMultiple", StringComparison.OrdinalIgnoreCase))
            {
                if (context.InputParameters.Contains("Query"))
                {
                    if (context.InputParameters["Query"] is FetchExpression)
                    {
                        emAction = emCRMUserAction.em_LIST_ACTION;
                        return;
                    }
                    else if (context.InputParameters["Query"] is QueryExpression)
                    {
                        emAction = emCRMUserAction.em_LIST_ACTION;
                        return;
                    }
                }
            }

            if (context.MessageName.Equals("Retrieve", StringComparison.OrdinalIgnoreCase))
            {
                emAction = emCRMUserAction.em_CLICK_RECORD_ACTION;
                return;
            }
        }
        
        private void AddFilterToChartPanel(Entity entityBusiness,
                                                          IPluginExecutionContext context,
                                                          IOrganizationService service,
                                                          Dictionary<string, FilterExpression> dirCacheChartFilter,
                                                          string strDefaultDenyMessage,
                                                          AttributeCollection userAttrs,
                                                          MemoryCache<SecureEntity> userAttrsStruct,
                                                          MemoryCache<SecureEntity> entityAttrsStruct,
                                                          MemoryCache<N1Relationship> n1Relationship,
                                                          MemoryCache<NNRelationship> nNRelationship,
                                                          ParameterCollection sharedParam,
                                                          List<Guid> userTeams,
                                                          Dictionary<string, List<Guid>> sharedRecords,
                                                          NextLabsCRMLogs log,
                                                          string userHostIP,
                                                          string strEntityName = null
            )
        {
            if (entityBusiness.Attributes != null && entityBusiness.Attributes.ContainsKey(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.Datadescription))
            {
                string strDatadescriptionXml = entityBusiness.Attributes[NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.Datadescription].ToString();
                ChartDatadefinitionXMLHelp chartDateDefindXMLHelp = ChartDatadefinitionXMLHelp.CreateInstance(strDatadescriptionXml);
                if (chartDateDefindXMLHelp != null)
                {
                    string strChartCurrentEntityName = strEntityName;
                    if (string.IsNullOrEmpty(strChartCurrentEntityName))
                    {
                        strChartCurrentEntityName = chartDateDefindXMLHelp.CurrentEntityName;
                    }
                    if (entityAttrsStruct.GetKeys().Exists((dir => { return dir.Equals(strChartCurrentEntityName, StringComparison.OrdinalIgnoreCase); })))
                    {
                        if (!dirCacheChartFilter.ContainsKey(strChartCurrentEntityName))
                        {
                            FetchExpression fetchTotal = new FetchExpression(chartDateDefindXMLHelp.GetFetchXMLInitial());
                            //QueryExpression queryTotal = new QueryExpression(strChartCurrentEntityName);

                            ConditionFactory conditionFactory = new ConditionFactory(entityAttrsStruct, service, context.InitiatingUserId, userTeams, true, sharedRecords);
                            Enforces.RetrieveMultiple retrieveMultipleEnforce = new RetrieveMultiple(service,
                                                                                                                                    strChartCurrentEntityName,
                                                                                                                                    userAttrs,
                                                                                                                                    userAttrsStruct,
                                                                                                                                    entityAttrsStruct,
                                                                                                                                    log,
                                                                                                                                    context.InitiatingUserId,
                                                                                                                                    n1Relationship,
                                                                                                                                    nNRelationship,
                                                                                                                                    fetchTotal,
                                                                                                                                    strDefaultDenyMessage,
                                                                                                                                    sharedParam,
                                                                                                                                    userTeams,
                                                                                                                                    sharedRecords,
                                                                                                                                    conditionFactory,
                                                                                                                                    userHostIP);
                            retrieveMultipleEnforce.DoEnforce(false);
                            retrieveMultipleEnforce.ExecuteEnforceResult();
                            chartDateDefindXMLHelp.ReplaceFetch(fetchTotal.Query);
                            entityBusiness.Attributes[Common.Constant.AttributeKeyName.Datadescription] = chartDateDefindXMLHelp.ToString();
                        }
                        else
                        {
                            FilterExpression filterExpressForChart = dirCacheChartFilter[strChartCurrentEntityName];
                            if (filterExpressForChart != null && filterExpressForChart.Filters != null && filterExpressForChart.Filters.Count > 0)
                            {
                                chartDateDefindXMLHelp.AddFilter(filterExpressForChart);
                                entityBusiness.Attributes[Common.Constant.AttributeKeyName.Datadescription] = chartDateDefindXMLHelp.ToString();
                                log.WriteLog(LogLevel.Error, "Datadescription---" + entityBusiness.Attributes[Common.Constant.AttributeKeyName.Datadescription].ToString());
                            }
                            else
                            {
                                log.WriteLog(LogLevel.Error, "When retriving 'savedqueryvisualization' on post-event, the chart cannot be filtered.");
                            }
                        }
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Information, string.Format("Enforcer can't secure the entity[{0}].", strChartCurrentEntityName));
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Warning, "When retriving 'savedqueryvisualization' on post-event, the chart could not be filtered because the serialization from xml failed.");
                    log.WriteLog(LogLevel.Debug, "Chart data XML:" + strDatadescriptionXml);
                }

            }
            else
            {
                log.WriteLog(LogLevel.Error, "When retriving 'savedqueryvisualization' on post-event, the chart could not be filtered because the attributes of the business entity in XML could not be found.");
            }
        }


        private void PostEventForRetriverSavedqueryvisualization(IPluginExecutionContext context,
                                                                                           IOrganizationService service,
                                                                                           string strDefaultDenyMessage,
                                                                                          AttributeCollection userAttrs,
                                                                                          MemoryCache<SecureEntity> userAttrsStruct,
                                                                                          MemoryCache<SecureEntity> entityAttrsStruct,
                                                                                          MemoryCache<N1Relationship> n1Relationship,
                                                                                          MemoryCache<NNRelationship> nNRelationship,
                                                                                          ParameterCollection sharedParam,
                                                                                          List<Guid> userTeams,
                                                                                          Dictionary<string, List<Guid>> sharedRecords,
                                                                                          NextLabsCRMLogs log,
                                                                                          string userHostIP
            )
        {
            if (context.OutputParameters.Contains(NextLabs.CRMEnforcer.Common.Constant.ParameterName.BusinessEntity) &&
                context.OutputParameters[NextLabs.CRMEnforcer.Common.Constant.ParameterName.BusinessEntity] is Entity)
            {
                Dictionary<string, FilterExpression> cacheDirChartFilter = new Dictionary<string, FilterExpression>();
                Entity entityBusiness = context.OutputParameters[NextLabs.CRMEnforcer.Common.Constant.ParameterName.BusinessEntity] as Entity;

                AddFilterToChartPanel(entityBusiness,
                                                context,
                                                service,
                                                cacheDirChartFilter,
                                                strDefaultDenyMessage,
                                                userAttrs,
                                                userAttrsStruct,
                                                entityAttrsStruct,
                                                n1Relationship,
                                                nNRelationship,
                                                sharedParam,
                                                userTeams,
                                                sharedRecords,
                                                log,
                                                userHostIP);
            }
            else
            {
                log.WriteLog(LogLevel.Error, "When retriving 'savedqueryvisualization' on post-event, the chart could not  be filtered because the business entity could not be found.");
            }
        }
        private void PostEventForRetrievrMuiltSavedqueryvisualzation(IPluginExecutionContext context,
                                                                                           IOrganizationService service,
                                                                                           string strDefaultDenyMessage,
                                                                                          AttributeCollection userAttrs,
                                                                                          MemoryCache<SecureEntity> userAttrsStruct,
                                                                                          MemoryCache<SecureEntity> entityAttrsStruct,
                                                                                          MemoryCache<N1Relationship> n1Relationship,
                                                                                          MemoryCache<NNRelationship> nNRelationship,
                                                                                          ParameterCollection sharedParam,
                                                                                          List<Guid> userTeams,
                                                                                          Dictionary<string, List<Guid>> sharedRecords,
                                                                                          NextLabsCRMLogs log,
                                                                                          string userHostIP)
        {
            if (context.OutputParameters.Contains(NextLabs.CRMEnforcer.Common.Constant.ParameterName.BusinessEntityCollection) && context.OutputParameters[NextLabs.CRMEnforcer.Common.Constant.ParameterName.BusinessEntityCollection] is EntityCollection)
            {

                EntityCollection entitys = context.OutputParameters[NextLabs.CRMEnforcer.Common.Constant.ParameterName.BusinessEntityCollection] as EntityCollection;
                if (entitys.Entities != null)
                {
                    Dictionary<string, FilterExpression> cacheDirChartFilter = new Dictionary<string, FilterExpression>();
                    foreach (Entity entityBusiness in entitys.Entities)
                    {
                        if (entityBusiness.Attributes != null && entityBusiness.Attributes.ContainsKey(NextLabs.CRMEnforcer.Common.Constant.AttributeKeyName.Datadescription))
                        {
                            if (context.PrimaryEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.Savedqueryvisualization))
                            {
                                AddFilterToChartPanel(entityBusiness,
                                                                context,
                                                                service,
                                                                cacheDirChartFilter,
                                                                strDefaultDenyMessage,
                                                                userAttrs,
                                                                userAttrsStruct,
                                                                entityAttrsStruct,
                                                                n1Relationship,
                                                                nNRelationship,
                                                                sharedParam,
                                                                userTeams,
                                                                sharedRecords,
                                                                log,
                                                                userHostIP,
                                                                entityBusiness.LogicalName);
                            }
                            else if (context.PrimaryEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.Userqueryvisualization))
                            {
                                AddFilterToChartPanel(entityBusiness,
                                                                context,
                                                                service,
                                                                cacheDirChartFilter,
                                                                strDefaultDenyMessage,
                                                                userAttrs,
                                                                userAttrsStruct,
                                                                entityAttrsStruct,
                                                                n1Relationship,
                                                                nNRelationship,
                                                                sharedParam,
                                                                userTeams,
                                                                sharedRecords,
                                                                log,
                                                                userHostIP);
                            }
                        }

                    }
                }
            }
        }
     
        private bool IsMessageSentByUs(int contextDepth, Guid? requestId)
        {
            if (contextDepth == 1 || requestId == null)
            {
                return false;
            }

            return Common.NXLIDProvider.IsNXLGUID(requestId.Value);
        }

        private bool IsPreEventMessage(int stage)
        {
            //this code for 10 (pre-validation), 20 (pre-operation)
            return stage == 10 || stage == 20;
        }

        private bool IsPostEventMessage(int stage)
        {
            //this code for 40 (post-operation), and 50 (post-operation,deprecated).
            return stage == 40 || stage == 50;
        }

        public void Execute(IServiceProvider serviceProvider)
        {
     DateTime prevTime = DateTime.Now; //time cost
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            //skip the msg sent from our plugin only works on D365 on-premise 
            if (IsMessageSentByUs(context.Depth, context.RequestId))
            {
                tracingService.Trace("this request-{0} is sent by us, ignore it", context.RequestId);
                return;
            }
            else
            {
                //D365 online use itself request id rather than the request id we created for our request from plugin, so use the old mechinism in v1.0 to prevent request loop
                if (context.Depth != 1)
                {
                    if (context.ParentContext == null || 
                        !(context.ParentContext.MessageName.Equals("ExportToExcel", StringComparison.OrdinalIgnoreCase) ||
                          context.ParentContext.MessageName.Equals("ExportToExcelOnline", StringComparison.OrdinalIgnoreCase) ||
                          context.ParentContext.MessageName.Equals("ExportDynamicToExcel", StringComparison.OrdinalIgnoreCase)))
                    {
                        tracingService.Trace("Parent context is {0}", context.ParentContext == null ? "null" : "not null");
                        tracingService.Trace("according to the depth, this request-{0} of message-{1} is sent by us, ignore it", 
                            context.RequestId,
                            context.ParentContext == null ? "null" : context.ParentContext.MessageName);
                        return;
                    }
                }
            }

            tracingService.Trace("this request id is {0}; stage {1}", context.RequestId,context.Stage);
            double serviceRetriveCost = DateTime.Now.Subtract(prevTime).TotalMilliseconds; //time cost
      prevTime = DateTime.Now; //timecost
            RefreshReason reason = RefreshReason.StillWait;
            string failedRefreshCacheMsg = string.Empty;
            try
            {
                //refresh cache from nxl_settings entity
                reason = CacheTier.CheckAndRefreshCache(new UserSettingsDCRMReader(service));
            }
            catch (Exception e)
            {
                failedRefreshCacheMsg = e.ToString();
            }

      double refreshCost = DateTime.Now.Subtract(prevTime).TotalMilliseconds; //time cost
      prevTime = DateTime.Now; //timecost

            GeneralSetting generalSetting = null;

            //init log first to ensure there is a log sevice useable at least.
            NextLabsCRMLogs log =
                new NextLabsCRMLogs(tracingService, "ID:" + context.InitiatingUserId.ToString(), LogLevel.Error);
      double loginitCost1 = DateTime.Now.Subtract(prevTime).TotalMilliseconds; //time cost
      prevTime = DateTime.Now; //timecost

            DateTime tryCostbegin = DateTime.Now;
            SPActionHelper spActionHelper = null;
            try
            {
                //get all the caches
                MemoryCache<GeneralSetting> generalSettingCache = (MemoryCache<GeneralSetting>)CacheTier.LookupCache(CacheType.GeneralSettings);
                MemoryCache<LogSettings> logCache = (MemoryCache<LogSettings>)CacheTier.LookupCache(CacheType.LogLevel);
                MemoryCache<SecureEntity> secureEntityCache = (MemoryCache<SecureEntity>)CacheTier.LookupCache(CacheType.SecureEntity);
                MemoryCache<SecureEntity> secureUserAttributeCache = (MemoryCache<SecureEntity>)CacheTier.LookupCache(CacheType.SecureUserAttribute);
                MemoryCache<N1Relationship> n1RelationshipCache = (MemoryCache<N1Relationship>)CacheTier.LookupCache(CacheType.N1Relationship);
                MemoryCache<NNRelationship> nnRelationshipCache = (MemoryCache<NNRelationship>)CacheTier.LookupCache(CacheType.NNRelationship);
                MemoryCache<SystemUser> curLogInfoCache = (MemoryCache<SystemUser>)CacheTier.LookupCache(CacheType.LoginUserInfo);
                MemoryCache<List<Guid>> curUserTeamIDCache = (MemoryCache<List<Guid>>)CacheTier.LookupCache(CacheType.LoginUserTeamID);

                //initialize log
                LogSettings logSettings = logCache.Lookup(UniqueRecordName.LogSettingsName,
                    x => new LogSettings(Common.Enums.LogLevel.Error));

                //secureUserAttributes will not null always
                SecureEntity secureUserAttributes = secureUserAttributeCache.Lookup(SystemUser.EntityLogicalName,
                    x => CacheTier.CreateDefaultUserMetaDataEnforced());
                secureUserAttributeCache.AddOrUpdate(SystemUser.EntityLogicalName, secureUserAttributes);

                LogUserInfoReader logUserInfoReader = new LogUserInfoReader(service);
                logUserInfoReader.SetSecureUserEntityCache(secureUserAttributeCache);

                SystemUser curUser = curLogInfoCache.Lookup(context.InitiatingUserId.ToString(),
                    logUserInfoReader.RetrieveUserFromDCRMByID);

                if (logUserInfoReader.UserTeamIDs != null)
                {
                    curUserTeamIDCache.AddOrUpdate(context.InitiatingUserId.ToString(), logUserInfoReader.UserTeamIDs);
                }

                string curUserFullName = curUser == null ? "ID:" + context.InitiatingUserId.ToString() : curUser.FullName;

       double baseSettingsGotCost = DateTime.Now.Subtract(prevTime).TotalMilliseconds; //time cost
       prevTime = DateTime.Now; //timecost
                log = new NextLabsCRMLogs(tracingService, curUserFullName, logSettings.CurrentLevel);
       
                if (reason == RefreshReason.Init || reason == RefreshReason.Timeout)
                {
                    log.WriteLog(LogLevel.Debug, "Cache refreshed, for reason "+ reason.ToString());
                }

                //record error log if failed to refresh the cache from nxl_settings entity
                if (!string.IsNullOrEmpty(failedRefreshCacheMsg))
                {
                    string errmsg = "Failed to refresh cache.\n";
                    log.WriteLog(LogLevel.Error, errmsg + failedRefreshCacheMsg);
                    //failedRefreshCacheMsg = string.Empty;
                    throw new InvalidPluginExecutionException(errmsg);
                }

                //record error log if failed to get the current user, it is seldom happen, 
                //because if the current user is not existing in the cache, by default, plugin will access DCRM service to get again
                if (curUser == null)
                {
                    string errmsg = "Failed to get the information of the current user";
                    log.WriteLog(LogLevel.Error, errmsg);
                    throw new InvalidPluginExecutionException(errmsg);
                }
                /*
                log.WriteLog(LogLevel.Debug,
                    string.Format("Message {0}: retrive PrimaryEntityName {1} on event {4}, context.Depth {2}, thread id {3}.",
                    context.MessageName, context.PrimaryEntityName, context.Depth, Thread.CurrentThread.ManagedThreadId.ToString(), context.Mode));
                */
                generalSetting = generalSettingCache.Lookup(UniqueRecordName.GeneralSettingsName, GeneralSetting.CreateEmptyInstance);
                emCRMUserAction userAction = emCRMUserAction.emUNKNOWN_ACTION;
                GetCRMUserAction(context, ref userAction);
                log.WriteLog(LogLevel.Debug, string.Format("Analysis user action {0}", userAction.ToString()));

          double loginitCost2 = DateTime.Now.Subtract(prevTime).TotalMilliseconds; //time cost

          if (generalSetting.TimeCostEnable) {//timecost
                    log.WriteLog(LogLevel.Debug, "[timecost]service get:" + serviceRetriveCost +
                                 ";refresh cache:" + refreshCost + ";log init 1:" +
                                 loginitCost1 + ";basic setting get:" + baseSettingsGotCost + ";log init2:" + loginitCost2);//timecost
                }

          //dedicated for SPAction from sharepoint
                string primaryEntityName = context.PrimaryEntityName;

                if (context.PrimaryEntityName.Equals(Common.Constant.EntityName.NXLSPAction, StringComparison.OrdinalIgnoreCase) 
                    && userAction == emCRMUserAction.em_LIST_ACTION 
                    && IsPreEventMessage(context.Stage))
                {
                    object objQuery = context.InputParameters[Common.Constant.ParameterName.Query];
                    QueryExpression qe = (QueryExpression)objQuery;

                    spActionHelper = new SPActionHelper(qe.Criteria.Conditions);
                    if (spActionHelper.NeedDisguise)
                    {
                        userAction = spActionHelper.UserAction;
                        spActionHelper.Disguise(service, secureEntityCache);
                        primaryEntityName = spActionHelper.DisguisedEntity.LogicalName;
                    }

                    if(!spActionHelper.NeedDisguise)
                    {
#if DEBUG
                        //FetchExpression finalFe = Util.QueryExpressionToFetchExpression(service,
                        //context.InputParameters[Common.Constant.ParameterName.Query] as QueryExpression);
#endif
                        return;
                    }
                }

                TimeEscapeRecorder timeRecorder = new TimeEscapeRecorder(log, generalSetting.TimeCostEnable);
                List<Guid> teamIDs = curUserTeamIDCache.Lookup(context.InitiatingUserId.ToString(),x=>null);

                timeRecorder.RecordTime("isShared & isOwner attributes", "start");
                Dictionary <string, List<Guid>> sharedRecords = null;
                if (userAction == emCRMUserAction.em_CLICK_RECORD_ACTION ||
                    userAction == emCRMUserAction.em_EDIT_ACTION ||
                    userAction == emCRMUserAction.em_DELETE_ACTION||
                    (userAction==emCRMUserAction.em_LIST_ACTION
                        && IsPostEventMessage(context.Stage)
                        && !context.PrimaryEntityName.Equals(Common.Constant.EntityName.NXLSPAction, StringComparison.OrdinalIgnoreCase)))
                {
                    //for performance, to reduce the time cost on join operation for shared records by team and unnecessary cost on "isowner is true"
                    //when list view, the user teamid will and shared records will not be here.
                    
                    if (Util.IsIsOwnerEnabled(primaryEntityName, secureEntityCache) && teamIDs == null)
                    {
                        log.WriteLog(LogLevel.Debug, "Is owner is enabled.");
                        teamIDs = EntityDataHelp.GetUserTeamIDs(service, context.InitiatingUserId);
                        if(teamIDs == null || teamIDs.Count == 0)
                        {
                            throw new Exceptions.InvalidUserException(context.InitiatingUserId, @"NextLabs.CRMEnforcer.Plugin.EventHandler.Execute:
                                Any user must belong to one team at least");
                        }
                    }

                    if (Util.IsIsSharedEnabled(primaryEntityName, secureEntityCache))
                    {
                        log.WriteLog(LogLevel.Debug, "Is shared is enabled.");
                        sharedRecords = EntityDataHelp.GetSharedRecordIDs(service, secureEntityCache, context.InitiatingUserId);
                    }
                }
                timeRecorder.RecordTime("isShared & isOwner attributes", "end");

                if (IsPreEventMessage(context.Stage))
                {
                    timeRecorder.RecordTime("PreEvent", "start "+ userAction);
                    IEnforce enforceInstance = null;
                    switch (userAction)
                    {

                        case emCRMUserAction.em_CREATE_ACTION:
                            {
                                if (context.InputParameters.ContainsKey(Common.Constant.ParameterName.Target))
                                {
                                    if (context.InputParameters[Common.Constant.ParameterName.Target] is Entity)
                                    {
                                        enforceInstance = new Enforces.Create(service,
                                                                                                primaryEntityName,
                                                                                                curUser.Attributes,
                                                                                             secureUserAttributeCache,
                                                                                             secureEntityCache,
                                                                                             log,
                                                                                             context.InitiatingUserId,
                                                                                             generalSetting.DefaultDenyMessage,
                                                                                             context.InputParameters[Common.Constant.ParameterName.Target] as Entity,
                                                                                             generalSetting.userHostIP);
                                    }
                                }
                            }
                            break;
                        case emCRMUserAction.em_CLICK_RECORD_ACTION:

                            Guid currentRecordID = Guid.Empty;
                            ColumnSet column = null;
                            if (spActionHelper != null && spActionHelper.NeedDisguise)
                            {
                                currentRecordID = spActionHelper.DisguiseRecordID;
                                column = spActionHelper.DisguiseEntityColumn;

                                ConditionFactory conditionFactory = new ConditionFactory(secureEntityCache, service, context.InitiatingUserId, teamIDs, false, sharedRecords);
                                enforceInstance = new Enforces.Retrieve(service,
                                                                                            primaryEntityName,
                                                                                            curUser.Attributes,
                                                                                            secureUserAttributeCache,
                                                                                            secureEntityCache,
                                                                                            log,
                                                                                            context.InitiatingUserId,
                                                                                            n1RelationshipCache,
                                                                                            nnRelationshipCache,
                                                                                            generalSetting.DefaultDenyMessage,
                                                                                            currentRecordID,
                                                                                            column,
                                                                                            context.SharedVariables,
                                                                                            teamIDs,          //nullable
                                                                                            sharedRecords,
                                                                                            conditionFactory, //nullable
                                                                                            generalSetting.userHostIP);

                            }
                            else
                            {
                                if (context.InputParameters.ContainsKey(Common.Constant.ParameterName.Target))
                                {
                                    
                                    if (context.InputParameters[Common.Constant.ParameterName.Target] is EntityReference)
                                    {
                                        EntityReference entityRefe = (EntityReference)context.InputParameters[Common.Constant.ParameterName.Target];
                                        currentRecordID = entityRefe.Id;
                                    }
                                    else if (context.InputParameters[Common.Constant.ParameterName.Target] is Entity)
                                    {
                                        Microsoft.Xrm.Sdk.Entity entityRefe = (Microsoft.Xrm.Sdk.Entity)context.InputParameters[Common.Constant.ParameterName.Target];
                                        currentRecordID = entityRefe.Id;
                                    }
                                    else
                                    {
                                        throw new Exceptions.ParameterNotFoundException(Common.Constant.ParameterName.Target, context.MessageName, primaryEntityName);
                                    }

                                    if (context.InputParameters.ContainsKey(Common.Constant.ParameterName.ColumnSet) && context.InputParameters[Common.Constant.ParameterName.ColumnSet] is ColumnSet)
                                    {
                                        column = context.InputParameters[Common.Constant.ParameterName.ColumnSet] as ColumnSet;
                                        ConditionFactory conditionFactory = new ConditionFactory(secureEntityCache, service, context.InitiatingUserId, teamIDs, false, sharedRecords);
                                        enforceInstance = new Enforces.Retrieve(service,
                                                                                                    primaryEntityName,
                                                                                                    curUser.Attributes,
                                                                                                    secureUserAttributeCache,
                                                                                                    secureEntityCache,
                                                                                                    log,
                                                                                                    context.InitiatingUserId,
                                                                                                    n1RelationshipCache,
                                                                                                    nnRelationshipCache,
                                                                                                    generalSetting.DefaultDenyMessage,
                                                                                                    currentRecordID,
                                                                                                    column,
                                                                                                    context.SharedVariables,
                                                                                                    teamIDs,          //nullable
                                                                                                    sharedRecords,
                                                                                                    conditionFactory, //nullable
                                                                                                    generalSetting.userHostIP);
                                    }
                                }
                            }
                            break;
                        case emCRMUserAction.em_EDIT_ACTION:
                            if (spActionHelper != null && spActionHelper.NeedDisguise)
                            {
                                ConditionFactory conditionFactory = new ConditionFactory(secureEntityCache, service, context.InitiatingUserId, teamIDs, false, sharedRecords);
                                Entity entity = spActionHelper.DisguisedEntity;
                                enforceInstance = new Enforces.Update(  service,
                                                                        primaryEntityName,
                                                                        curUser.Attributes,
                                                                        secureUserAttributeCache,
                                                                        secureEntityCache,
                                                                        log, 
                                                                        context.InitiatingUserId,
                                                                        n1RelationshipCache,
                                                                        nnRelationshipCache,
                                                                        generalSetting.DefaultDenyMessage,
                                                                        entity,
                                                                        context.SharedVariables,
                                                                        teamIDs,          //nullable
                                                                        sharedRecords,
                                                                        conditionFactory, //nullable
                                                                        generalSetting.userHostIP);
                            }
                            else
                            {
                                if (context.InputParameters.ContainsKey(Common.Constant.ParameterName.Target) && context.InputParameters[Common.Constant.ParameterName.Target] is Entity)
                                {
                                    ConditionFactory conditionFactory = new ConditionFactory(secureEntityCache, service, context.InitiatingUserId, teamIDs, false, sharedRecords);
                                    Entity entityRefe = (Entity)context.InputParameters[Common.Constant.ParameterName.Target];
                                    enforceInstance = new Enforces.Update(service,
                                                                                        primaryEntityName,
                                                                                           curUser.Attributes,
                                                                                            secureUserAttributeCache,
                                                                                            secureEntityCache,
                                                                                            log, context.InitiatingUserId,
                                                                                            n1RelationshipCache,
                                                                                            nnRelationshipCache,
                                                                                            generalSetting.DefaultDenyMessage,
                                                                                            entityRefe,
                                                                                            context.SharedVariables,
                                                                                            teamIDs,          //nullable
                                                                                            sharedRecords,
                                                                                            conditionFactory, //nullable
                                                                                            generalSetting.userHostIP);
                                }
                                else
                                {
                                    throw new Exceptions.ParameterNotFoundException(Common.Constant.ParameterName.Target, context.MessageName, primaryEntityName);
                                }
                            }
                            break;
                        case emCRMUserAction.em_DELETE_ACTION:
                            {
                                if (context.InputParameters.ContainsKey(Common.Constant.ParameterName.Target) && context.InputParameters[Common.Constant.ParameterName.Target] is EntityReference)
                                {
                                    ConditionFactory conditionFactory = new ConditionFactory(secureEntityCache, service, context.InitiatingUserId, teamIDs, false, sharedRecords);
                                    EntityReference entityRefe = (EntityReference)context.InputParameters[Common.Constant.ParameterName.Target];
                                    enforceInstance = new Enforces.Delete(service,
                                                                                        primaryEntityName,
                                                                                           curUser.Attributes,
                                                                                            secureUserAttributeCache,
                                                                                            secureEntityCache,
                                                                                            log, context.InitiatingUserId,
                                                                                            n1RelationshipCache,
                                                                                            nnRelationshipCache,
                                                                                            generalSetting.DefaultDenyMessage,
                                                                                            entityRefe,
                                                                                            context.SharedVariables,
                                                                                            teamIDs,           //nullable
                                                                                            sharedRecords,  //nullable
                                                                                            conditionFactory,
                                                                                            generalSetting.userHostIP); 
                                }
                                else
                                {
                                    throw new Exceptions.ParameterNotFoundException(Common.Constant.ParameterName.Target, context.MessageName, primaryEntityName);
                                }
                                break;
                            }
                        case emCRMUserAction.em_LIST_ACTION:
                            if (context.InputParameters.ContainsKey(Common.Constant.ParameterName.Query))
                            {
                                //bug 47560, in an entity's list view which display in another entity's form view
                                //the list view does not contain id which is critical for our logic of mask field
                                //this case is not existing on all the enviroment, just occurred in an online enviroment.
                                //here add the id if the id not exist in the column collection
                                object objQuery = context.InputParameters[Common.Constant.ParameterName.Query];
                                SecureEntity entityMetadata = secureEntityCache.Lookup(context.PrimaryEntityName, x => null);
                                if(entityMetadata == null)
                                {
                                    throw new Exception(string.Format("cannot find the meta data of the entity {0}", context.PrimaryEntityName));
                                }
                                if (objQuery is FetchExpression)
                                {
                                    //log.WriteLog(LogLevel.Debug, "IN objQuery is FetchExpression");
                                    FetchExpression fe = (FetchExpression)objQuery;
                                    string idAttr = "<attribute name=\"" + entityMetadata.Schema.PrimaryIDName.ToLower() + "\" />";
                                    if (!fe.Query.Contains(idAttr))
                                    {
                                        int insertIndex = fe.Query.IndexOf("<attribute name=");
                                        fe.Query.Insert(insertIndex, idAttr);
                                    }
                                    log.WriteLog(LogLevel.Debug, string.Format("fetch expression in query is {0}", fe.Query));
                                }
                                else if (objQuery is QueryExpression)
                                {
                                    //log.WriteLog(LogLevel.Debug, "IN objQuery is QueryExpression");
                                    QueryExpression qe = (QueryExpression)objQuery;
                                    string idColumn = entityMetadata.Schema.PrimaryIDName.ToLower();
                                    if (!qe.ColumnSet.Columns.Contains(idColumn))
                                    {
                    
                                        qe.ColumnSet.Columns.Add(idColumn);
                                    }

                                    //FetchExpression fe = Util.QueryExpressionToFetchExpression(service, qe);
                                    //log.WriteLog(LogLevel.Debug, string.Format("query expression in query is {0}", fe.Query));
                                }

                                ConditionFactory conditionFactory = new ConditionFactory(secureEntityCache, service, context.InitiatingUserId, teamIDs, true, sharedRecords);
                                enforceInstance = new Enforces.RetrieveMultiple(service,
                                                                                                    primaryEntityName,
                                                                                                    curUser.Attributes,
                                                                                                    secureUserAttributeCache,
                                                                                                    secureEntityCache,
                                                                                                    log,
                                                                                                    context.InitiatingUserId,
                                                                                                    n1RelationshipCache,
                                                                                                    nnRelationshipCache,
                                                                                                    context.InputParameters[Common.Constant.ParameterName.Query],
                                                                                                    generalSetting.DefaultDenyMessage,
                                                                                                    context.SharedVariables,
                                                                                                    null,
                                                                                                    null,
                                                                                                    conditionFactory,
                                                                                                    generalSetting.userHostIP);
                            }
                            else
                            {
                                throw new Exceptions.ParameterNotFoundException(Common.Constant.ParameterName.Query, context.MessageName, primaryEntityName);
                            }

                            break;
                        default:
                            break;
                    }
                    EnforceResult result = enforceInstance.DoEnforce(false);
                    if (spActionHelper != null && spActionHelper.NeedDisguise)
                    {
                        spActionHelper.Decision = result.Decision;
                        if (!context.SharedVariables.Contains(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key))
                        {
                            context.SharedVariables.Add(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key,
                                spActionHelper.GetResult());
                        }
                        else
                        {
                            context.SharedVariables[Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key] =
                            spActionHelper.GetResult();
                        }
                    }
                    else
                    {
                        enforceInstance.ExecuteEnforceResult();
                    }
#if DEBUG
                    //FetchExpression finalFe = Util.QueryExpressionToFetchExpression(service,
                    //    context.InputParameters[Common.Constant.ParameterName.Query] as QueryExpression);
#endif
                    timeRecorder.RecordTime("PreEvent", "end");
                    
                }
                else if (IsPostEventMessage(context.Stage))
                {
                    timeRecorder.RecordTime("PostEvent", "start");

                    //this code only for chart
                    if (context.PrimaryEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.Savedqueryvisualization, StringComparison.OrdinalIgnoreCase)
                        || context.PrimaryEntityName.Equals(NextLabs.CRMEnforcer.Common.Constant.EntityName.Userqueryvisualization))
                    {
                        if (context.MessageName.Equals(NextLabs.CRMEnforcer.Common.Constant.MessageName.Retrieve, StringComparison.OrdinalIgnoreCase))
                        {
                            PostEventForRetriverSavedqueryvisualization(context,
                                                                                               service,
                                                                                               generalSetting.DefaultDenyMessage,
                                                                                               curUser.Attributes,
                                                                                               secureUserAttributeCache,
                                                                                               secureEntityCache,
                                                                                               n1RelationshipCache,
                                                                                               nnRelationshipCache,
                                                                                               context.SharedVariables,
                                                                                               teamIDs,
                                                                                               sharedRecords,
                                                                                               log,
                                                                                               generalSetting.userHostIP
                                                                                               );
                        }
                        else if (context.MessageName.Equals(NextLabs.CRMEnforcer.Common.Constant.MessageName.Retrievemultiple, StringComparison.OrdinalIgnoreCase))
                        {
                            PostEventForRetrievrMuiltSavedqueryvisualzation(context,
                                                                                               service,
                                                                                               generalSetting.DefaultDenyMessage,
                                                                                               curUser.Attributes,
                                                                                               secureUserAttributeCache,
                                                                                               secureEntityCache,
                                                                                               n1RelationshipCache,
                                                                                               nnRelationshipCache,
                                                                                               context.SharedVariables,
                                                                                               teamIDs,
                                                                                               sharedRecords,
                                                                                               log,
                                                                                               generalSetting.userHostIP);
                        }
                    }
                    else
                    {
                        //v1.2
                        if (context.SharedVariables.Contains(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key)
                            && context.PrimaryEntityName.Equals(Common.Constant.EntityName.NXLSPAction,StringComparison.OrdinalIgnoreCase)
                            && userAction == emCRMUserAction.em_LIST_ACTION
                            && context.OutputParameters.Contains(Common.Constant.ParameterName.BusinessEntityCollection)
                            && context.OutputParameters[Common.Constant.ParameterName.BusinessEntityCollection] is EntityCollection)
                        {
                            
                            EntityCollection spActions = 
                                context.OutputParameters[Common.Constant.ParameterName.BusinessEntityCollection] as EntityCollection;
                            if (spActions.Entities != null && spActions.Entities.Count > 0)
                            {
                                Entity spAction = spActions.Entities[0];
                                IStringSerialize xmlSerializer = new XMLSerializeHelper();
                                SPActionResult result = (SPActionResult)xmlSerializer.Deserialize(typeof(SPActionResult),
                                        (string)context.SharedVariables[Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key]);
                                spAction.Attributes[Common.Constant.AttributeKeyName.NXL_SPAction_Decision] = (int)result.Decision;
                                spAction.Attributes[Common.Constant.AttributeKeyName.NXL_SPAction_ErrorMessage] = result.ErrorMessage;
                            }
                        } 

                        if (context.SharedVariables.Contains(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_Key))
                        {
                            switch (userAction)
                            {
                                case emCRMUserAction.em_LIST_ACTION:
                                    if (context.OutputParameters.Contains(Common.Constant.ParameterName.BusinessEntityCollection) 
                                        && context.OutputParameters[Common.Constant.ParameterName.BusinessEntityCollection] is EntityCollection)
                                    {
                                        EnforceResult enforcerResult = EnforceResult.LoadFromJson(context.SharedVariables[Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_Key].ToString());
                                        if (enforcerResult != null)
                                        {
                                            List<string> lisColumn = EntityDataHelp.GetEntityColumn(context.InputParameters[Common.Constant.ParameterName.Query]);
                                            DoListAction(service, 
                                                enforcerResult, 
												lisColumn.ToArray(),
                                                context.OutputParameters[Common.Constant.ParameterName.BusinessEntityCollection] as EntityCollection, 
                                                secureEntityCache, context.PrimaryEntityName,
                                                context.InitiatingUserId,
                                                teamIDs,
                                                sharedRecords,
                                                log);
                                        }
                                        else
                                        {
                                            log.WriteLog(LogLevel.Error, 
                                                "Cannot serialize JSON to object , cannot do obligation on post event . JSON detail:" + context.SharedVariables[Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_Key].ToString());
                                        }
                                    }
                                    else
                                    {
                                        log.WriteLog(LogLevel.Warning, "Cannot find BusinessEntityCollection on output parameters , cannot do obligation on post event");
                                    }

                                    break;
                                case emCRMUserAction.em_CLICK_RECORD_ACTION:
                                    {
                                        if (context.OutputParameters.Contains(Common.Constant.ParameterName.BusinessEntity) && context.OutputParameters[Common.Constant.ParameterName.BusinessEntity] is Entity)
                                        {
                                            Entity entityDisplay = context.OutputParameters[Common.Constant.ParameterName.BusinessEntity] as Entity;
                                            Common.DataModel.Share.ExcuteObligationResult excuteObligationResult = Common.DataModel.Share.ExcuteObligationResult.LoadFromJson(context.SharedVariables[Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_Key].ToString());
                                            if (excuteObligationResult != null)
                                            {
                                                string[] arrayColumn = (context.InputParameters[Common.Constant.ParameterName.ColumnSet] as ColumnSet).Columns.ToArray();
                                                if(arrayColumn.Length==0)
                                                {
                                                    //this code only for mobile, it column count is 0, so we need get column from attribute
                                                    arrayColumn = new string[entityDisplay.Attributes.Count];
                                                    int nIndex = 0;
                                                    foreach(var item in entityDisplay.Attributes)
                                                    {
                                                        arrayColumn[nIndex] = item.Key;
                                                        nIndex++;
                                                    }
                                                }
                                                MaskFieldHandles maskFieldHandle = new MaskFieldHandles(context.PrimaryEntityName, log);
                                                SecureEntity currentEntityStruct = secureEntityCache.Lookup(context.PrimaryEntityName, x => null);
                                                maskFieldHandle.DoObligation(excuteObligationResult.MaskFields, arrayColumn, entityDisplay.Attributes, currentEntityStruct);
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            log.WriteLog(LogLevel.Debug, "Cannot find shared variable on context , ignore do obligation on post event");
                        }
                    }

                    timeRecorder.RecordTime("PostEvent", "end");
                }
            }
            catch (InvalidPluginExecutionException exp)
            {
                DateTime tryCostEnd = DateTime.Now;//time cost
                log.WriteLog(LogLevel.Debug, "[timecost]time cost when InvalidPluginExecutionException occurr:" + tryCostEnd.Subtract(tryCostbegin));//time cost

                log.WriteLog(LogLevel.Error, string.Format("An exception occurred in the NextLabsCRMMsg plug-in:{0}", exp.Message));
                throw (exp);
            }
            catch (Exception ex)
            {
                DateTime tryCostEnd = DateTime.Now;//time cost
                log.WriteLog(LogLevel.Debug, "[timecost]time cost when Exception occurr:" + tryCostEnd.Subtract(tryCostbegin));//time cost

                if (ex.Message.StartsWith("Cache detail:"))
                {
                    log.WriteLog(LogLevel.Error, string.Format("An exception occurred in the 'NextLabsCRMMsg' plug-in. It may be caused by some attribute is not secured. \r\nError message is {0} {1}", ex.Message, ex.StackTrace));
                }
                else
                {
                    log.WriteLog(LogLevel.Error, string.Format("An exception occurred in the 'NextLabsCRMMsg' plug-in. Error message is {0} {1}", ex.Message, ex.StackTrace));
                }                

                if(spActionHelper != null && spActionHelper.NeedDisguise)
                {
                    spActionHelper.Decision = CEResponse.CEDeny;
                    spActionHelper.ExceptionMessage = ex.Message;
                    if (!context.SharedVariables.Contains(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key))
                    {
                        context.SharedVariables.Add(Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key,
                            spActionHelper.GetResult());
                    }
                    else
                    {
                        context.SharedVariables[Common.Constant.AttributeKeyName.Nextlabs_ShareVariable_SPAction_Key] =
                        spActionHelper.GetResult();
                    }
                    return;
                }
                if (generalSetting == null)
                {
                    throw new InvalidPluginExecutionException(Common.Constant.CacheControl.DefaultExceptionMessage);
                }
                else
                {
                    if (generalSetting.PolicyDecision != PolicyDecision.Allow)
                    {
                        throw new InvalidPluginExecutionException(generalSetting.SystemErrorMessage);
                    }
                }
            }
        }
    }
}
