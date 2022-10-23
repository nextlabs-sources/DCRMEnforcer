using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataBussiness.Serialization;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Sharepoint
{
    public class SPActionHelper
    {
        [XmlRoot("result")]
        public class SPActionResult
        {
            [XmlElement("decision")]
            public int Decision
            {
                get;set;
            }

            [XmlElement("errormessage")]
            public string ErrorMessage
            {
                get;set;
            }
        }
        public class SPLocationBranch
        {
            private List<Entity> m_datalist = new List<Entity>();
            private Entity m_base;
            private Entity m_top;
            private Guid m_topUpperId;
            private bool m_isFinish;
            
            public SPLocationBranch(Entity baseNode)
            {
                m_base = baseNode;
                m_top = baseNode;
                m_datalist.Add(baseNode);

                m_topUpperId = GetTopUpperGuid();
            }

            

            public Entity BaseNode
            {
                get { return m_base; }
            }
            public void Append(Entity data)
            {
                if(m_isFinish)
                {
                    return;
                }

                if (m_topUpperId.Equals(data.Id))
                {
                    m_datalist.Add(data);
                    m_top = data;
                    m_topUpperId = GetTopUpperGuid();
                    m_isFinish = IsFinish();
                }         
            }

            public bool IsMatch(string[] path)
            {
                int count = path.Count();
                if (path == null || count <= 0 || count != m_datalist.Count())
                {
                    return false;
                }

                var nodes = m_datalist.ToArray().Reverse().ToArray();
                for (int index = count -1;index >=0; index--)
                {
                    if(!nodes[index].GetAttributeValue<string>("relativeurl").Equals(path[index],StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }
            public List<Entity> DataList
            {
                get { return m_datalist; }
            }
        
            private Guid GetTopUpperGuid()
            {
                EntityReference parentSiteOrLocation = m_top.GetAttributeValue<EntityReference>("parentsiteorlocation");
                if(parentSiteOrLocation == null)
                {
                    return Guid.Empty;
                }
                if(parentSiteOrLocation.Id == null || parentSiteOrLocation.Id.Equals(Guid.Empty))
                {
                    return Guid.Empty;
                }

                return parentSiteOrLocation.Id;
            }

            private bool IsFinish()
            {
                EntityReference parentSiteOrLocation = m_top.GetAttributeValue<EntityReference>("parentsiteorlocation");
                return parentSiteOrLocation.Id.
                    Equals(m_top.GetAttributeValue<Guid>("sitecollectionid"));
 
            }

            public bool Complete
            {
                get { return m_isFinish; }
            }
        }

        private string m_folderPath;
        private string m_site;
        private emCRMUserAction m_userAction;
        private Entity m_disguisedEntity;
        private ColumnSet m_columnSet;
        public SPActionHelper(AttributeCollection keyAttributes)
        {
            string action = (string)keyAttributes[Constant.AttributeKeyName.NXL_SPAction_Name];
            NeedDisguise = true;
            switch (action.ToLower())
            {
                case "new":
                case "update":
                case "checkin":
                case "checkout":
                case "delete":
                case "update_property":
                    m_userAction = emCRMUserAction.em_EDIT_ACTION;
                    break;
                case "view":
                    m_userAction = emCRMUserAction.em_CLICK_RECORD_ACTION;
                    break;
                default:
                    NeedDisguise = false;
                    break;
            }

            m_folderPath = (string)keyAttributes[Constant.AttributeKeyName.NXL_SPAction_FolderPath];
            m_site = (string)keyAttributes[Constant.AttributeKeyName.NXL_SPAction_Site];
        }

        public SPActionHelper(DataCollection<ConditionExpression> conditions)
        {
            string action="";
            int count = conditions.Count;
            for (int index = count -1;index >=0;index--)
            {
                ConditionExpression condition = conditions[index];
                if (condition.AttributeName.Equals(Constant.AttributeKeyName.NXL_SPAction_Name, 
                    StringComparison.OrdinalIgnoreCase))
                {
                    action = (string)condition.Values[0];
                }
                else if(condition.AttributeName.Equals(Constant.AttributeKeyName.NXL_SPAction_FolderPath, StringComparison.OrdinalIgnoreCase))
                {
                    m_folderPath = (string)condition.Values[0];
                    conditions.RemoveAt(index);
                }
                else if(condition.AttributeName.Equals(Constant.AttributeKeyName.NXL_SPAction_Site, StringComparison.OrdinalIgnoreCase))
                {
                    m_site = (string)condition.Values[0];
                    conditions.RemoveAt(index);
                }
            }
            
            NeedDisguise = true;
            switch (action)
            {
                case "new":
                case "update":
                case "checkin":
                case "checkout":
                case "delete":
                case "update_property":
                    m_userAction = emCRMUserAction.em_EDIT_ACTION;
                    break;
                case "view":
                    m_userAction = emCRMUserAction.em_CLICK_RECORD_ACTION;
                    break;
                default:
                    NeedDisguise = false;
                    break;
            }

            if(string.IsNullOrEmpty(m_folderPath) ||
                string.IsNullOrEmpty(m_site))
            {
                NeedDisguise = false;
            }
        }
        public bool NeedDisguise
        {
            get;set;
        }
        public string ExceptionMessage
        {
            get; set;
        }
        
        public emCRMUserAction UserAction
        {
            get { return m_userAction; }
        }

        public JavaPC.RestAPISDK.CEModel.CEResponse Decision
        {
            get;set;
        }
        public Entity DisguisedEntity
        {
            get { return m_disguisedEntity; }
        }

        public Guid DisguiseRecordID
        {
            get { return m_disguisedEntity.Id; }
        }

        public ColumnSet DisguiseEntityColumn
        {
            get { return m_columnSet; }
        }
        public Entity Disguise(IOrganizationService service, MemoryCache<DataModel.SecureEntity> secureEntityCache)
        {
            Entity location = GetRegardingRecordKeyValue(service, m_folderPath);
            if (null == location)
            {
                throw new Exception(string.Format("cannot find the location of the sharepoint document folder path {0}", m_folderPath));
            }
            EntityReference regardingObj = location.GetAttributeValue<EntityReference>("regardingobjectid");
            string entityName = regardingObj.LogicalName;
            DataModel.SecureEntity enforcedEntity = secureEntityCache.Lookup(entityName, x => null);
            if(enforcedEntity == null)
            {
                throw new Exception(string.Format("cannot find the regarding entity of the sharepoint document folder path {0}", m_folderPath));
            }

            m_columnSet = new ColumnSet();
            foreach(DataModel.MetaData.Attribute attr in enforcedEntity.Schema.Attributes)
            {
                if(attr.LogicName.Equals(Constant.AttributeKeyName.NXL_ISOwner,StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (attr.LogicName.Equals(Constant.AttributeKeyName.NXL_ISShared, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                m_columnSet.AddColumn(attr.LogicName);
            }
            m_disguisedEntity = GetRegardingRecord(service, regardingObj.LogicalName,regardingObj.Id, m_columnSet);

            if(m_disguisedEntity == null)
            {
                throw new Exception(string.Format("cannot find the regarding record of the sharepoint document folder path {0}", m_folderPath));
            }

            return m_disguisedEntity;
        }

        public string GetResult()
        {
            SPActionResult result = new SPActionResult();
            result.Decision = (int)Decision;
            result.ErrorMessage = ExceptionMessage;
            IStringSerialize xmlSerializer = new XMLSerializeHelper();
            return xmlSerializer.Serialize(typeof(SPActionResult), result);

        }
        private string[] ParsePath(string path)
        {
            return path.Split(new char[] {'/'});
        }

        private EntityCollection GetAllLocationsFromService(IOrganizationService service, string[] relativeUrls)
        {
            QueryExpression qe = new QueryExpression();
            qe.ColumnSet = new ColumnSet(
                new string[] { "sharepointdocumentlocationid",
                    "parentsiteorlocation",
                    "relativeurl",
                    "sitecollectionid",
                    "regardingobjectid",
                    "regardingobjecttypecode" });

            qe.Criteria = new FilterExpression(LogicalOperator.And);
            qe.EntityName = "sharepointdocumentlocation";
            qe.Criteria.Conditions.Add(new ConditionExpression("relativeurl", ConditionOperator.In, relativeUrls));

            LinkEntity linkSite = new LinkEntity("sharepointdocumentlocation",
                                                    "sharepointsite",
                                                    "sitecollectionid",
                                                    "sharepointsiteid",
                                                    JoinOperator.Inner);
            linkSite.LinkCriteria = new FilterExpression(LogicalOperator.And);
            linkSite.LinkCriteria.AddCondition(new ConditionExpression("sharepointsite","absoluteurl", ConditionOperator.Equal, m_site));
            //linkSite.Columns.AddColumn("sitecollectionid");
            qe.LinkEntities.Add(linkSite);

            EntityCollection locations = EntityDataHelp.RetrieveMultiple(service, qe);

            return locations;
        }

        private Entity GetRegardingRecordKeyValue(IOrganizationService service, string path)
        {
            string[] relativeUrls = ParsePath(path);
            if (relativeUrls.Length < 2)
            {
                return null;
            }

            EntityCollection locations = GetAllLocationsFromService(service, relativeUrls);
            List<Entity> locationArray = locations.Entities.ToList();

            Dictionary<Guid, SPLocationBranch> nodes = new Dictionary<Guid, SPLocationBranch>();

            //init branch
            for (int index = locationArray.Count - 1; index >= 0; index--)
            {
                Entity location = locationArray[index];
                string url = location.GetAttributeValue<string>("relativeurl");
                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                if (url.Equals(relativeUrls[relativeUrls.Length - 1], StringComparison.OrdinalIgnoreCase))
                {
                    SPLocationBranch branch = new SPLocationBranch(location);
                    nodes.Add(location.Id, branch);
                    locationArray.RemoveAt(index);
                }
            }

            //fill branch
            while (true)
            {
                bool allGetTopNode = true;
                foreach (SPLocationBranch branch in nodes.Values)
                {
                    if (branch != null)
                    {                       
                        if(!branch.Complete)
                        {
                            allGetTopNode = false;
                        }
                    }
                }

                if (allGetTopNode)
                {
                    break;
                }

                for (int index = 0; index < locationArray.Count; index++)
                {
                    foreach(SPLocationBranch branch in nodes.Values)
                    {
                        branch.Append(locationArray[index]);
                    }
                }
            }

            foreach(KeyValuePair<Guid,SPLocationBranch> item in nodes)
            {
                if(item.Value != null && item.Value.IsMatch(relativeUrls))
                {
                    return item.Value.BaseNode;
                }
            }
            return null;
        }

        private Microsoft.Xrm.Sdk.Entity GetRegardingRecord(IOrganizationService service, string entityName,Guid id, ColumnSet colset)
        {
            return EntityDataHelp.RertieveSingleEntity(service, entityName, id, colset);
        }
    }
}
