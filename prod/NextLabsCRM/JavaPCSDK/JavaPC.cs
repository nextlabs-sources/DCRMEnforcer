using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NextLabs.JavaPC.RestAPISDK
{
    public enum RequestDataType
    {
        Json,
        Xml
    }
    public enum PCResult
    {
        HttpFaild_Unknow,//This maybe network transmission problem
        OK,//Call Java PC Success
        Failed,//call Java PC Faild , and we don't know why
        ContentTypeNotSupported,// content type is not support
        SendFaild,// please check java pc url , maybe address or port error
        GetResponseFaild,//
        ResponseStatusAbnormal,//
        TransformResponseFaild,//
        TransformToJsonDataFaild,//
        MissAttributes,// Miss evaluation attributes , please check ceuser ceapp cesource had been send to java pc
        HttpFaild_Unauthorized,
        HttpFaild_BadRequest,
        HttpFaild_NotFound,
        HttpFaild_MethodNotAllowed,
        HttpFaild_ProxyAuthenticationRequired,
        HttpFaild_RequestTimeout,
        HttpFaild_RequestUriTooLong,
        HttpFaild_UnsupportedMediaType,
        HttpFaild_NotImplemented,
        HttpFaild_BadGateway,
        HttpFaild_ServiceUnavailable,
        HttpFaild_HttpVersionNotSupported,
        HttpFaild_GatewayTimeout,
        InvalidClient,
        InvalidOauthServer
    }


    public class JavaPC
    {
        private RequestDataType m_dataType;
        public RequestDataType DataType { get { return m_dataType; } }
        private string m_strJavaPcHost;

        private string m_strOAuthServiceHost;

        //private bool m_bIsCloudAZ;
        public string OAuthServiceHost
        {
            get
            {
                return m_strOAuthServiceHost;
            }
        }

        public string OAuthServiceUrl
        {
            get
            {
                string suffix = Constant.General.OnPremAuthSuffix;
                /*if (m_bIsCloudAZ)
                {
                    suffix = Constant.General.CloudAZAuthSuffix;
                }*/
                if (m_strOAuthServiceHost.EndsWith(Constant.General.CloudAZAuthSuffix+"/"))
                {
                    return m_strOAuthServiceHost.Substring(0, m_strOAuthServiceHost.Length - 1);
                }
                else
                {
                    return m_strOAuthServiceHost + suffix;
                }
            }
        }
        public string JavaPCHost
        {
            get
            {
                return m_strJavaPcHost;
            }
        }
        public string JavaPcUrl
        {
            get { return JavaPCHost + Constant.General.JavaPCPDPSuffix; }
        }
        public string JavaPCTGTUrl
        {
            get { return JavaPCHost + Constant.General.JavaPCTGTSuffix; }
        }

        private string m_strAccount = string.Empty;
        private string Acccout
        {
            get
            {
                return m_strAccount;
            }
        }
        private string m_strPassword = string.Empty;
        private string Password
        {
            get
            {
                return m_strPassword;
            }
        }

        private int m_iExpiresIn;
        private int ExpiresIn
        { 
            get
            {
                return m_iExpiresIn;
            }
        }

        private ReaderWriterLock m_rwlockAuthorization = new ReaderWriterLock();
        private ReaderWriterLock m_rwlockToken = new ReaderWriterLock();

        private PCResult m_Authorization;
        public PCResult Authorizationed
        {
            get
            {
                try
                {
                    m_rwlockAuthorization.AcquireReaderLock(Constant.HttpRquest.TimeOut);
                    if (m_rwlockAuthorization.IsReaderLockHeld)
                    {
                        return m_Authorization;
                    }
                    else
                    {
                        return PCResult.Failed;
                    }
                }
                finally
                {
                    m_rwlockAuthorization.ReleaseReaderLock();
                }
            }
            set
            {
                try
                {
                    m_rwlockAuthorization.AcquireWriterLock(Constant.HttpRquest.TimeOut);
                    if (m_rwlockAuthorization.IsWriterLockHeld)
                    {
                        m_Authorization = value;
                    }
                }
                finally
                {
                    m_rwlockAuthorization.ReleaseWriterLock();
                }
            }
        }

        private string m_strToken;
        public string Token
        {
            get
            {
                try
                {
                    m_rwlockToken.AcquireReaderLock(Constant.HttpRquest.TimeOut);
                    if (m_rwlockToken.IsReaderLockHeld)
                    {
                        return m_strToken;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                finally
                {
                    m_rwlockToken.ReleaseReaderLock();
                }
            }
            set
            {
                try
                {
                    m_rwlockToken.AcquireWriterLock(Constant.HttpRquest.TimeOut);
                    if (m_rwlockToken.IsWriterLockHeld)
                    {
                        m_strToken = value;
                    }
                }
                finally
                {
                    m_rwlockToken.ReleaseWriterLock();
                }
            }
        }

        HttpHelpHttpClient m_HttpClient = null;
        private System.Threading.Tasks.Task m_task;
        private System.Threading.Timer m_timerUpdateCookie;
        private System.Threading.Timer m_timerUpdateToken;
        public delegate void DelegateLog(string strMsg);
        private DelegateLog m_DelegateLog = null;



        /// <summary>
        /// This is For ColudAZ 8.0.2
        /// </summary>
        /// <param name="dataType">We can choose rest request datatype , json or xml</param>
        /// <param name="strJavaPcHost">for example https://crm-jpc.crm.nextlabs.solutions/</param>
        /// <param name="strCCAccount">for example Administrator</param>
        /// <param name="strCCPassword">for example 123next!</param>
        /// <param name="bSync">Because get Authentication with CloudAZ will spend manay time, so is this value is true, this get Authentication opear is Sync, after method end, we can get Authentication Result.</param>
        /// <param name="delegateLog">This is a deletage, you can get this type from JavaPC.DelegateLog , if this value is not null and build debug version, SDK will call this function when it need out put log</param>
        /// <param name="iAutoUpdateCookieTime">set time interval, it will trigger update cookie event , unit is minute </param>
        [Obsolete]
        public JavaPC(RequestDataType dataType, string strJavaPcHost, string strCCAccount, string strCCPassword, bool bSync, DelegateLog delegateLog, int iAutoUpdateCookieTime = 15)
        {
#if  DEBUG
            Stopwatch swConstructor = new Stopwatch();
            swConstructor.Start();
#endif
            m_HttpClient = new HttpHelpHttpClient();
            m_dataType = dataType;

            m_DelegateLog = delegateLog;

            if (!strJavaPcHost.EndsWith(Constant.HttpRquest.HttpSeparator))
            {
                strJavaPcHost = strJavaPcHost + Constant.HttpRquest.HttpSeparator;
            }
            m_strJavaPcHost = strJavaPcHost;
            m_strAccount = ModelTransform.UrlEncoder.Encode(strCCAccount);
            m_strPassword = ModelTransform.UrlEncoder.Encode(strCCPassword);
            m_timerUpdateCookie = new Timer(new TimerCallback(TimerCallback_UpdateCookie), null, iAutoUpdateCookieTime * Constant.General.AutoUpdateCookieTimeUnit, iAutoUpdateCookieTime * Constant.General.AutoUpdateCookieTimeUnit);
            m_task = new System.Threading.Tasks.Task(UpdateCookie);
            m_task.Start();
            if (bSync)
            {
                m_task.Wait();
            }

#if  DEBUG
            swConstructor.Stop();
            log("Constructor TimeSpan:" + swConstructor.ElapsedMilliseconds);
#endif
        }

        
        /// <summary>
        /// This is For ColudAZ 8.0.4
        /// </summary>
        /// <param name="dataType">We can choose rest request datatype , json or xml</param>
        /// <param name="strJavaPcHost">for example https://crm-jpc.crm.nextlabs.solutions/</param>
        /// <param name="strClientId">for example apiuser</param>
        /// <param name="strClientSecret">for example 123next!</param>
        /// <param name="delegateLog">This is a deletage, you can get this type from JavaPC.DelegateLog , if this value is not null and build debug version, SDK will call this function when it need out put log</param>
        /// <param name="bSync">Because get Authentication with CloudAZ will spend manay time, so is this value is true, this get Authentication opear is Sync, after method end, we can get Authentication Result.</param>
        /// <param name="iExpiresIn">set time interval, it will trigger update token event , unit is minute</param>
        public JavaPC(RequestDataType dataType, string strJavaPcHost, string strClientId, string strClientSecret, DelegateLog delegateLog,bool bSync, int iExpiresIn=6)
        {
#if  DEBUG
            Stopwatch swConstructor = new Stopwatch();
            swConstructor.Start();
#endif
            m_HttpClient = new HttpHelpHttpClient();
            m_dataType = dataType;

            m_DelegateLog = delegateLog;
            m_iExpiresIn = iExpiresIn;
            if (!strJavaPcHost.EndsWith(Constant.HttpRquest.HttpSeparator))
            {
                strJavaPcHost = strJavaPcHost + Constant.HttpRquest.HttpSeparator;
            }
            m_strJavaPcHost = strJavaPcHost;
            m_strAccount = ModelTransform.UrlEncoder.Encode(strClientId);
            m_strPassword = ModelTransform.UrlEncoder.Encode(strClientSecret);
            m_timerUpdateToken = new Timer(new TimerCallback(TimerCallback_UpdateToken), null, ExpiresIn * Constant.General.AutoUpdateCookieTimeUnit, ExpiresIn * Constant.General.AutoUpdateCookieTimeUnit);
            m_task = new System.Threading.Tasks.Task(UpdateToken);
            m_task.Start();
            if (bSync)
            {
                m_task.Wait();
            }

#if  DEBUG
            swConstructor.Stop();
            log("Constructor TimeSpan:" + swConstructor.ElapsedMilliseconds);
#endif
        }

        /// <summary>
        /// For CC8.1
        /// </summary>
        /// <param name="dataType">We can choose rest request datatype , json or xml</param>
        /// <param name="strJavaPcHost">for example https://crm-jpc.crm.nextlabs.solutions/</param>
        /// <param name="strOAuthServiceURL">https://<OAuth2>:<port>/oauth/token</param>
        /// <param name="strClientId">for example apiuser</param>
        /// <param name="strClientSecret">for example 123next!</param>
        /// <param name="delegateLog">This is a deletage, you can get this type from JavaPC.DelegateLog , if this value is not null and build debug version, SDK will call this function when it need out put log</param>
        /// <param name="bSync">Because get Authentication with CloudAZ will spend manay time, so is this value is true, this get Authentication opear is Sync, after method end, we can get Authentication Result.</param>
        /// <param name="iExpiresIn">set time interval, it will trigger update token event , unit is minute</param>
        public JavaPC(RequestDataType dataType, string strJavaPcHost, string strOAuthServiceHost,/*bool bIsCloudAZ,*/ string strClientId, string strClientSecret, DelegateLog delegateLog, bool bSync, int iExpiresIn = 6)
        {
#if  DEBUG
            Stopwatch swConstructor = new Stopwatch();
            swConstructor.Start();
#endif
            m_HttpClient = new HttpHelpHttpClient();
            m_dataType = dataType;

            m_DelegateLog = delegateLog;
            m_iExpiresIn = iExpiresIn;

            if (!strJavaPcHost.EndsWith(Constant.HttpRquest.HttpSeparator))
            {
                strJavaPcHost = strJavaPcHost + Constant.HttpRquest.HttpSeparator;
            }
            m_strJavaPcHost = strJavaPcHost;

            if (!strOAuthServiceHost.EndsWith(Constant.HttpRquest.HttpSeparator))
            {
                strOAuthServiceHost = strOAuthServiceHost + Constant.HttpRquest.HttpSeparator;
            }
            m_strOAuthServiceHost = strOAuthServiceHost;
            //m_bIsCloudAZ = bIsCloudAZ;

            m_strAccount = ModelTransform.UrlEncoder.Encode(strClientId);
            m_strPassword = ModelTransform.UrlEncoder.Encode(strClientSecret);
            m_timerUpdateToken = new Timer(new TimerCallback(TimerCallback_UpdateToken), null, ExpiresIn * Constant.General.AutoUpdateCookieTimeUnit, ExpiresIn * Constant.General.AutoUpdateCookieTimeUnit);
            m_task = new System.Threading.Tasks.Task(UpdateToken);
            m_task.Start();
            if (bSync)
            {
                m_task.Wait();
            }

#if  DEBUG
            swConstructor.Stop();
            log("Constructor TimeSpan:" + swConstructor.ElapsedMilliseconds);
#endif
        }

        /// <summary>
        ///This is for local Java PC 
        /// </summary>
        /// <param name="dataType">We can choose rest request datatype , json or xml</param>
        /// <param name="strJavaPcHost">for example https://crm-jpc.crm.nextlabs.solutions/</param>
        /// <param name="delegateLog">This is a deletage, you can get this type from JavaPC.DelegateLog , if this value is not null and build debug version, SDK will call this function when it need out put log</param>
        public JavaPC(RequestDataType dataType, string strJavaPcHost, DelegateLog delegateLog)
        {
#if  DEBUG
            Stopwatch swConstructor = new Stopwatch();
            swConstructor.Start();
#endif
            m_DelegateLog = delegateLog;
            m_dataType = dataType;
            if (!strJavaPcHost.EndsWith(Constant.HttpRquest.HttpSeparator))
            {
                strJavaPcHost = strJavaPcHost + Constant.HttpRquest.HttpSeparator;
            }
            m_strJavaPcHost = strJavaPcHost;
            Authorizationed = PCResult.OK;
            m_HttpClient = new HttpHelpHttpClient();
#if  DEBUG
            swConstructor.Stop();
            log("Constructor TimeSpan:" + swConstructor.ElapsedMilliseconds);
#endif
        }

        private void TimerCallback_UpdateCookie(object sender)
        {
#if  DEBUG
            log("TimerCallback_UpdateCookie Start...");
#endif
            UpdateCookie();
#if  DEBUG
            log("TimerCallback_UpdateCookie End...");
#endif
        }

        private void TimerCallback_UpdateToken(object sender)
        {
#if  DEBUG
            log("TimerCallback_UpdateToken Start...");
#endif
            UpdateToken();
#if  DEBUG
            log("TimerCallback_UpdateToken End...");
#endif
        }
        public void UpdateCookie()
        {
#if  DEBUG
            log("UpdateCookie Start...");
#endif
            PCResult tempAuthorizationed = PCResult.HttpFaild_Unauthorized;
            string strResponse = null;
            List<KeyValuePair<string, string>> lisOutHeaders = null;
            //step 1,Getting Ticket Granting Ticket
            //The first step of the authentication process is to get the Ticket Granting Ticket (TGT), in Postman, you can invoke a POST call to url: https://​{JPC_​HOST}/​cas/​v1/​tickets with x-www-form-urlencoded data:
            //username: Your Control Center username
            //password: Your Control Center password
            //After you get the response, you can switch to be reponse header section and copy the Location header value:

            tempAuthorizationed = m_HttpClient.SendDataAndGetResponse(Constant.HttpRquest.Method_Post, Constant.HttpRquest.ContentType_X_WWW_From_Urlencoded, null, JavaPCTGTUrl, GetX_WWW_Form_UrlencodedData(m_strAccount, m_strPassword), Encoding.ASCII, out strResponse, out lisOutHeaders,true);
            if (tempAuthorizationed.Equals(PCResult.OK))
            {
                if (lisOutHeaders != null && lisOutHeaders.Count > 0)
                {
                    KeyValuePair<string, string> pairLocation = lisOutHeaders.Find(delegate(KeyValuePair<string, string> item) { return item.Key.Equals(Constant.HttpRquest.Header_Key_Location); });
                    if (pairLocation.Value != null)
                    {
                        string strLocation = pairLocation.Value;
                        //Getting Service Ticket
                        //The second step of the authentication process is to get the Service Ticket (ST). The url to request is the Location header value get from 1st step. In Postman, you can invoke a POST call to the url with x-www-form-urlencoded data:
                        //service: https://{JPC_HOST}/dpc/authorization/pdp
                        //This is issue need check with cc team, do we need add port number for service
                        tempAuthorizationed = m_HttpClient.SendDataAndGetResponse(Constant.HttpRquest.Method_Post, Constant.HttpRquest.ContentType_X_WWW_From_Urlencoded, null, strLocation, string.Format(Constant.HttpRquest.X_WWW_From_Urlencoded_Service, ModelTransform.UrlEncoder.Encode(JavaPcUrl.Replace(":"+new Uri(JavaPcUrl).Port,""))), Encoding.ASCII, out strResponse, out lisOutHeaders,true);
                        if (tempAuthorizationed.Equals(PCResult.OK))
                        {
                            string strServiceTicket = strResponse;
                            //Getting Authentication Cookie
                            //The 3rd step of the authentication process is to get the Authentication cookie using ST. In postman, you can invoke a GET call to the url: https://​{JPC_​HOST}/​dpc/​authorization/​pdp with query string parameter:
                            //ticket: The ST value


                            tempAuthorizationed = m_HttpClient.SendDataAndGetResponse(Constant.HttpRquest.Method_Get, Constant.HttpRquest.ContentType_X_WWW_From_Urlencoded, null, string.Format(Constant.HttpRquest.UrlParameter_Getting_Authentication_Cookie, JavaPcUrl, strServiceTicket), null, Encoding.ASCII, out strResponse, out lisOutHeaders,true);

                            if (!tempAuthorizationed.Equals(PCResult.OK))
                            {
#if  DEBUG
                                log("Getting Service Ticket, Result:" + tempAuthorizationed);
#endif
                            }
                        }
                        else
                        {
#if  DEBUG
                            log("Getting Service Ticket, Result:" + tempAuthorizationed);
#endif
                        }
                    }
                    else
                    {
#if  DEBUG
                        log("Getting Ticket Granting Ticket Faild, Location header value is empty");
#endif
                    }

                }
                else
                {
#if  DEBUG
                    log("Getting Ticket Granting Ticket Faild , Response Header is empty");
#endif
                }
            }
            else
            {
#if  DEBUG
                log("Getting Ticket Granting Ticket Faild, Result:" + tempAuthorizationed);
#endif
            }
            Authorizationed = tempAuthorizationed;
#if  DEBUG
            log("UpdateCookie End");
#endif


        }
        public void UpdateToken()
        {
#if  DEBUG
            log("UpdateToken Start...");
#endif
            if (OAuthServiceHost.Equals(string.Empty))
            {
                return;
            }
            PCResult tempAuthorizationed = PCResult.HttpFaild_Unauthorized;
            string strResponse = null;
            List<KeyValuePair<string, string>> lisOutHeaders = null;
            //Q:why ExpiresIn+1?
            //A:Allow token to be longer than the update time , make Prevent updates without authentication
            
            tempAuthorizationed = m_HttpClient.SendDataAndGetResponse(Constant.HttpRquest.Method_Post, Constant.HttpRquest.ContentType_X_WWW_From_Urlencoded, null, OAuthServiceUrl, new NextLabs.JavaPC.RestAPISDK.RestModel.Authorized.TokenRequest(m_strAccount, m_strPassword, (ExpiresIn + 1) * Constant.General.AutoUpdateTokenTimeUnit).ToString(), Encoding.ASCII, out strResponse, out lisOutHeaders);
            if (tempAuthorizationed.Equals(PCResult.OK))
            {
                RestModel.Authorized.TokenResponse tokenResponse = ModelTransform.Function.TransformFormJSONToTokenResponse(strResponse);
                if(!string.IsNullOrEmpty(tokenResponse.access_token)&&!string.IsNullOrEmpty(tokenResponse.token_type))
                {
                    Token = tokenResponse.token_type + " " + tokenResponse.access_token;
                }
                else
                {
                    tempAuthorizationed = ModelTransform.Function.TransformFromInvaillTokenToPcResult(strResponse);
                }
            }
            else
            {
#if  DEBUG
                log("Getting Token Faild, Result:" + tempAuthorizationed);
#endif
            }
            if (tempAuthorizationed.Equals(PCResult.SendFaild)||
                tempAuthorizationed.Equals(PCResult.HttpFaild_NotFound))
            {
                Authorizationed = PCResult.InvalidOauthServer;
            }
            else
            {
                Authorizationed = tempAuthorizationed;
            }
#if  DEBUG
            log("UpdateToken End");
#endif
        }

        private string GetX_WWW_Form_UrlencodedData(string strAccount, string strPassword)
        {
            return string.Format(Constant.HttpRquest.X_WWW_From_Urlencoded_Data_Format, strAccount, strPassword);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ceRequest">you need prepare request , please don't set ceuser or ceapp or cereosurce empty , maybe it can retun faild</param>
        /// <param name="ceResponse">policy enforce result</param>
        /// <param name="lisObligation">policy return obligation</param>
        /// <returns></returns>
        public PCResult CheckResource(NextLabs.JavaPC.RestAPISDK.CEModel.CERequest ceRequest, out NextLabs.JavaPC.RestAPISDK.CEModel.CEResponse ceResponse, out List<NextLabs.JavaPC.RestAPISDK.CEModel.CEObligation> lisObligation)
        {
#if  DEBUG
            log("CheckResource Start...");
            Stopwatch swCheckResource = new Stopwatch();
            swCheckResource.Start();
#endif
            PCResult pcResult = PCResult.Failed;
            ceResponse = NextLabs.JavaPC.RestAPISDK.CEModel.CEResponse.CEDontCare;

            lisObligation = null;
            string strContentType = string.Empty;

            if (PraseRequestType(m_dataType, out strContentType))
            {
                List<KeyValuePair<string, string>> lisHeaders = new List<KeyValuePair<string, string>>();
                {
                    lisHeaders.Add(new KeyValuePair<string, string>(Constant.HttpRquest.Service, Constant.HttpRquest.Service_Eval));
                    lisHeaders.Add(new KeyValuePair<string, string>(Constant.HttpRquest.Version, Constant.HttpRquest.Version_1_0));
                    if(!string.IsNullOrEmpty(Token))
                    {
                        lisHeaders.Add(new KeyValuePair<string, string>(Constant.HttpRquest.Authorization, Token));
                    }
                }

                RestAPISDK.RestModel.Single.RestRequest restRequest = RestAPISDK.ModelTransform.Function.TransformToSingleRequest(ceRequest);
                string strRestRequest = restRequest.ToString();
#if  DEBUG
                log("Rest Request:" + strRestRequest);
#endif
                if (strRestRequest != null && strRestRequest.Length > 0)
                {
                    string strResponse = null;
                    List<KeyValuePair<string, string>> lisOutHeaders = null;
                    //8.0.2 don't use token so , we can use it to distinguish versions
                    if (!string.IsNullOrEmpty(Token))
                    {
                        pcResult = m_HttpClient.SendDataAndGetResponse(Constant.HttpRquest.Method_Post, Constant.HttpRquest.ContentType_JSON, lisHeaders, JavaPcUrl, strRestRequest, Encoding.ASCII, out strResponse, out lisOutHeaders);
                    }
                    else
                    {
                        pcResult = m_HttpClient.SendDataAndGetResponse(Constant.HttpRquest.Method_Post, Constant.HttpRquest.ContentType_JSON, lisHeaders, JavaPcUrl, strRestRequest, Encoding.ASCII, out strResponse, out lisOutHeaders,true);
                    }
#if  DEBUG
                    log("Rest Response:" + strResponse);
#endif
                    if (pcResult.Equals(PCResult.OK))
                    {
                        RestModel.Single.ResultNode restResult = null;
                        RestModel.Single.RestResponse restResponse = ModelTransform.Function.TransformFormJSONToRestResponse(strResponse);
                        if (restResponse != null && restResponse.Response != null && restResponse.Response.Result != null && restResponse.Response.Result.Count > 0)
                        {
                            restResult = restResponse.Response.Result[0];
                        }
                        // Add new response formart to support JavaPC 8.7
                        if (restResult == null)
                        {
                            RestModel.Single.NewRestResponse newRestResponse = ModelTransform.Function.TransformFormJSONToNewRestResponse(strResponse);
                            if (newRestResponse != null && newRestResponse.Response != null && newRestResponse.Response.Count > 0)
                            {
                                restResult = newRestResponse.Response[0];
                            }
                        }
                        if (restResult != null)
                        { 
                            if (restResult.Status != null && restResult.Status.StatusMessage != null && restResult.Status.StatusCode != null && restResult.Status.StatusCode.Value != null)
                            {
                                string strStatusCode = restResult.Status.StatusCode.Value;
                                pcResult = RestAPISDK.ModelTransform.Function.TransformFromStatusToPcResult(strStatusCode);
                                if (pcResult.Equals(PCResult.OK))
                                {
                                    ceResponse = RestAPISDK.ModelTransform.Function.TransformToCEResponse(restResult.Decision);
                                    lisObligation = RestAPISDK.ModelTransform.Function.TransformToCEObligation(restResult);
                                }
                            }
                            else
                            {
                                pcResult = PCResult.ResponseStatusAbnormal;
                            }

                        }
                        else
                        {
                            pcResult = PCResult.TransformResponseFaild;
                        }

                    }
                    else
                    {
#if  DEBUG
                        log("SendDataAndGetResponse Faild");
#endif
                    }
                }
                else
                {
                    pcResult = PCResult.TransformToJsonDataFaild;
                }
            }
            else
            {
                pcResult = PCResult.ContentTypeNotSupported;

            }
#if  DEBUG
            swCheckResource.Stop();
            log("CheckResource End Query Result:" + pcResult + " Evaluation Result:" + ceResponse + " TimeSpan:" + swCheckResource.ElapsedMilliseconds);
#endif
            return pcResult;
        }

        public PCResult CheckPCState()
        {
            PCResult result = PCResult.Failed;
            List<CEModel.CEObligation> lisCeObs = new List<CEModel.CEObligation>();
            CEModel.CEResponse ceResponse = CEModel.CEResponse.CEDontCare;
            CEModel.CERequest sampleRequest = new CEModel.CERequest();
            sampleRequest.Set_User("Sid", "Name", null);
            sampleRequest.Set_Action("Action");
            sampleRequest.Set_Source("SourceName", "SourceType", null);
            sampleRequest.Set_NameAttributes("dont-care-acceptable", "yes", NextLabs.JavaPC.RestAPISDK.CEModel.CEAttributeType.XACML_String);
            sampleRequest.Set_NameAttributes("error-result-acceptable", "yes", NextLabs.JavaPC.RestAPISDK.CEModel.CEAttributeType.XACML_String);
            result = CheckResource(sampleRequest, out ceResponse, out lisCeObs);
            return result;

        }
        private bool PraseRequestType(RequestDataType dataType, out string strContentType)
        {
            bool bResult = true;
            strContentType = Constant.HttpRquest.ContentType_Unknow;
            if (m_dataType.Equals(RequestDataType.Json))
            {
                strContentType = Constant.HttpRquest.ContentType_JSON;
            }
            else if (m_dataType.Equals(RequestDataType.Xml))
            {
                strContentType = Constant.HttpRquest.ContentType_XML;
            }
            else
            {
                bResult = false;
            }
            return bResult;
        }
#if  DEBUG
        private void log(string strMsg)
        {
            if (m_DelegateLog != null)
            {
                m_DelegateLog.Invoke("[Nextlabs.JavaPC.RestAPISDK]:" + strMsg);
            }
            else
            {
                //System.Diagnostics.Trace.WriteLine("[Nextlabs.JavaPC.RestAPISDK]:" + strMsg);
            }
        }
#endif

        public override string ToString()
        {
            StringBuilder sbDetail = new StringBuilder();
            sbDetail.Append("OAuthServiceUrl:" + OAuthServiceUrl + "\r\n");
            sbDetail.Append("OAuthServiceHost:" + OAuthServiceHost + "\r\n");
            sbDetail.Append("JavaPCHost:" + JavaPCHost + "\r\n");
            sbDetail.Append("JavaPcUrl:" + JavaPcUrl + "\r\n");
            sbDetail.Append("Acccout:" + Acccout + "\r\n");
            return sbDetail.ToString();
        }
    }
}
