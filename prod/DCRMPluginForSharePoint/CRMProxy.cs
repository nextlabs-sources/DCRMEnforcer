
using Microsoft.Xrm.Sdk.Client;
using System;
using System.ServiceModel.Description;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Diagnostics;

namespace DCRMPluginForSharePoint
{
    public class CRMProxy
    {
        private OrganizationServiceProxy m_serviceProxy;
        private OrganizationServiceContext m_orgContext;
        public CRMProxy()
        {
        }

        public void Init(   Uri orgUri,
                            Uri homeRealUri,
                            ClientCredentials clientCredentials,
                            ClientCredentials deviceCredentials)
        {
            m_serviceProxy = new OrganizationServiceProxy(orgUri,
                                                            homeRealUri,
                                                            clientCredentials,
                                                            deviceCredentials);
            m_serviceProxy.EnableProxyTypes();
            m_orgContext = new OrganizationServiceContext(m_serviceProxy);
        }

        public void Uninit()
        {
            m_orgContext.Dispose();
            m_serviceProxy.Dispose();
        }

        public Guid QueryUserIDByEmail(string emailaddress)
        {

            Guid userId = (from user in m_orgContext.CreateQuery<SystemUser>()
                           where user.InternalEMailAddress == emailaddress
                           select user.SystemUserId.Value).FirstOrDefault();
            return userId;
            
        }

        public Guid QueryUserIDByDomainName(string domainName)
        {

            Guid userId = (from user in m_orgContext.CreateQuery<SystemUser>()
                           where user.DomainName == domainName
                           select user.SystemUserId.Value).FirstOrDefault();
            return userId;

        }

        private int OnAction(string action, Guid userId, string path, string site)
        {
            m_serviceProxy.CallerId = userId;

            QueryExpression qe = new QueryExpression();
            qe.NoLock = true;
            qe.ColumnSet = new ColumnSet();
            qe.ColumnSet.AddColumn("nxl_spactionid");
            qe.ColumnSet.AddColumn("nxl_name");
            qe.ColumnSet.AddColumn("nxl_decision");
            qe.ColumnSet.AddColumn("nxl_errormessage");

            qe.EntityName = "nxl_spaction";
            qe.Criteria = new FilterExpression(LogicalOperator.And);
            qe.Criteria.AddCondition(new ConditionExpression("nxl_name", ConditionOperator.Equal, action));
            qe.Criteria.AddCondition(new ConditionExpression("nxl_folderpath", ConditionOperator.Equal, path));
            qe.Criteria.AddCondition(new ConditionExpression("nxl_site", ConditionOperator.Equal, site));

            DataCollection<Entity> entities = m_serviceProxy.RetrieveMultiple(qe).Entities;
            if (entities.Count == 0)
            {
                Trace.WriteLine(string.Format("Cannot find any record about {0} for nxl_spaction entity", action));
                return 2;
            }
            Entity spAction = m_serviceProxy.RetrieveMultiple(qe).Entities[0];
            int decision = spAction.GetAttributeValue<int>("nxl_decision");
            string errorMsg = spAction.GetAttributeValue<string>("nxl_errormessage");

            return decision;
        }

        public int OnViewAction(Guid userId, string path, string site)
        {
            return OnAction("view", userId, path, site);
        }

        public int OnUpdateAction(Guid userId, string path, string site)
        {
            return OnAction("update", userId, path, site);
        }

        public int OnCheckinAction(Guid userId, string path, string site)
        {
            return OnAction("checkin", userId, path, site);
        }

        public int OnCheckoutAction(Guid userId, string path, string site)
        {
            return OnAction("checkout", userId, path, site);
        }

        public int OnDeleteAction(Guid userId, string path, string site)
        {
            return OnAction("delete", userId, path, site);
        }

        public int OnPropertyUpdateAction(Guid userId, string path, string site)
        {
            return OnAction("update_property", userId, path, site);
        }
    }
}

