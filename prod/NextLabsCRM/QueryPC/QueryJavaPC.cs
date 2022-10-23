using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.JavaPC.RestAPISDK;
using NextLabs.JavaPC.RestAPISDK.CEModel;
using NextLabs.CRMEnforcer.Common.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Log;

namespace NextLabs.CRMEnforcer.Common.QueryPC
{

    public class CQuery
    {
        private const string m_strOblColumn = "COL";
        private const string m_strOblOperate = "OP";
        private const string m_strOblValue = "VAL";
        public class CAlertMsg
        {
            public string strAlertMsg;
        }
        public class OwnerAlwayAllow
        {
            public bool bAllow;
        }
        public class CCRMfilter
        {

            public string strColumn;
            public string strOperator;
            public string strValue;
            public string strfeOperator;
            public ConditionOperator qeOperator;
            public CCRMfilter(Common.DataModel.ApplySecurityFilterBasedonParentAttributesModel obligation)
            {
                strColumn = obligation.Filter.Field;
                qeOperator = obligation.Filter.Operator;
                strValue = obligation.Filter.Value;
            }

        }

        private NextLabs.JavaPC.RestAPISDK.JavaPC.DelegateLog m_delegateLog = null;
        public NextLabs.JavaPC.RestAPISDK.JavaPC.DelegateLog DelegateLog
        {
            set
            {
                m_delegateLog = value;
            }
        }

        public EnforceResult QueryJavaPCEva(NextLabsCRMLogs log, string strAction, string strEntityName,
               CEResource ceResource, CEUser ceUser, CEHost ceHost, CEApp ceApp)
        {
            EnforceResult enforcerResult = new EnforceResult();
            CERequest request = new CERequest();
            string strPCAction = strEntityName.ToUpper() + "_" + strAction.ToUpper();

            request.Set_Action(strPCAction);
            request.Set_NameAttributes("dont-care-acceptable", "yes", NextLabs.JavaPC.RestAPISDK.CEModel.CEAttributeType.XACML_String);
            request.Set_NameAttributes("error-result-acceptable", "yes", NextLabs.JavaPC.RestAPISDK.CEModel.CEAttributeType.XACML_String);
            request.Set_User(ceUser.Sid, ceUser.Name, ceUser.Attres);
            request.Set_Source(ceResource.SourceName,ceResource.SourceType,ceResource.Attres);
            request.Set_App(ceApp.Name, ceApp.Path, ceApp.Url, ceApp.Attres);
            request.Set_Host(ceHost.Name, ceHost.IPAddress, ceHost.Attres);

            List<CEObligation> lisOutObs = new List<CEObligation>();
            CEResponse ceOutResponse= CEResponse.CEDontCare;
            PCResult pcResult = PCResult.Failed;
            {
                //we need check pc is Authorizationed, only cloudAz need Authorizationed.
                if (JPCWrapper.JAVAPC.Authorizationed.Equals(PCResult.OK))
                {
                    //if pcresult HttpFaild_Unauthorized, we need update token or cookie
                    pcResult = JPCWrapper.JAVAPC.CheckResource(request, out ceOutResponse, out lisOutObs);
                    if (pcResult.Equals(PCResult.HttpFaild_Unauthorized))
                    {
                        log.WriteLog(LogLevel.Warning, "Unauthorized to access JPC. System is updating the token now.");
                        JPCWrapper.JAVAPC.UpdateToken();
                        pcResult = JPCWrapper.JAVAPC.CheckResource(request, out ceOutResponse, out lisOutObs);
                    }
                }
                else
                {
                    if (JPCWrapper.JAVAPC.Authorizationed.Equals(PCResult.InvalidOauthServer))
                    {
                        log.WriteLog(LogLevel.Error, "Unauthorized to access JPC. Please check your control center server setting.");
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Error, "Invalid credential, please check your client id and password.");
                    }
                    pcResult = JPCWrapper.JAVAPC.Authorizationed;
                }
            }
            if (pcResult != PCResult.OK)
            {
                string errorMsg = string.Format("'CheckResource' query on PC failed. The return value is {0}", pcResult);
                log.WriteLog(LogLevel.Error, errorMsg);
                throw new Exceptions.RestPDPException("CQuery", "QueryJavaPCEva", pcResult, JPCWrapper.JAVAPC);
            }
            enforcerResult.Decision = ceOutResponse;
            enforcerResult.ListObligations = lisOutObs;
            return enforcerResult;
        }

    }
}
