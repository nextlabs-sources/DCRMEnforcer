using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    public class Util
    {
        public static QueryExpression FetchExpressionToQueryExpression(IOrganizationService service, string fetchXml)
        {

            // Convert the FetchXML into a query expression.
            var conversionRequest = new FetchXmlToQueryExpressionRequest
            {
                FetchXml = fetchXml
            };

            var conversionResponse = (FetchXmlToQueryExpressionResponse)service.Execute(conversionRequest);

            // Use the newly converted query expression to make a retrieve multiple
            // request to Microsoft Dynamics CRM.
            QueryExpression queryExpression = conversionResponse.Query;

            return queryExpression;
        }

        static public FetchExpression QueryExpressionToFetchExpression(IOrganizationService service, QueryExpression qe)
        {

            // Convert the query expression to FetchXML.
            var conversionRequest = new QueryExpressionToFetchXmlRequest
            {
                Query = qe
            };
            var conversionResponse = (QueryExpressionToFetchXmlResponse)service.Execute(conversionRequest);

            // Use the converted query to make a retrieve multiple request to Microsoft Dynamics CRM.
            String fetchXml = conversionResponse.FetchXml;
            return new FetchExpression(fetchXml);
        }

        public static bool IsIsOwnerEnabled(SecureEntity secureEntity)
        {
            if (!secureEntity.Secured || secureEntity.Schema == null)
            {
                return false;
            }

            if (secureEntity.Schema.Attributes == null || secureEntity.Schema.Attributes.Count == 0)
            {
                return false;
            }
            const string isOwnerAttribute = Constant.AttributeKeyName.NXL_ISOwner;
            DataModel.MetaData.Attribute isSharedAttr =
            secureEntity.Schema.Attributes.Find((DataModel.MetaData.Attribute attr)
            => attr.LogicName.Equals(isOwnerAttribute, StringComparison.OrdinalIgnoreCase));

            if (isSharedAttr == null)
            {
                return false;
            }
            return true;
        }

        public static bool IsIsOwnerEnabled(string entityName, MemoryCache<SecureEntity> secureEntityCache)
        {
            SecureEntity entity = secureEntityCache.Lookup(entityName, x => null);
            return IsIsOwnerEnabled(entity);
        }

        public static bool IsIsSharedEnabled(SecureEntity secureEntity)
        {

            if (!secureEntity.Secured || secureEntity.Schema == null)
            {
                return false;
            }

            if (secureEntity.Schema.Attributes == null || secureEntity.Schema.Attributes.Count == 0)
            {
                return false;
            }

            const string isSharedAttribute = Constant.AttributeKeyName.NXL_ISShared;
            DataModel.MetaData.Attribute isSharedAttr =
            secureEntity.Schema.Attributes.Find((DataModel.MetaData.Attribute attr)
            => attr.LogicName.Equals(isSharedAttribute, StringComparison.OrdinalIgnoreCase));

            if (isSharedAttr == null)
            {
                return false;
            }
            return true;
        }

        public static bool IsIsSharedEnabled(string entityName, MemoryCache<SecureEntity> secureEntityCache)
        {
            SecureEntity entity = secureEntityCache.Lookup(entityName, x => null);
            return IsIsSharedEnabled(entity);
        }

        public static bool IsEntityEnabled(string entitdyName,MemoryCache<SecureEntity> userAttributeCache)
        {
            SecureEntity entity = userAttributeCache.Lookup(entitdyName, x => null);
            if(entity == null)
            {
                return false;
            }

            return true;
        }

        public static bool IsTeamEnbledForUserAttribute(MemoryCache<SecureEntity> userAttributeCache)
        {
            return IsEntityEnabled(Team.EntityLogicalName, userAttributeCache);
        }

        public static bool IsRoleEnbledForUserAttribute(MemoryCache<SecureEntity> userAttributeCache)
        {
            return IsEntityEnabled(Role.EntityLogicalName, userAttributeCache);
        }

        public static string GetJPCHost(string inaddr) {
            return HostMatch(inaddr, Common.DataModel.ServerType.JPC);
        }

        public static string GetCCHost(string inaddr) {
            return HostMatch(inaddr, Common.DataModel.ServerType.CC);
        }

        public static string GetJPCPort()
        {
            return "443";
        }

        public static string GetCCPort()
        {
            return "443";
        }

        public static string HostMatch(string inaddr, Common.DataModel.ServerType type)
        {
            //http://, https://, http:\\, https:\\, /, \
            inaddr = inaddr.Trim().ToLower();
            if (inaddr.StartsWith(@"http://") || inaddr.StartsWith(@"http:\\"))
            {
                inaddr = inaddr.Substring(7);
            }
            else if (inaddr.StartsWith(@"https://") || inaddr.StartsWith(@"https:\\"))
            {
                inaddr = inaddr.Substring(8);
            }

            if (inaddr.EndsWith(@"/") || inaddr.EndsWith(@"\"))
            {
                inaddr = inaddr.Substring(0, inaddr.Length - 1);
            }

            if (inaddr.EndsWith(@"/console") || inaddr.EndsWith(@"\console"))
            {
                inaddr = inaddr.Substring(0, inaddr.Length - 8);
            }

            string[] domainblock = inaddr.Split('.');

            if (type == Common.DataModel.ServerType.CC)
            {
                if (domainblock[0].EndsWith(@"cc"))
                {
                }
                else if (domainblock[0].EndsWith(@"jpc"))
                {
                    domainblock[0] = domainblock[0].Substring(0, domainblock[0].Length - 3) + @"cc";
                }
                else
                {
                    domainblock[0] = domainblock[0] + @"cc";
                }
            }
            if (type == Common.DataModel.ServerType.JPC)
            {
                if (domainblock[0].EndsWith(@"jpc"))
                {
                }
                else if (domainblock[0].EndsWith(@"cc"))
                {
                    domainblock[0] = domainblock[0].Substring(0, domainblock[0].Length - 2) + @"jpc";
                }
                else
                {
                    domainblock[0] = domainblock[0] + @"jpc";
                }
            }

            string domain = string.Empty;
            foreach (string s in domainblock)
            {
                domain += s + @".";
            }
            return domain.Substring(0, domain.Length - 1);
        }
    }
}
