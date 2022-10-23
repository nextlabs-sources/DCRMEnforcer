using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.DataModel.MetaData;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.IO
{
    class EntityMetaDataReader
    {
        private IOrganizationService m_service = null;

        public EntityMetaDataReader(IOrganizationService service)
        {
            m_service = service;
        }

        private RetrieveEntityRequest CreateRetrieveEntityRequest(string entityLogicName)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.Attributes,
                RetrieveAsIfPublished = true,
                LogicalName = entityLogicName,
            };

            return request;
        }

        private ExecuteMultipleRequest CreateExecuteMultipleRequest()
        {
            ExecuteMultipleRequest requestWithResults = new ExecuteMultipleRequest()
            {
                // Assign settings that define execution behavior: continue on error, return responses. 
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = false,
                    ReturnResponses = true
                },
                // Create an empty organization request collection.
                Requests = new OrganizationRequestCollection()
            };

            return requestWithResults;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">entity logic name</param>
        /// <returns></returns>
        public DataModel.MetaData.Entity Read(string entityLogicName)
        {
            if (m_service == null)
            {
                return null;
            }

            if(string.IsNullOrWhiteSpace(entityLogicName))
            {
                return null;
            }

            RetrieveEntityRequest request = CreateRetrieveEntityRequest(entityLogicName);

            RetrieveEntityResponse response = null;


            try
            {
                response = (RetrieveEntityResponse)m_service.Execute(request);
            }catch(Exception e)
            {
                return null;
            }
            if (response == null)
            {
                return null;
            }

            if (response.EntityMetadata == null)
            {
                return null;
            }
            if (response.EntityMetadata.Attributes == null)
            {
                return null;
            }

            DataModel.MetaData.Entity bizEntity = 
                MetaDataConverter.CovertDCRMEntity2BizMetaDataEntity(response.EntityMetadata);

            return bizEntity;
        }

        public List<DataModel.MetaData.Entity> Read(List<string> entityLogicNames)
        {
            if (m_service == null)
            {
                return null;
            }

            if(entityLogicNames == null || entityLogicNames.Count == 0)
            {
                return null;
            }

            ExecuteMultipleRequest requestWithResults = CreateExecuteMultipleRequest();

            foreach (string entityLogicName in entityLogicNames)
            {
                RetrieveEntityRequest request = CreateRetrieveEntityRequest(entityLogicName);
                requestWithResults.Requests.Add(request);
            }

            ExecuteMultipleResponse responseWithResults =
                (ExecuteMultipleResponse)m_service.Execute(requestWithResults);

            List<DataModel.MetaData.Entity> enities = 
                new List<DataModel.MetaData.Entity>(requestWithResults.Requests.Count);


            foreach (var responseItem in responseWithResults.Responses)
            {
                int requestIndex = responseItem.RequestIndex;
                // A valid response.
                if (responseItem.Response != null)
                {
                    RetrieveEntityResponse response = (RetrieveEntityResponse)responseItem.Response;
                    DataModel.MetaData.Entity entity =
                        MetaDataConverter.CovertDCRMEntity2BizMetaDataEntity(response.EntityMetadata);
                    enities[requestIndex] = entity;
                }
                // An error has occurred.
                else if (responseItem.Fault != null)
                {
                    //need log
                    //DisplayFault(requestWithResults.Requests[responseItem.RequestIndex],
                    //responseItem.RequestIndex, responseItem.Fault);
                    enities.Clear();
                    break;
                }
            }

            return enities;
        }
    }
}
