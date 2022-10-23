using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using NextLabs.CRMEnforcer.Common.DataModel;
using NextLabs.CRMEnforcer.Common.DataBussiness.LogonCC;
using NextLabs.CRMEnforcer.Common.DataBussiness.PolicyModel;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Log;
using NextLabs.JavaPC.RestAPISDK;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    public class NxlCommon
    {
        public string m_hostaddr;
        public string m_ccusername;
        public string m_ccpassword;

        //Dictionary<Op dataType, Dictionary<logic symbol, cloudAz symbol>>
        private Dictionary<string, Dictionary<string, int>> m_dicOperators = new Dictionary<string, Dictionary<string, int>>();
        private List<NxlRequestModel.TagsItem> m_Tags = new List<NxlRequestModel.TagsItem>();
        private string m_version = string.Empty;

        public NxlCommon()
        {

        }

        public NxlCommon(string hostaddr, string username, string password)
        {
            m_hostaddr = hostaddr;
            m_ccusername = username;
            m_ccpassword = password;
        }

        public enum PMResult
        {
            HttpFaild_Unknow,//This maybe network transmission problem
            OK,//Call Java PC Success
            Failed,//call Java PC Faild , and we don"t know why
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
        public string GetCC()
        {
            return @"https://" + Util.HostMatch(m_hostaddr, ServerType.CC);

        }
        public string GetQueryPolicyModelAPI()
        {
            return GetCC() + "/console/api/v1/policyModel/search";
        }
        public string GetQueryPolicyModelDetailAPI()
        {
            return GetCC() + "/console/api/v1/policyModel/mgmt/active/";
        }
        public string GetModifyPolicyModelAPI()
        {
            return GetCC() + "/console/api/v1/policyModel/mgmt/modify";
        }
        public string GetAddPolicyModelAPI()
        {
            return GetCC() + "/console/api/v1/policyModel/mgmt/add";
        }
        private string GetPolicyModelOperatorAPI()
        {
            return GetCC() + "/console/api/v1/config/dataType/list/";
        }
        private string GetPolicyModelStringOperatorAPI()
        {
            return GetPolicyModelOperatorAPI() + "STRING";
        }
        private string GetPolicyModelNumberOperatorAPI()
        {
            return GetPolicyModelOperatorAPI() + "NUMBER";
        }
        private string GetPolicyModelMultivalOperatorAPI()
        {
            return GetPolicyModelOperatorAPI() + "MULTIVAL";
        }
        private string GetPolicyModelTagsAPI()
        {
            return GetCC() + "/console/api/v1/config/tags/list";
        }
        private string CreatePolicyModelTagsAPI()
        {
            return GetCC() + "/console/api/v1/config/tags/add";
        }
        private string GetSystemVersionAPI()
        {
            return GetCC() + "/console/api/v1/system/version";
        }
        private string GetAddPolicyModelTagAPI()
        {
            return GetCC() + "/console/api/v1/config/tags/add/POLICY_MODEL_TAG";
        }
        private string GetAddComponentTagAPI()
        {
            return GetCC() + "/console/api/v1/config/tags/add/COMPONENT_TAG";
        }
        private string GetAddPolicyTagAPI()
        {
            return GetCC() + "/console/api/v1/config/tags/add/POLICY_TAG";
        }
        //Bug 54930 - Unable to import action to CC automatically when secured entities(CC87) 
        //private string GetCreateComponentAPI()
        //{
        //    return GetCC() + "/console/api/v1/component/mgmt/add";
        //}
        //private string GetListComponentsAPI()
        //{
        //    return GetCC() + "/console/api/v1/component/search";
        //}
        public bool versionPriorTo87()
        {
            if (!string.IsNullOrEmpty(m_version) && m_version.StartsWith("8.7")) return false;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strMethod"></param>
        /// <param name="strContentType"></param>
        /// <param name="lisHeaders"></param>
        /// <param name="strUrl"></param>
        /// <param name="strRequest"></param>
        /// <param name="encode"></param>
        /// <param name="strResponse"></param>
        /// <param name="lisOutHeaders"></param>
        /// <returns></returns>
        public PMResult SendDataAndGetResponse(string strMethod, string strContentType, List<KeyValuePair<string, string>> lisHeaders, string strUrl, string strRequest, Encoding encode, out string strResponse, out List<KeyValuePair<string, string>> lisOutHeaders, NextLabsCRMLogs log)
        {
            PMResult result = PMResult.Failed;
            lisOutHeaders = new List<KeyValuePair<string, string>>();
            strResponse = string.Empty;
            HttpMethod method = new HttpMethod(strMethod);
            Uri uriSite = new Uri(strUrl);
            HttpRequestMessage requestMessage = new HttpRequestMessage(method, uriSite);
            if (!string.IsNullOrEmpty(strRequest))
            {
                requestMessage.Content = new StringContent(strRequest, encode, strContentType);
                requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(strContentType);
                //log.WriteLog(LogLevel.Debug, "SendDataAndGetResponse ContentLength: " + requestMessage.Content.Headers.ContentLength + " ContentType: " + requestMessage.Content.Headers.ContentType);
            }
            //log.WriteLog(LogLevel.Debug, "strMethod : " + strMethod);
            //log.WriteLog(LogLevel.Debug, "strUrl : " + strUrl);

            var handler = new HttpClientHandler { UseCookies = false };
            var client = new HttpClient(handler) { BaseAddress = uriSite };
            if (lisHeaders != null && lisHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> header in lisHeaders)
                {
                    if (header.Value != null)
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                    //log.WriteLog(LogLevel.Debug, "header.Key : " + header.Key + "  header.Value: " + header.Value);
                }
            }

            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = client.SendAsync(requestMessage).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    foreach (var p in httpResponse.Headers)
                    {
                        lisOutHeaders.Add(new KeyValuePair<string, string>(p.Key, string.Join(" ", p.Value)));
                    }
                    strResponse = httpResponse.Content.ReadAsStringAsync().Result;
                    result = PMResult.OK;
                }
                else
                {
                    log.WriteLog(LogLevel.Debug, "SendDataAndGetResponse httpResponse.StatusCode: " + httpResponse.StatusCode);
                    //log.WriteLog(LogLevel.Debug, "SendDataAndGetResponse httpResponse.Result: " + httpResponse.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                result = PMResult.SendFaild;
            }
            return result;
        }


        public string LoginWebConsole(string strConsoleUrl, string strUserName, string strPassword, NextLabsCRMLogs log)
        {
            try
            {
                string cookie = string.Empty;
                string strAuthUrl = string.Empty;
                string strlt = string.Empty;
                string strExecution = string.Empty;
                LogonWebConsole.GetAuthUrlAndParameter(strConsoleUrl, out strAuthUrl, out strlt, out strExecution);

                LogonWebConsole.Login(GetCC() + strAuthUrl, strUserName, LogonWebConsole.UrlEncode(strPassword), strlt, LogonWebConsole.UrlEncode(strExecution), out cookie);
                return cookie;
            }
            catch (Exception exp)
            {
                log.WriteLog(LogLevel.Debug, "LoginWebConsole Exception: " + exp.ToString());
                return "";
            }
        }
        public string SendPostRequestToConsole(string strUrl, string strCookie, string strDataType, string strPostData, NextLabsCRMLogs log)
        {
            string policyResponse = string.Empty;
            List<KeyValuePair<string, string>> lisHeaders = new List<KeyValuePair<string, string>>();
            {
                lisHeaders.Add(new KeyValuePair<string, string>("Cookie", strCookie));

            }

            /*PMResult result = PMResult.Failed;*/
            List<KeyValuePair<string, string>> lisOutHeaders = null;
            /*result = */
            SendDataAndGetResponse(@"POST", strDataType, lisHeaders, strUrl, strPostData, Encoding.UTF8, out policyResponse, out lisOutHeaders, log);
            return policyResponse;
        }
        public string SendPutRequestToConsole(string strUrl, string strCookie, string strDataType, string strPostData, NextLabsCRMLogs log)
        {
            string policyResponse = string.Empty;
            List<KeyValuePair<string, string>> lisHeaders = new List<KeyValuePair<string, string>>();
            {
                lisHeaders.Add(new KeyValuePair<string, string>("cookie", strCookie));

            }
            /*PMResult result = PMResult.Failed;*/
            List<KeyValuePair<string, string>> lisOutHeaders = null;
            /*result = */
            SendDataAndGetResponse(@"PUT", strDataType, lisHeaders, strUrl, strPostData, Encoding.UTF8, out policyResponse, out lisOutHeaders, log);

            return policyResponse;
        }
        public string SendGetRequestToConsole(string strUrl, string strCookie, NextLabsCRMLogs log)
        {
            string policyResponse = string.Empty; //m_response.getBody();
            List<KeyValuePair<string, string>> lisHeaders = new List<KeyValuePair<string, string>>();
            {
                lisHeaders.Add(new KeyValuePair<string, string>("cookie", strCookie));

            }
            //PMResult result = PMResult.Failed;
            List<KeyValuePair<string, string>> lisOutHeaders = null;
            /*result = */
            SendDataAndGetResponse(@"GET", @"application/x-www-form-urlencoded", lisHeaders, strUrl, null, Encoding.UTF8, out policyResponse, out lisOutHeaders, log);
            return policyResponse;
        }
        private void InitPolicyModelTags(string strCookie, NextLabsCRMLogs log)
        {
            string strReqtags = "{\"tag\":{\"type\":\"POLICY_MODEL_TAG\",\"label\":\"EMDCRM\"},\"pageNo\":0,\"pageSize\":65535}";
            string strAPIListTagsAddr = GetPolicyModelTagsAPI();
            string strRepTagsJSON = SendPostRequestToConsole(strAPIListTagsAddr, strCookie, "application/json", strReqtags, log);
            log.WriteLog(LogLevel.Debug, "InitPolicyModelTags, strRepTagsJSON :" + strRepTagsJSON);
            if (!string.IsNullOrEmpty(strRepTagsJSON))
            {
                TagsRepModel.Root tagsRepModel = JsonSerializer.LoadFromJson<TagsRepModel.Root>(strRepTagsJSON);
                if ((tagsRepModel.statusCode == "1003") && (tagsRepModel.data.Count > 0))
                {
                    foreach (TagsRepModel.DataItem rtag in tagsRepModel.data)
                    {
                        m_Tags.Add(new NxlRequestModel.TagsItem(rtag));
                    }
                }
                else if (tagsRepModel.statusCode == "5000")
                {
                    string strAPIAddTags = GetAddPolicyModelTagAPI();
                    string strReqAddTag = "{\"key\":\"EM for Dynamics 365\",\"label\":\"EMDCRM\",\"type\":\"POLICY_MODEL_TAG\",\"status\":\"ACTIVE\"}";
                    string strAddTagsJSON = SendPostRequestToConsole(strAPIAddTags, strCookie, "application/json", strReqAddTag, log);
                    if (!string.IsNullOrEmpty(strAddTagsJSON))
                    {
                        AddTagRepModel.Root addTagRepModel = JsonSerializer.LoadFromJson<AddTagRepModel.Root>(strAddTagsJSON);
                        if (addTagRepModel.statusCode == "1000")
                        {
                            m_Tags.Add(new NxlRequestModel.TagsItem(addTagRepModel.data, "EM for Dynamics 365", "EMDCRM", "POLICY_MODEL_TAG", "ACTIVE"));
                        }
                        else
                        {
                            log.WriteLog(LogLevel.Debug, "InitPolicyModelTags | Add tags failed");
                        }
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Error, "InitPolicyModelTags | failed to add tag to cc");
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Debug, "InitPolicyModelTags failed, tagsRepModel.statusCode: " + tagsRepModel.statusCode);
                }
            }
            else
            {
                log.WriteLog(LogLevel.Error, "InitPolicyModelTags | failed to get tags from cc");
            }
        }
        private void InitComponentTags(string strCookie, NextLabsCRMLogs log)
        {
            string strReqtags = "{\"tag\":{\"type\":\"COMPONENT_TAG\",\"label\":\"EMDCRM\"},\"pageNo\":0,\"pageSize\":65535}";
            string strAPIListTagsAddr = GetPolicyModelTagsAPI();
            string strRepTagsJSON = SendPostRequestToConsole(strAPIListTagsAddr, strCookie, "application/json", strReqtags, log);
            log.WriteLog(LogLevel.Debug, "InitComponentTags, strRepTagsJSON :" + strRepTagsJSON);
            if (!string.IsNullOrEmpty(strRepTagsJSON))
            {
                TagsRepModel.Root tagsRepModel = JsonSerializer.LoadFromJson<TagsRepModel.Root>(strRepTagsJSON);
                if ((tagsRepModel.statusCode == "1003") && (tagsRepModel.data.Count > 0))
                {
                    foreach (TagsRepModel.DataItem rtag in tagsRepModel.data)
                    {
                        m_Tags.Add(new NxlRequestModel.TagsItem(rtag));
                    }
                }
                else if (tagsRepModel.statusCode == "5000")
                {
                    string strAPIAddTags = GetAddComponentTagAPI();
                    string strReqAddTag = "{\"key\":\"EM for Dynamics 365\",\"label\":\"EMDCRM\",\"type\":\"COMPONENT_TAG\",\"status\":\"ACTIVE\"}";
                    string strAddTagsJSON = SendPostRequestToConsole(strAPIAddTags, strCookie, "application/json", strReqAddTag, log);
                    if (!string.IsNullOrEmpty(strAddTagsJSON))
                    {
                        AddTagRepModel.Root addTagRepModel = JsonSerializer.LoadFromJson<AddTagRepModel.Root>(strAddTagsJSON);
                        if (addTagRepModel.statusCode == "1000")
                        {
                            m_Tags.Add(new NxlRequestModel.TagsItem(addTagRepModel.data, "EM for Dynamics 365", "EMDCRM", "COMPONENT_TAG", "ACTIVE"));
                        }
                        else
                        {
                            log.WriteLog(LogLevel.Debug, "InitComponentTags | Add tags failed");
                        }
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Error, "InitComponentTags | failed to add tag to cc");
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Debug, "InitComponentTags failed, tagsRepModel.statusCode: " + tagsRepModel.statusCode);
                }
            }
            else
            {
                log.WriteLog(LogLevel.Error, "InitComponentTags | failed to get tags from cc");
            }
        }
        private void InitPolicyTags(string strCookie, NextLabsCRMLogs log)
        {
            string strReqtags = "{\"tag\":{\"type\":\"POLICY_TAG\",\"label\":\"EMDCRM\"},\"pageNo\":0,\"pageSize\":65535}";
            string strAPIListTagsAddr = GetPolicyModelTagsAPI();
            string strRepTagsJSON = SendPostRequestToConsole(strAPIListTagsAddr, strCookie, "application/json", strReqtags, log);
            log.WriteLog(LogLevel.Debug, "InitPolicyTags, strRepTagsJSON :" + strRepTagsJSON);
            if (!string.IsNullOrEmpty(strRepTagsJSON))
            {
                TagsRepModel.Root tagsRepModel = JsonSerializer.LoadFromJson<TagsRepModel.Root>(strRepTagsJSON);
                if ((tagsRepModel.statusCode == "1003") && (tagsRepModel.data.Count > 0))
                {
                    foreach (TagsRepModel.DataItem rtag in tagsRepModel.data)
                    {
                        m_Tags.Add(new NxlRequestModel.TagsItem(rtag));
                    }
                }
                else if (tagsRepModel.statusCode == "5000")
                {
                    string strAPIAddTags = GetAddPolicyTagAPI();
                    string strReqAddTag = "{\"key\":\"EM for Dynamics 365\",\"label\":\"EMDCRM\",\"type\":\"POLICY_TAG\",\"status\":\"ACTIVE\"}";
                    string strAddTagsJSON = SendPostRequestToConsole(strAPIAddTags, strCookie, "application/json", strReqAddTag, log);
                    if (!string.IsNullOrEmpty(strAddTagsJSON))
                    {
                        AddTagRepModel.Root addTagRepModel = JsonSerializer.LoadFromJson<AddTagRepModel.Root>(strAddTagsJSON);
                        if (addTagRepModel.statusCode == "1000")
                        {
                            m_Tags.Add(new NxlRequestModel.TagsItem(addTagRepModel.data, "EM for Dynamics 365", "EMDCRM", "POLICY_MODEL_TAG", "ACTIVE"));
                        }
                        else
                        {
                            log.WriteLog(LogLevel.Debug, "InitPolicyTags | Add tags failed");
                        }
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Error, "InitPolicyTags | failed to add tag to cc");
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Debug, "InitPolicyTags failed, tagsRepModel.statusCode: " + tagsRepModel.statusCode);
                }
            }
            else
            {
                log.WriteLog(LogLevel.Error, "InitPolicyTags | failed to get tags from cc");
            }
        }
        private void InitTagsPriorTocc87(string strCookie, NextLabsCRMLogs log)
        {
            string strReqtags = "{\"tag\":{\"type\":\"POLICY_MODEL_TAG\",\"label\":\"EMDCRM\"},\"pageNo\":0,\"pageSize\":65535}";
            string strAPIListTagsAddr = GetPolicyModelTagsAPI();
            string strRepTagsJSON = SendPostRequestToConsole(strAPIListTagsAddr, strCookie, "application/json", strReqtags, log);
            log.WriteLog(LogLevel.Debug, "InitTagsPriorTocc87, strRepTagsJSON :" + strRepTagsJSON);
            if (!string.IsNullOrEmpty(strRepTagsJSON))
            {
                TagsRepModel.Root tagsRepModel = JsonSerializer.LoadFromJson<TagsRepModel.Root>(strRepTagsJSON);
                if ((tagsRepModel.statusCode == "1003") && (tagsRepModel.data.Count > 0))
                {
                    foreach (TagsRepModel.DataItem rtag in tagsRepModel.data)
                    {
                        m_Tags.Add(new NxlRequestModel.TagsItem(rtag));
                    }
                }
                else if (tagsRepModel.statusCode == "5000")
                {
                    string strAPIAddTags = CreatePolicyModelTagsAPI();
                    string strReqAddTag = "{\"key\":\"EM for Dynamics 365\",\"label\":\"EMDCRM\",\"type\":\"POLICY_MODEL_TAG\",\"status\":\"ACTIVE\"}";
                    string strAddTagsJSON = SendPostRequestToConsole(strAPIAddTags, strCookie, "application/json", strReqAddTag, log);
                    if (!string.IsNullOrEmpty(strAddTagsJSON))
                    {
                        AddTagRepModel.Root addTagRepModel = JsonSerializer.LoadFromJson<AddTagRepModel.Root>(strAddTagsJSON);
                        if (addTagRepModel.statusCode == "1000")
                        {
                            m_Tags.Add(new NxlRequestModel.TagsItem(addTagRepModel.data, "EM for Dynamics 365", "EMDCRM", "POLICY_MODEL_TAG", "ACTIVE"));
                        }
                        else
                        {
                            log.WriteLog(LogLevel.Debug, "InitTagsPriorTocc87 | Add tags failed");
                        }
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Error, "InitTagsPriorTocc87 | failed to add tag to cc");
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Debug, "InitTagsPriorTocc87 failed, tagsRepModel.statusCode: " + tagsRepModel.statusCode);
                }
            }
            else
            {
                log.WriteLog(LogLevel.Error, "InitTagsPriorTocc87 | failed to get tags from cc");
            }
        }
        private void InitTagsPostcc87(string strCookie, NextLabsCRMLogs log)
        {// after cc8.7, we have to call PM, component, policy seperately.
            InitPolicyModelTags(strCookie, log);
            InitComponentTags(strCookie, log);
            InitPolicyTags(strCookie, log);
        }
        private void InitTags(string strCookie, NextLabsCRMLogs log)
        {
            if (versionPriorTo87())
            {
                InitTagsPriorTocc87(strCookie, log);
            }
            else
            {
                InitTagsPostcc87(strCookie, log);
            }

        }
        private void InitOperatorConfigs(string strCookie, NextLabsCRMLogs log)
        {
            try
            {
                Dictionary<string, int> dicStringOperator = new Dictionary<string, int>();
                Dictionary<string, int> dicNumberOperator = new Dictionary<string, int>();
                Dictionary<string, int> dicMultivalOperator = new Dictionary<string, int>();

                string strOpStringUrl = GetPolicyModelStringOperatorAPI();
                string strOpString = SendGetRequestToConsole(strOpStringUrl, strCookie, log);
                PolicyModelOperators.StringOperator OpString = JsonSerializer.LoadFromJson<PolicyModelOperators.StringOperator>(strOpString);
                foreach (PolicyModelOperators.OperatorElem datum in OpString.data)
                {
                    dicStringOperator[datum.key] = datum.id;
                    //log.WriteLog(LogLevel.Debug, "InitOperatorConfigs|OpString|key = " + datum.key + ", id = " + datum.id);
                }
                m_dicOperators["STRING"] = dicStringOperator;

                string strOpNumberUrl = GetPolicyModelNumberOperatorAPI();
                string strOpNumber = SendGetRequestToConsole(strOpNumberUrl, strCookie, log);
                PolicyModelOperators.NumberOperator OpNumber = JsonSerializer.LoadFromJson<PolicyModelOperators.NumberOperator>(strOpNumber);
                foreach (PolicyModelOperators.OperatorElem datum in OpNumber.data)
                {
                    dicNumberOperator[datum.key] = datum.id;
                    //log.WriteLog(LogLevel.Debug, "InitOperatorConfigs|OpNumber|key = " + datum.key + ", id = " + datum.id);
                }
                m_dicOperators["NUMBER"] = dicNumberOperator;
                string strOpMultivalUrl = GetPolicyModelMultivalOperatorAPI();
                string strOpMultival = SendGetRequestToConsole(strOpMultivalUrl, strCookie, log);
                PolicyModelOperators.MultivalOperator OpMultival = JsonSerializer.LoadFromJson<PolicyModelOperators.MultivalOperator>(strOpMultival);
                foreach (PolicyModelOperators.OperatorElem datum in OpMultival.data)
                {
                    dicMultivalOperator[datum.key] = datum.id;
                    //log.WriteLog(LogLevel.Debug, "InitOperatorConfigs|OpMultival|key = " + datum.key + ", id = " + datum.id);
                }
                m_dicOperators["MULTIVAL"] = dicMultivalOperator;
            }
            catch (Exception e)
            {
                log.WriteLog(LogLevel.Debug, "InitOperatorConfigs|trycatch : " + e.ToString());
            }
        }
        private void GetSystemVersion(string strCookie, NextLabsCRMLogs log)
        {
            try
            {
                string strSvUrl = GetSystemVersionAPI();
                string strSvString = SendGetRequestToConsole(strSvUrl, strCookie, log);
                log.WriteLog(LogLevel.Debug, "GetSystemVersion :" + strSvString);

                if (!string.IsNullOrEmpty(strSvString))
                {
                    SystemVersion.Root SvString = JsonSerializer.LoadFromJson<SystemVersion.Root>(strSvString);

                    if (SvString.statusCode == "1004")
                    {
                        m_version = SvString.data;
                    }
                    else
                    {
                        log.WriteLog(LogLevel.Debug, "GetSystemVersion failed, statusCode: " + SvString.statusCode);
                    }
                }
                else
                {
                    log.WriteLog(LogLevel.Error, "failed to get system version from cc");
                }
            }
            catch (Exception e)
            {
                log.WriteLog(LogLevel.Debug, "GetSystemVersion|trycatch : " + e.ToString());
            }
        }
        //Bug 54930 - Unable to import action to CC automatically when secured entities(CC87) 
        //SearchComponent for cc87, cc priored to 87 can create actions when syncing policy model
        //public bool SearchActionComponents(string strModelType, string strCookie, NextLabsCRMLogs log)
        //{
        //    string strAPISearchComponent = GetListComponentsAPI();
        //    string strReqJSON = string.Format("{{\"criteria\":{{\"fields\":[{{\"field\":\"group\",\"type\":\"SINGLE\",\"value\":{{\"type\":\"String\",\"value\":\"ACTION\"}}}},{{\"field\":\"modelType\",\"type\":\"SINGLE\",\"value\":{{\"type\":\"String\",\"value\":\"{0}\"}}}}],\"sortFields\":[{{\"field\":\"lastUpdatedDate\",\"order\":\"DESC\"}}],\"pageNo\":0,\"pageSize\":65535}}}}", strModelType);
        //    log.WriteLog(LogLevel.Debug, "SearchActionComponents | strReqJSON : " + strReqJSON);
        //    string ComponentsList = SendPostRequestToConsole(strAPISearchComponent, strCookie, "application/json", strReqJSON, log);
        //    log.WriteLog(LogLevel.Debug, "SearchActionComponents | ComponentsList : " + ComponentsList);
        //    NxlListActionResponse.Root ListComponentsRepModel = JsonSerializer.LoadFromJson<NxlListActionResponse.Root>(ComponentsList);
        //    log.WriteLog(LogLevel.Debug, "SearchActionComponents | CreateComponentsRepModel : " + ListComponentsRepModel.statusCode + ", " + ListComponentsRepModel.message + ", " + ListComponentsRepModel.data);
        //    //Generally, Actions are fixed, so if count > 0, we consider the Action is existed. 
        //    if (ListComponentsRepModel.statusCode == "1004" && ListComponentsRepModel.data.Count > 0) return true;
        //    else return false;
        //}
        //CreatComponent for cc87, cc priored to 87 can create actions when syncing policy model
        //public bool CreatActionComponent(LocalPolicyModel.ActionsItem action, List<NxlRequestModel.TagsItem> tags, int pmid, string pmname, string pmshortname, string strCookie, NextLabsCRMLogs log)
        //{            
        //    string strAPICreateComponent = GetCreateComponentAPI();
        //    string strReqJSON = string.Format("{{\"id\":null,\"name\":\"{0}\",\"description\":null,\"tags\":[{{\"id\":{1},\"key\":\"{2}\",\"label\":\"{3}\",\"type\":\"{4}\",\"status\":\"{5}\"}}],\"type\":\"ACTION\",\"conditions\":[],\"actions\":[\"{6}\"],\"subComponents\":[],\"version\":null,\"policyModel\":{{\"id\":{7},\"name\":\"{8}\",\"shortName\":\"{9}\"}},\"status\":\"DRAFT\"}}",
        //                                      action.name.ToUpper(), tags[0].id, tags[0].key, tags[0].label, tags[0].type, tags[0].status, action.shortName, pmid, pmname, pmshortname);
        //    string ComponentsList = SendPostRequestToConsole(strAPICreateComponent, strCookie, "application/json", strReqJSON, log);
        //    NxlCreateActionResponse.Root CreateComponentsRepModel = JsonSerializer.LoadFromJson<NxlCreateActionResponse.Root>(ComponentsList);
        //    log.WriteLog(LogLevel.Debug, "CreatActionComponent | CreateComponentsRepModel : " + CreateComponentsRepModel.statusCode + ", " + CreateComponentsRepModel.message + ", " + CreateComponentsRepModel.data);
        //    if (CreateComponentsRepModel.statusCode == "1000") return true;
        //    else return false;
        //}
        //public bool CreatActionComponent(LocalPolicyModel.ActionsItem action, List<NxlRequestModel.TagsItem> tags)
        //{
        //    NxlRequestModel.AddComponent ReqComponent = new NxlRequestModel.AddComponent(tags);

        //    return false;
        //}
        public bool SyncPolicyModel(string policyContent, NextLabsCRMLogs log, ref string strRep)
        {
            try
            {
                string UserName = m_ccusername;
                string Password = m_ccpassword;
                string strUrl = GetCC() + "/console";
                string strConsoleCookie = LoginWebConsole(strUrl, UserName, Password, log);
                GetSystemVersion(strConsoleCookie, log);
                InitOperatorConfigs(strConsoleCookie, log);
                InitTags(strConsoleCookie, log);

                LocalPolicyModel.Root localPolicyModelList = JsonSerializer.LoadFromJson<LocalPolicyModel.Root>(policyContent);

                string postData = "{\"criteria\":{\"fields\":[{\"field\":\"type\",\"type\":\"MULTI\",\"value\":{\"type\":\"String\",\"value\":[\"RESOURCE\",\"SUBJECT\"]}}],\"sortFields\":[{\"field\":\"type\",\"order\":\"DESC\"},{\"field\":\"lastUpdatedDate\",\"order\":\"DESC\"}],\"pageNo\":0,\"pageSize\":65535}}";
                string strAPISerarchPolicyModel = GetQueryPolicyModelAPI();
                string policyModelList = SendPostRequestToConsole(strAPISerarchPolicyModel, strConsoleCookie, "application/json", postData, log);
                PolicyModelRepModel.Root policyModelRepModel = JsonSerializer.LoadFromJson<PolicyModelRepModel.Root>(policyModelList);

                List<NxlRequestModel.TagsItem> tempTags = new List<NxlRequestModel.TagsItem>();
                foreach (NxlRequestModel.TagsItem tag in m_Tags)
                {
                    if (tag.type.Equals("COMPONENT_TAG"))
                    {
                        tempTags.Add(tag);
                        log.WriteLog(LogLevel.Debug, "TryModifyPolicyModel | CreatActionComponent | tag label : " + tag.label + ", type : " + tag.type);
                    }
                }

                foreach (LocalPolicyModel.PolicyModelsItem localPolicyModel in localPolicyModelList.policyModels)
                {
                    string policyModelName = localPolicyModel.shortName;
                    bool isHaved = false;
                    foreach (PolicyModelRepModel.DataItem policymodel in policyModelRepModel.data)
                    {
                        string shortName = policymodel.shortName;
                        if (policyModelName.ToLower() == shortName)
                        {
                            isHaved = true;
                            // to check modify necessary
                            int id = policymodel.id;
                            TryModifyPolicyModel(id, localPolicyModel, strConsoleCookie, log, ref strRep);
                            //if (TryModifyPolicyModel(id, localPolicyModel, strConsoleCookie, log, ref strRep))
                            //{
                            //    log.WriteLog(LogLevel.Debug, "Modify | policymodel.name : " + policymodel.name);
                            //    if (!SearchActionComponents(policymodel.name, strConsoleCookie, log))
                            //    {
                            //        foreach (LocalPolicyModel.ActionsItem act in localPolicyModel.actions)
                            //        {
                            //            CreatActionComponent(act, tempTags, policymodel.id, policymodel.name, policymodel.shortName, strConsoleCookie, log);
                            //        }
                            //    }
                            //}
                        }
                    }
                    if (!isHaved)
                    {
                        // the model is new,to do add
                        AddPolicyModel(localPolicyModel, strConsoleCookie, log, ref strRep);
                        //int pmid = AddPolicyModel(localPolicyModel, strConsoleCookie, log, ref strRep);
                        //if (pmid != -1 && !versionPriorTo87() && !SearchActionComponents(localPolicyModel.name, strConsoleCookie, log))
                        //{
                        //    foreach (LocalPolicyModel.ActionsItem act in localPolicyModel.actions)
                        //    {
                        //        CreatActionComponent(act, tempTags, pmid, localPolicyModel.name, localPolicyModel.shortName, strConsoleCookie, log);
                        //    }
                        //}
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.WriteLog(LogLevel.Debug, "Exception:" + ex.ToString());
                return false;
            }
        }
        private PolicyModelDetail.Data GetPolicyModelFromCC(int id, string strConsoleCookie, NextLabsCRMLogs log)
        {
            string strAPISerarchPolicyModelDetail = GetQueryPolicyModelDetailAPI() + id;
            string strPolicyModel = SendGetRequestToConsole(strAPISerarchPolicyModelDetail, strConsoleCookie, log);
            PolicyModelDetail.Root policyModelDetail = JsonSerializer.LoadFromJson<PolicyModelDetail.Root>(strPolicyModel);
            return policyModelDetail.data;
            //log.WriteLog(LogLevel.Debug,"strPolicyModelDEtail : "+strPolicyModel);
        }

        private void TryModifyAttributes(NxlRequestModel.AddPolicyModel ReqPolicyModel, List<LocalPolicyModel.AttributesItem> LocalAttributes, ref bool bmodified)
        {
            foreach (LocalPolicyModel.AttributesItem lattr in LocalAttributes)
            {
                bool isIn = false;
                foreach (NxlRequestModel.AttributesItem rattr in ReqPolicyModel.attributes)
                {
                    if (lattr.shortName == rattr.shortName && lattr.dataType == lattr.dataType)
                    {
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    bmodified = true;
                    int sortorder = ReqPolicyModel.attributes.Count;
                    ReqPolicyModel.attributes.Add(new NxlRequestModel.AttributesItem(lattr, sortorder + 1, m_dicOperators));
                }
            }
        }

        private void TryModifyActions(NxlRequestModel.AddPolicyModel ReqPolicyModel, List<LocalPolicyModel.ActionsItem> LocalActions, ref bool bmodified)
        {
            foreach (LocalPolicyModel.ActionsItem laction in LocalActions)
            {
                bool isIn = false;
                foreach (NxlRequestModel.ActionsItem raction in ReqPolicyModel.actions)
                {
                    if (laction.shortName == raction.shortName)
                    {
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    bmodified = true;
                    int sortorder = ReqPolicyModel.actions.Count;
                    ReqPolicyModel.actions.Add(new NxlRequestModel.ActionsItem(laction, sortorder + 1));
                }
            }
        }

        private string TryMergelistValues(string listValues, string rlistValues)
        {
            List<string> MergedValues = new List<string>();
            if (listValues.Length > 0)
            {
                string[] Values = listValues.Split(',');
                foreach (string lvalue in Values)
                {
                    if (!MergedValues.Contains(lvalue))
                    {
                        MergedValues.Add(lvalue);
                    }
                }
            }
            if (rlistValues.Length > 0)
            {
                string[] rValues = rlistValues.Split(',');
                foreach (string value in rValues)
                {
                    if (!MergedValues.Contains(value))
                    {
                        MergedValues.Add(value);
                    }
                }
            }
            string ret = "";
            if (MergedValues.Count > 0)
            {
                ret = string.Join(",", MergedValues.ToArray());
            }
            return ret;
        }

        private void TryModifyObParams(NxlRequestModel.ObligationsItem ReqObligation, List<LocalPolicyModel.ParametersItem> LocalObParams, ref bool bmodified)
        {
            foreach (LocalPolicyModel.ParametersItem lparam in LocalObParams)
            {
                bool isIn = false;
                int len = ReqObligation.parameters.Count;
                for(int i = 0; i < len; ++i)
                {
                    if ((lparam.shortName == ReqObligation.parameters[i].shortName) && (lparam.type == ReqObligation.parameters[i].type))
                    {
                        if (ReqObligation.parameters[i].type == "LIST")
                        {
                            int beforelen = ReqObligation.parameters[i].listValues.Length;
                            ReqObligation.parameters[i].listValues = TryMergelistValues(lparam.listValues, ReqObligation.parameters[i].listValues);
                            if (ReqObligation.parameters[i].listValues.Length != beforelen) {
                                bmodified = true;
                            }
                        }
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    bmodified = true;
                    int sortorder = ReqObligation.parameters.Count;
                    ReqObligation.parameters.Add(new NxlRequestModel.ParametersItem(lparam, sortorder + 1));
                }
            }
        }

        public void TryModifyObligations(NxlRequestModel.AddPolicyModel ReqPolicyModel, List<LocalPolicyModel.ObligationsItem> obligations, ref bool bmodified)
        {
            foreach (LocalPolicyModel.ObligationsItem lob in obligations)
            {
                bool isIn = false;
                int len = ReqPolicyModel.obligations.Count;
                for (int i = 0; i < len; ++i)
                {
                    if (lob.shortName == ReqPolicyModel.obligations[i].shortName)
                    {
                        TryModifyObParams(ReqPolicyModel.obligations[i], lob.parameters, ref bmodified);
                        isIn = true;
                        break;
                    }
                }
                if (!isIn)
                {
                    bmodified = true;
                    int sortorder = ReqPolicyModel.obligations.Count;
                    ReqPolicyModel.obligations.Add(new NxlRequestModel.ObligationsItem(lob, sortorder + 1));
                }
            }
        }

        private bool TryModifyPolicyModel(int id, LocalPolicyModel.PolicyModelsItem localModel, string strConsoleCookie, NextLabsCRMLogs log, ref string strRep)
        {
            //log.WriteLog(LogLevel.Debug, "TryModifyPolicyModel start!");
            bool isNeedModify = false;

            PolicyModelDetail.Data ProtoPolicyModel = GetPolicyModelFromCC(id, strConsoleCookie, log);
            NxlRequestModel.AddPolicyModel ReqPolicyModel = new NxlRequestModel.AddPolicyModel(ProtoPolicyModel);

            if (ReqPolicyModel.shortName.ToLower() != "user")
            {
                foreach (NxlRequestModel.TagsItem tag in m_Tags)
                {
                    if (tag.type.Equals("POLICY_MODEL_TAG"))
                    {
                        ReqPolicyModel.tags.Add(tag);
                        log.WriteLog(LogLevel.Debug, "TryModifyPolicyModel tag label : " + tag.label + ", type : " + tag.type);
                    }
                }
            }

            TryModifyAttributes(ReqPolicyModel, localModel.attributes, ref isNeedModify);
            TryModifyActions(ReqPolicyModel, localModel.actions, ref isNeedModify);
            TryModifyObligations(ReqPolicyModel, localModel.obligations, ref isNeedModify);

            if (isNeedModify)
            {
                string addData = JsonSerializer.SaveToJson(ReqPolicyModel);
                //log.WriteLog(LogLevel.Debug, "addData:" + addData);
                string strAPIModifyPolicyModel = GetModifyPolicyModelAPI();
                string result = SendPutRequestToConsole(strAPIModifyPolicyModel, strConsoleCookie, "application/json", addData, log);
                log.WriteLog(LogLevel.Debug, "TryModifyPolicyModel" + result);
                RepStatusModel.Root repStatus = JsonSerializer.LoadFromJson<RepStatusModel.Root>(result);
                if (repStatus.statusCode == "1001")
                {
                    strRep = "OK";
                    return true;
                }
                else
                {
                    strRep = repStatus.message;
                    return false;
                }
                //log.WriteLog(LogLevel.Debug, "modify result :repStatus.statusCode " + repStatus.statusCode);
            }
            else {
                strRep = "OK";
                return true;
            }

            //log.WriteLog(LogLevel.Debug, "TryModifyPolicyModel end!");
        }

        public int AddPolicyModel(LocalPolicyModel.PolicyModelsItem localModel, string strConsoleCookie, NextLabsCRMLogs log, ref string strRep)
        {
            //log.WriteLog(LogLevel.Debug, "AddPolicyModel start!");
            NxlRequestModel.AddPolicyModel ReqPolicyModel = new NxlRequestModel.AddPolicyModel(localModel, m_dicOperators);
            if (ReqPolicyModel.shortName.ToLower() != "user")
            {
                foreach (NxlRequestModel.TagsItem tag in m_Tags)
                {
                    if (tag.type.Equals("POLICY_MODEL_TAG"))
                    {
                        ReqPolicyModel.tags.Add(tag);
                        log.WriteLog(LogLevel.Debug, "AddPolicyModel tag label : " + tag.label + ", type : " + tag.type);
                    }
                }
            }

            string addData = JsonSerializer.SaveToJson(ReqPolicyModel);
            string strAPIAddPolicyModel = GetAddPolicyModelAPI();
            string result = SendPostRequestToConsole(strAPIAddPolicyModel, strConsoleCookie, "application/json", addData, log);
            log.WriteLog(LogLevel.Debug, "AddPolicyModel" + result);
            AddTagRepModel.Root repStatus = JsonSerializer.LoadFromJson<AddTagRepModel.Root>(result);
            if (repStatus.statusCode == "1000")
            {
                strRep = "OK";
                return repStatus.data;
            }
            else
            {
                strRep = repStatus.message;
                return -1;  //add policy model failed
            }
            //log.WriteLog(LogLevel.Debug, "add result :repStatus.statusCode " + repStatus.statusCode);
            //log.WriteLog(LogLevel.Debug, "AddPolicyModel end!");
        }
    }
}