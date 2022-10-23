using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NextLabs.JavaPC.RestAPISDK.RestModel.Single
{
    [DataContract]
    public class RestRequest
    {
        [DataMember]
        public RequestNode Request { get; set; }

        public override string ToString()
        {
            return JsonSerializer.SaveToJson(this);
        }
    }
    [DataContract]
    public class RequestNode
    {
        [DataMember]
        public string ReturnPolicyIdList { get; set; }

        [DataMember]
        public List<CategoryNode> Category { get; set; }
    }

    [DataContract]
    public class CategoryNode
    {
        [DataMember]
        public string CategoryId { get; set; }

        //[DataMember]
        //public string Id { get; set; }

        [DataMember]
        public List<AttributeNode> Attribute { get; set; }
    }
    [DataContract]
    public class AttributeNode
    {
        public AttributeNode()
        {
            Value = new List<string>();
        }
        [DataMember]
        public string AttributeId { get; set; }

        [DataMember]
        public List<string> Value { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public bool IncludeInResult { get; set; }
    }



    [DataContract]
    public class RestResponse
    {
        [DataMember]
        public ResponseNode Response { get; set; }
    }
    [DataContract]
    public class ResponseNode
    {
        [DataMember]
        public List<ResultNode> Result { get; set; }
    }
    [DataContract]
    public class NewRestResponse
    {
        [DataMember]
        public List<ResultNode> Response { get; set; }
    }
    [DataContract]
    public class ResultNode
    {
        [DataMember]
        public string Decision { get; set; }

        [DataMember]
        public StatusNode Status { get; set; }

        [DataMember]
        public List<ObligationsNode> Obligations { get; set; }
    }
    [DataContract]
    public class StatusNode
    {
        [DataMember]
        public string StatusMessage { get; set; }
        [DataMember]
        public StatusCodeNode StatusCode { get; set; }
    }
    [DataContract]
    public class StatusCodeNode
    {
        [DataMember]
        public string Value { get; set; }
    }
    [DataContract]
    public class ObligationsNode
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public List<AttributeAssignmentNode> AttributeAssignment { get; set; }
    }
    [DataContract]
    public class AttributeAssignmentNode
    {
        [DataMember]
        public string AttributeId { get; set; }
        [DataMember]
        public List<string> Value { get; set; }
    }
}

namespace NextLabs.JavaPC.RestAPISDK.RestModel.Authorized
{
    
    public class TokenRequest
    {
        public TokenRequest()
        {
            GrantType = "client_credentials";
            ExpiresIn = 3600;
        }
        public TokenRequest(string strClientId, string strClientSecret, int iExpiresIn)
        {
            GrantType = "client_credentials";
            ClientId = strClientId;
            ClientSecret = strClientSecret;
            ExpiresIn = iExpiresIn;
        }
        public string GrantType { get; set; }
        public string ClientId {get;set;}
        public string ClientSecret {get;set;}
        public int ExpiresIn { get; set; }
        public override string ToString()
        {
            return string.Format("grant_type={0}&client_id={1}&client_secret={2}&expires_in={3}", GrantType, ClientId, ClientSecret, ExpiresIn);
        }
    }

    [DataContract]
    public class TokenResponse
    {
        [DataMember]
        public string access_token { get; set; }

        [DataMember]
        public string token_type { get; set; }

        [DataMember]
        public int expires_in { get; set; }

        public override string ToString()
        {
            return JsonSerializer.SaveToJson(this);
        }
    }
}
namespace NextLabs.JavaPC.RestAPISDK.CEModel
{
    public enum CENoiseLevel
    {
        CE_NOISE_LEVEL_MIN = 0, /**< Minimum */
        CE_NOISE_LEVEL_SYSTEM = 1, /**< System */
        CE_NOISE_LEVEL_APPLICATION = 2, /**< Application */
        CE_NOISE_LEVEL_USER_ACTION = 3, /**< User Action */
        CE_NOISE_LEVEL_MAX = 4  /**< Maximum */
    }
    enum CEResourceType
    {
        Source = 0,
        Destination = 1,
        NameAttributes = 2
    }

    public enum CEResponse
    {
        CEDeny = 0,
        CEAllow = 1,
        CEDontCare = 2
    }

    public enum CEAttributeType
    {
        XACML_String,
        XACML_Boolean,
        XACML_Integer,
        XACML_Double,
        XACML_Time,
        XACML_Date,
        XACML_DateTime,
        XACML_DayTimeDuration,
        XACML_YearMonthDuration,
        XACML_AnyURI,
        XACML_HexBinary,
        XACML_Base64Binary,
        XACML_Rfc822Name,
        XACML_X500Name,
        XACML_IpAddress,
        XACML_DnsName,
        XACML_XpathExpression,
        NXL_List

    }

    public class CEAttribute
    {
        public const string SperaterForListValue = ";N*X*L;";
        public string Name { get; set; }
        public string Value { get; set; }
        public CEAttributeType Type { get; set; }
        public CEAttribute(string strName, string strValue, CEAttributeType ceAttributeType)
        {
            Name = strName;
            Value = strValue;
            Type = ceAttributeType;
        }
    }

    public class CEAttres
    {
        List<CEAttribute> m_lisKeyValuePair = null;
        public CEAttres()
        {
            m_lisKeyValuePair = new List<CEAttribute>();
        }
        public void Add_Attre(string strName, string strValue, CEAttributeType ceAttributesType)
        {
            CEAttribute pair = new CEAttribute(strName, strValue, ceAttributesType);
            m_lisKeyValuePair.Add(pair);
        }
        public void Add_Attre(CEAttribute ceAttribute)
        {
            m_lisKeyValuePair.Add(ceAttribute);
        }

        public void Add_Attres(CEAttres other)
        {
            if (other != null && other.m_lisKeyValuePair != null)
            {
                m_lisKeyValuePair.AddRange(other.m_lisKeyValuePair);
            }
        }
        public void Get_Attre(int nIndex, out string strName, out string strValue,out CEAttributeType ceAttributesType)
        {
            strName = null;
            strValue = null;
            ceAttributesType = CEAttributeType.XACML_String;
            if (nIndex < m_lisKeyValuePair.Count)
            {
                strName = m_lisKeyValuePair[nIndex].Name;
                strValue = m_lisKeyValuePair[nIndex].Value;
                ceAttributesType = m_lisKeyValuePair[nIndex].Type;
            }

        }
        public int get_count()
        {
            return m_lisKeyValuePair.Count;
        }
    }

    public class CEApp
    {
        public CEApp() { }
        public CEApp(string strName, string strPath, string strUrl, CEAttres attres)
        {
            Name = strName;
            Path = strPath;
            Url = strUrl;
            Attres = attres;
        }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public CEAttres Attres { get; set; }
    }
    public class CEUser
    {
        public CEUser() { }
        public CEUser(string strSid, string strName, CEAttres attres)
        {
            Sid = strSid;
            Name = strName;
            Attres = attres;
        }
        public string Sid { get; set; }
        public string Name { get; set; }
        public CEAttres Attres { get; set; }
    }
    public class CEHost
    {
        public CEHost() { }
        public string IPAddress { get; set; }
        public string Name { get; set; }
        public CEAttres Attres { get; set; }
        public CEHost(string strName,string strIPAddress,CEAttres attres)
        {
            Name = strName;
            IPAddress = strIPAddress;
            Attres = attres;
        }
    }
    public class CEResource
    {
        public CEResource() { }
        public CEResource(string strName, string strType, CEAttres attres)
        {
            SourceName = strName;
            SourceType = strType;
            Attres = attres;
        }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
        public CEAttres Attres { get; set; }
    }
    public class CERequest
    {
        private string m_strAction = null;
        private CEApp m_ceApp = null;
        private CENoiseLevel m_ceNotiseLevel;
        private CEUser m_ceUser = null;
        private CEHost m_ceHost = null;
        private bool m_PerformObligation;
        private CEResource m_ceSource = null;
        private CEResource m_ceDest = null;
        private CEAttres m_ceNameAttributes = null;
        private CEUser m_ceRecipient = null;
        private List<string> m_lisRecipients = null;
        public CERequest()
        {

        }

        public void Set_Action(string strAction)
        {
            m_strAction = strAction;
        }
        public string Get_Action()
        {
            return m_strAction;
        }
        public void Set_App(string strName, string strPath, string strUrl, CEAttres ceAttres)
        {
            if (m_ceApp == null)
            {
                m_ceApp = new CEApp();
            }
            m_ceApp.Name = strName;
            m_ceApp.Path = strPath;
            m_ceApp.Url = strUrl;
            m_ceApp.Attres = ceAttres;
        }
        public CEApp Get_App()
        {
            return m_ceApp;
        }
        public void set_noiseLevel(CENoiseLevel notiseLevel)
        {
            m_ceNotiseLevel = notiseLevel;
        }
        public CENoiseLevel Get_noiseLevel()
        {
            return m_ceNotiseLevel;
        }
        public void Set_User(string strSid, string strName, CEAttres ceAttres)
        {
            if (m_ceUser == null)
            {
                m_ceUser = new CEUser();
            }
            m_ceUser.Sid = strSid;
            m_ceUser.Name = strName;
            m_ceUser.Attres = ceAttres;
        }
        public CEUser Get_User()
        {
            return m_ceUser;
        }
        public CEHost Get_Host()
        {
            return m_ceHost;
        }
        public void Set_Host(string strName,string strIPAddress,CEAttres ceAttres)
        {
            if(m_ceHost==null)
            {
                m_ceHost = new CEHost();
            }
            m_ceHost.Name = strName;
            m_ceHost.IPAddress = strIPAddress;
            m_ceHost.Attres = ceAttres;
        }
        [Obsolete] 
        public void Set_PerformObligation(bool bPerformObligation)
        {
            m_PerformObligation = bPerformObligation;
        }
        public bool Get_PerformObligation()
        {
            return m_PerformObligation;
        }
        public void Set_Source(string strSourceName, string strSourceType, CEAttres ceAttres)
        {
            if (m_ceSource == null)
            {
                m_ceSource = new CEResource();
            }
            m_ceSource.SourceName = strSourceName;
            m_ceSource.SourceType = strSourceType;
            m_ceSource.Attres = ceAttres;
        }
        public CEResource Get_Source()
        {
            return m_ceSource;
        }
        public void Set_Dest(string strDestName, string strDestType, CEAttres ceAttres)
        {
            if (m_ceDest == null)
            {
                m_ceDest = new CEResource();
            }
            m_ceDest.SourceName = strDestName;
            m_ceDest.SourceType = strDestType;
            m_ceDest.Attres = ceAttres;
        }
        public CEResource Get_Dest()
        {
            return m_ceDest;
        }
        public void Set_NameAttributes(string strName, string strValue, CEAttributeType ceAttributes)
        {
            if (m_ceNameAttributes == null)
            {
                m_ceNameAttributes = new CEAttres();
            }
            m_ceNameAttributes.Add_Attre(strName, strValue, ceAttributes);
        }
        public CEAttres Get_NameAttributes()
        {
            return m_ceNameAttributes;
        }
        public void Set_Recipient(string strSid, string strName, CEAttres ceAttres)
        {
            if (m_ceRecipient == null)
            {
                m_ceRecipient = new CEUser();
            }
            m_ceRecipient.Sid = strSid;
            m_ceRecipient.Name = strName;
            m_ceRecipient.Attres = ceAttres;
        }
        public void Set_Recipient(params string[] strEmailAddresses)
        {
            if(m_lisRecipients==null)
            {
                m_lisRecipients = new List<string>();
            }
            foreach (string strEmailAddress in strEmailAddresses)
            {
                m_lisRecipients.Add(strEmailAddress);
            }
        }
        public CEUser Get_Recipient()
        {
            return m_ceRecipient;
        }
        public List<string> Get_Recipients()
        {
            return m_lisRecipients;
        }



    }

    public class CEObligation
    {
        private CEAttres m_CEAttres = null;
        private string m_ObligationName = string.Empty;
        private string m_PolicyName = string.Empty;
        public CEObligation(string strObligationnName, CEAttres ceAttres, string strPolicyName)
        {
            m_CEAttres = ceAttres;
            m_ObligationName = strObligationnName;
            m_PolicyName = strPolicyName;
        }
        public CEAttres GetCEAttres()
        {
            return m_CEAttres;
        }
        public string Get_Nmae()
        {
            return m_ObligationName;
        }
        public string Get_PolicyName()
        {
            throw new NotImplementedException("Sorry , javaPC can not get policy name at this version . by bear 2016.12.11");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Obligation name:"+m_ObligationName+System.Environment.NewLine);
            //sb.Append("Policy name:" + m_PolicyName+ System.Environment.NewLine);
            for(int i=0;i<m_CEAttres.get_count();i++)
            {
                string strKey, strValue = string.Empty;
                CEAttributeType ceType;
                m_CEAttres.Get_Attre(i, out strKey, out strValue,out ceType);
                sb.Append("Key:" + strKey + " Value:" + strValue + " Type:" + ceType.ToString()+ System.Environment.NewLine);
            }

            return sb.ToString();
        }
    }




    //public class CEObligation;
}
namespace NextLabs.JavaPC.RestAPISDK.ModelTransform
{
    class Function
    {
        public static CEModel.CEResponse TransformToCEResponse(string strRestResponse)
        {
            CEModel.CEResponse result = CEModel.CEResponse.CEDontCare;
            if (strRestResponse.Equals(Constant.RestResponse.Deny , StringComparison.OrdinalIgnoreCase))
            {
                result = CEModel.CEResponse.CEDeny;
            }
            else if (strRestResponse.Equals(Constant.RestResponse.Allow, StringComparison.OrdinalIgnoreCase))
            {
                result = CEModel.CEResponse.CEAllow;
            }
            return result;
        }

        public static string TransformCEAttributeTypeToString(CEModel.CEAttributeType ceAttributeType)
        {
            string strResult = string.Empty;
            switch (ceAttributeType)
            {
                case CEModel.CEAttributeType.XACML_AnyURI:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#anyURI";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Base64Binary:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#base64Binary";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Boolean:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#boolean";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Date:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#date";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_DateTime:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#dateTime";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_DayTimeDuration:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#dayTimeDuration";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_DnsName:
                    {
                        strResult = "urn:oasis:names:tc:xacml:2.0:data-type:dnsName";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Double:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#double";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_HexBinary:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#hexBinary";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Integer:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#integer";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_IpAddress:
                    {
                        strResult = "urn:oasis:names:tc:xacml:2.0:data-type:ipAddress";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Rfc822Name:
                    {
                        strResult = "urn:oasis:names:tc:xacml:1.0:data-type:rfc822Name";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_String:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#string";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_Time:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#time";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_X500Name:
                    {
                        strResult = "urn:oasis:names:tc:xacml:1.0:data-type:x500Name";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_XpathExpression:
                    {
                        strResult = "urn:oasis:names:tc:xacml:3.0:data-type:xpathExpression";
                    }
                    break;
                case CEModel.CEAttributeType.XACML_YearMonthDuration:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#yearMonthDuration";
                    }
                    break;
                case CEModel.CEAttributeType.NXL_List:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#string";
                    }
                    break;
                default:
                    {
                        strResult = "http://www.w3.org/2001/XMLSchema#anyURI";
                    }
                    break;
            }
            return strResult;
        }

        private static void AddValue(List<string> target, string value, CEModel.CEAttributeType attrType)
        {
            if (attrType != CEModel.CEAttributeType.NXL_List)
            {
                target.Add(value);
            }
            else
            {
                string[] values = value.Split(new string[1] { CEModel.CEAttribute.SperaterForListValue }, StringSplitOptions.RemoveEmptyEntries);
                target.AddRange(values);
            }
        }
        public static RestModel.Single.RestRequest TransformToSingleRequest(CEModel.CERequest ceRequest)
        {
            RestModel.Single.RestRequest restModel = new RestModel.Single.RestRequest();

            restModel.Request = new RestModel.Single.RequestNode();
            restModel.Request.ReturnPolicyIdList = true.ToString();
            restModel.Request.Category = new List<RestModel.Single.CategoryNode>();

            #region CEHost
            CEModel.CEHost ceHost = ceRequest.Get_Host();
            if(ceHost!=null)
            {
                RestModel.Single.CategoryNode categoryHost = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categoryHost);
                categoryHost.CategoryId = Constant.XACML.Host_Host;
                categoryHost.Attribute = new List<RestModel.Single.AttributeNode>();
                if (!string.IsNullOrEmpty(ceHost.IPAddress))
                {
                    RestModel.Single.AttributeNode attributeIPAddress = new RestModel.Single.AttributeNode();
                    categoryHost.Attribute.Add(attributeIPAddress);
                    attributeIPAddress.AttributeId = Constant.XACML.Host_Inet_Address;
                    attributeIPAddress.Value.Add(ceHost.IPAddress);
                    attributeIPAddress.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_IpAddress);
                    attributeIPAddress.IncludeInResult = false;
                }
                if (!string.IsNullOrEmpty(ceHost.Name))
                {
                    RestModel.Single.AttributeNode attributeName = new RestModel.Single.AttributeNode();
                    categoryHost.Attribute.Add(attributeName);
                    attributeName.AttributeId = Constant.XACML.Host_Name;
                    attributeName.Value.Add(ceHost.Name);
                    attributeName.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeName.IncludeInResult = false;
                }
                if(ceHost.Attres!=null)
                {
                    for (int i = 0; i < ceHost.Attres.get_count(); i++)
                    {
                        string strName, strValue;
                        CEModel.CEAttributeType ceAttributeType;
                        ceHost.Attres.Get_Attre(i, out strName, out strValue, out ceAttributeType);
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                        {
                            RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                            categoryHost.Attribute.Add(attributeOther);
                            attributeOther.AttributeId = Constant.XACML.Host_Prefix + strName;
                            attributeOther.DataType = RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                            attributeOther.IncludeInResult = false;
                            attributeOther.Value.Add(strValue);
                        }
                    }
                }
            }

            #endregion

            #region CEUSER
            CEModel.CEUser ceUser = ceRequest.Get_User();
            if (ceUser != null)
            {
                RestModel.Single.CategoryNode categorySubject = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categorySubject);
                categorySubject.CategoryId = Constant.XACML.Subject_Access_Subject;
                categorySubject.Attribute = new List<RestModel.Single.AttributeNode>();

                if (!string.IsNullOrEmpty(ceUser.Sid))
                {
                    RestModel.Single.AttributeNode attributeSid = new RestModel.Single.AttributeNode();
                    categorySubject.Attribute.Add(attributeSid);
                    attributeSid.AttributeId = Constant.XACML.Subject_Subejct_Id;
                    attributeSid.Value.Add(ceUser.Sid);
                    attributeSid.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeSid.IncludeInResult = false;
                }
                if (!string.IsNullOrEmpty(ceUser.Name))
                {
                    RestModel.Single.AttributeNode attributeName = new RestModel.Single.AttributeNode();
                    categorySubject.Attribute.Add(attributeName);
                    attributeName.AttributeId =Constant.XACML.Subject_Subject_Name;
                    attributeName.Value.Add( ceUser.Name);
                    attributeName.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeName.IncludeInResult = false;
                }
                if (ceUser.Attres != null)
                {
                    for (int i = 0; i < ceUser.Attres.get_count(); i++)
                    {
                        string strName, strValue;
                        CEModel.CEAttributeType ceAttributeType;
                        ceUser.Attres.Get_Attre(i, out strName, out strValue,out ceAttributeType);
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                        {
                            RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                            categorySubject.Attribute.Add(attributeOther);
                            attributeOther.AttributeId =Constant.XACML.Subject_Prefix + strName;
                            attributeOther.DataType =RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                            attributeOther.IncludeInResult = false;

                            AddValue(attributeOther.Value, strValue, ceAttributeType);
                            
                        }
                    }
                }
            }
            #endregion

            #region CERECIPIENTS
            CEModel.CEUser ceRecpient = ceRequest.Get_Recipient();
            List<string> lisRecipients=ceRequest.Get_Recipients();
            if (ceRecpient != null)
            {
                RestModel.Single.CategoryNode categorySubject = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categorySubject);
                categorySubject.CategoryId =Constant.XACML.Recipient_Recipient_Subject;
                categorySubject.Attribute = new List<RestModel.Single.AttributeNode>();

                if (!string.IsNullOrEmpty(ceRecpient.Sid))
                {
                    RestModel.Single.AttributeNode attributeSid = new RestModel.Single.AttributeNode();
                    categorySubject.Attribute.Add(attributeSid);
                    attributeSid.AttributeId =Constant.XACML.Recipient_Id;
                    attributeSid.Value.Add( ceRecpient.Sid);
                    attributeSid.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeSid.IncludeInResult = false;
                }
                if (!string.IsNullOrEmpty(ceRecpient.Name))
                {
                    RestModel.Single.AttributeNode attributeName = new RestModel.Single.AttributeNode();
                    categorySubject.Attribute.Add(attributeName);
                    attributeName.AttributeId =Constant.XACML.Recipient_Name;
                    attributeName.Value.Add( ceRecpient.Name);
                    attributeName.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeName.IncludeInResult = false;
                }
                if (ceRecpient.Attres != null)
                {
                    for (int i = 0; i < ceRecpient.Attres.get_count(); i++)
                    {
                        string strName, strValue;
                        CEModel.CEAttributeType ceAttributeType;
                        ceRecpient.Attres.Get_Attre(i, out strName, out strValue,out ceAttributeType);
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                        {
                            RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                            categorySubject.Attribute.Add(attributeOther);
                            attributeOther.AttributeId =Constant.XACML.Recipient_Prefix + strName;
                            attributeOther.DataType =RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                            attributeOther.IncludeInResult = false;
                            attributeOther.Value.Add( strValue);
                        }
                    }
                }

            }
            else if(lisRecipients!=null)
            {
                RestModel.Single.CategoryNode categorySubject = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categorySubject);
                categorySubject.CategoryId =Constant.XACML.Recipient_Recipient_Subject;
                categorySubject.Attribute = new List<RestModel.Single.AttributeNode>();

                RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                categorySubject.Attribute.Add(attributeOther);
                attributeOther.AttributeId =Constant.XACML.Recipient_Email;
                attributeOther.DataType = RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                attributeOther.IncludeInResult = false;
                foreach(string strRecipient in lisRecipients)
                {
                attributeOther.Value.Add(strRecipient);
                }
            }
            #endregion

            #region CESOURCE
            CEModel.CEResource ceSource = ceRequest.Get_Source();
            if (ceSource != null)
            {
                RestModel.Single.CategoryNode categorySource = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categorySource);
                categorySource.Attribute = new List<RestModel.Single.AttributeNode>();
                categorySource.CategoryId =Constant.XACML.Resource;
                if (!string.IsNullOrEmpty(ceSource.SourceName))
                {
                    RestModel.Single.AttributeNode attributeSoueceId = new RestModel.Single.AttributeNode();
                    categorySource.Attribute.Add(attributeSoueceId);
                    attributeSoueceId.AttributeId =Constant.XACML.Resource_Resource_Id;
                    attributeSoueceId.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_AnyURI);
                    attributeSoueceId.IncludeInResult = false;
                    attributeSoueceId.Value.Add( ceSource.SourceName);
                }
                if (!string.IsNullOrEmpty(ceSource.SourceType))
                {
                    RestModel.Single.AttributeNode attributeSourceType = new RestModel.Single.AttributeNode();
                    categorySource.Attribute.Add(attributeSourceType);
                    attributeSourceType.AttributeId =Constant.XACML.Resource_Resource_Type;
                    attributeSourceType.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_AnyURI);
                    attributeSourceType.IncludeInResult = false;
                    attributeSourceType.Value.Add( ceSource.SourceType);
                }
                {
                    RestModel.Single.AttributeNode attributeSourceDimension = new RestModel.Single.AttributeNode();
                    categorySource.Attribute.Add(attributeSourceDimension);
                    attributeSourceDimension.AttributeId =Constant.XACML.Resource_Resource_Dimension;
                    attributeSourceDimension.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_AnyURI);
                    attributeSourceDimension.IncludeInResult = false;
                    attributeSourceDimension.Value.Add(Constant.XACML.Resource_Dimension_From);
                }
                if (ceSource.Attres != null)
                {
                    for (int i = 0; i < ceSource.Attres.get_count(); i++)
                    {
                        string strName, strValue;
                        CEModel.CEAttributeType ceAttributeType;
                        ceSource.Attres.Get_Attre(i, out strName, out strValue,out ceAttributeType);
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                        {
                            RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                            categorySource.Attribute.Add(attributeOther);
                            attributeOther.AttributeId = Constant.XACML.Resource_Prefix + strName;
                            attributeOther.DataType = RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                            attributeOther.IncludeInResult = false;
                            attributeOther.Value.Add( strValue);
                        }
                    }
                }

            }
            #endregion

            #region CEDest
            CEModel.CEResource ceDest = ceRequest.Get_Dest();
            if (ceDest != null)
            {
                RestModel.Single.CategoryNode categorySource = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categorySource);
                categorySource.Attribute = new List<RestModel.Single.AttributeNode>();
                categorySource.CategoryId = Constant.XACML.Resource;
                if (!string.IsNullOrEmpty(ceDest.SourceName))
                {
                    RestModel.Single.AttributeNode attributeSoueceId = new RestModel.Single.AttributeNode();
                    categorySource.Attribute.Add(attributeSoueceId);
                    attributeSoueceId.AttributeId =Constant.XACML.Resource_Resource_Id;
                    attributeSoueceId.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_AnyURI);
                    attributeSoueceId.IncludeInResult = false;
                    attributeSoueceId.Value.Add( ceDest.SourceName);
                }
                if (!string.IsNullOrEmpty(ceDest.SourceType))
                {
                    RestModel.Single.AttributeNode attributeSourceType = new RestModel.Single.AttributeNode();
                    categorySource.Attribute.Add(attributeSourceType);
                    attributeSourceType.AttributeId =Constant.XACML.Resource_Resource_Type;
                    attributeSourceType.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_AnyURI);
                    attributeSourceType.IncludeInResult = false;
                    attributeSourceType.Value.Add( ceDest.SourceType);
                }
                {
                    RestModel.Single.AttributeNode attributeSourceDimension = new RestModel.Single.AttributeNode();
                    categorySource.Attribute.Add(attributeSourceDimension);
                    attributeSourceDimension.AttributeId =Constant.XACML.Resource_Resource_Dimension;
                    attributeSourceDimension.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_AnyURI);
                    attributeSourceDimension.IncludeInResult = false;
                    attributeSourceDimension.Value.Add(Constant.XACML.Resource_Dimension_To);
                }
                if (ceDest.Attres != null)
                {
                    for (int i = 0; i < ceDest.Attres.get_count(); i++)
                    {
                        string strName, strValue;
                        CEModel.CEAttributeType ceAttributeType;
                        ceDest.Attres.Get_Attre(i, out strName, out strValue,out ceAttributeType);
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                        {
                            RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                            categorySource.Attribute.Add(attributeOther);
                            attributeOther.AttributeId = Constant.XACML.Resource_Prefix + strName;
                            attributeOther.DataType = RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                            attributeOther.IncludeInResult = false;
                            attributeOther.Value.Add( strValue);
                        }
                    }
                }

            }
            #endregion

            #region ACTION
            string strAction = ceRequest.Get_Action();
            if (strAction != null)
            {
                RestModel.Single.CategoryNode categoryAction = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categoryAction);
                categoryAction.Attribute = new List<RestModel.Single.AttributeNode>();
                categoryAction.CategoryId =Constant.XACML.Action;
                RestModel.Single.AttributeNode attributeAction = new RestModel.Single.AttributeNode();
                categoryAction.Attribute.Add(attributeAction);
                attributeAction.AttributeId =Constant.XACML.Action_Action_Id;
                attributeAction.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                attributeAction.IncludeInResult = false;
                attributeAction.Value.Add( strAction);
            }
            #endregion

            #region CEAPP
            CEModel.CEApp ceApp = ceRequest.Get_App();
            if (ceApp != null)
            {
                RestModel.Single.CategoryNode categoryApplication = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categoryApplication);
                categoryApplication.Attribute = new List<RestModel.Single.AttributeNode>();
                categoryApplication.CategoryId =Constant.XACML.Application;

                if (!string.IsNullOrEmpty(ceApp.Name))
                {
                    RestModel.Single.AttributeNode attributeAppId = new RestModel.Single.AttributeNode();
                    categoryApplication.Attribute.Add(attributeAppId);
                    attributeAppId.AttributeId =Constant.XACML.Application_Application_Id;
                    attributeAppId.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeAppId.IncludeInResult = false;
                    attributeAppId.Value.Add( ceApp.Name);
                }

                if (!string.IsNullOrEmpty(ceApp.Path))
                {
                    RestModel.Single.AttributeNode attributeAppName = new RestModel.Single.AttributeNode();
                    categoryApplication.Attribute.Add(attributeAppName);
                    attributeAppName.AttributeId =Constant.XACML.Application_Application_Name;
                    attributeAppName.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeAppName.IncludeInResult = false;
                    attributeAppName.Value.Add( ceApp.Path);
                }
                if (!string.IsNullOrEmpty(ceApp.Url))
                {
                    RestModel.Single.AttributeNode attributeAppUrl = new RestModel.Single.AttributeNode();
                    categoryApplication.Attribute.Add(attributeAppUrl);
                    attributeAppUrl.AttributeId =Constant.XACML.Application_Application_Url;
                    attributeAppUrl.DataType = ModelTransform.Function.TransformCEAttributeTypeToString(CEModel.CEAttributeType.XACML_String);
                    attributeAppUrl.IncludeInResult = false;
                    attributeAppUrl.Value.Add( ceApp.Url);
                }
                if (ceApp.Attres != null)
                {
                    for (int i = 0; i < ceApp.Attres.get_count(); i++)
                    {
                        string strName, strValue;
                        CEModel.CEAttributeType ceAttributeType;
                        ceRequest.Get_App().Attres.Get_Attre(i, out strName, out strValue,out ceAttributeType);
                        if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                        {
                            RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                            categoryApplication.Attribute.Add(attributeOther);
                            attributeOther.AttributeId = Constant.XACML.Application_Application_Prefix + strName;
                            attributeOther.DataType = RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                            attributeOther.IncludeInResult = false;
                            attributeOther.Value.Add( strValue);
                        }
                    }
                }
            }
            #endregion

            #region NAMEATTRIBUTES
            CEModel.CEAttres ceNameAttribute = ceRequest.Get_NameAttributes();
            if (ceNameAttribute != null)
            {
                RestModel.Single.CategoryNode categoryNameAttributes = new RestModel.Single.CategoryNode();
                restModel.Request.Category.Add(categoryNameAttributes);
                categoryNameAttributes.Attribute = new List<RestModel.Single.AttributeNode>();
                categoryNameAttributes.CategoryId =Constant.XACML.Environment;
                for (int i = 0; i < ceNameAttribute.get_count(); i++)
                {
                    string strName, strValue;
                    CEModel.CEAttributeType ceAttributeType;
                    ceNameAttribute.Get_Attre(i, out strName, out strValue,out ceAttributeType);
                    if (!string.IsNullOrEmpty(strName) && !string.IsNullOrEmpty(strValue))
                    {
                        RestModel.Single.AttributeNode attributeOther = new RestModel.Single.AttributeNode();
                        categoryNameAttributes.Attribute.Add(attributeOther);
                        attributeOther.AttributeId =Constant.XACML.Enviroment_Prefix + strName;
                        attributeOther.DataType = RestAPISDK.ModelTransform.Function.TransformCEAttributeTypeToString(ceAttributeType);
                        attributeOther.IncludeInResult = false;
                        attributeOther.Value.Add( strValue);
                    }
                }
            }
            #endregion

            return restModel;
        }

        public static List<CEModel.CEObligation> TransformToCEObligation(RestModel.Single.ResultNode restResult)
        {
            List<CEModel.CEObligation> lisResult = new List<CEModel.CEObligation>();
            if (restResult.Obligations != null && restResult.Obligations.Count > 0)
            {
                foreach (RestModel.Single.ObligationsNode restObligation in restResult.Obligations)
                {
                    string strObligationName = restObligation.Id;
                    CEModel.CEAttres ceAttres = new CEModel.CEAttres();
                    if (restObligation.AttributeAssignment != null && restObligation.AttributeAssignment.Count > 0)
                    {

                        foreach (RestModel.Single.AttributeAssignmentNode attributeNode in restObligation.AttributeAssignment)
                        {
                            string strAttrName = attributeNode.AttributeId;
                            string strAttributeValue = string.Join(";", attributeNode.Value.ToArray<string>());
                            ceAttres.Add_Attre(strAttrName, strAttributeValue, CEModel.CEAttributeType.XACML_String);
                        }

                    }
                    CEModel.CEObligation ceOblitaion = new CEModel.CEObligation(strObligationName, ceAttres, "");
                    lisResult.Add(ceOblitaion);
                }
            }


            return lisResult;
        }

        public static PCResult TransformFromStatusToPcResult(string strStatusMessage)
        {
            if(strStatusMessage.Equals(Constant.RestResponseStatus.Status_Ok,StringComparison.OrdinalIgnoreCase))
            {
                return PCResult.OK;
            }
            else if(strStatusMessage.Equals(Constant.RestResponseStatus.Status_MissAttributes))
            {
                return PCResult.MissAttributes;
            }
            else
            {
                return PCResult.ResponseStatusAbnormal;
            }

        }

        public static PCResult TransformFromStatusCodeToPcResult(System.Net.HttpStatusCode code)
        {
            PCResult pcResult = PCResult.HttpFaild_Unknow;
            switch(code)
            {
                case System.Net.HttpStatusCode.BadRequest:
                    pcResult = PCResult.HttpFaild_BadRequest;
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    pcResult = PCResult.HttpFaild_Unauthorized;
                    break;
                case System.Net.HttpStatusCode.NotFound:
                    pcResult = PCResult.HttpFaild_NotFound;
                    break;
                case System.Net.HttpStatusCode.MethodNotAllowed:
                    pcResult = PCResult.HttpFaild_MethodNotAllowed;
                    break;
                case System.Net.HttpStatusCode.ProxyAuthenticationRequired:
                    pcResult = PCResult.HttpFaild_ProxyAuthenticationRequired;
                    break;
                case System.Net.HttpStatusCode.RequestTimeout:
                    pcResult = PCResult.HttpFaild_RequestTimeout;
                    break;
                case System.Net.HttpStatusCode.RequestUriTooLong:
                    pcResult = PCResult.HttpFaild_RequestUriTooLong;
                    break;
                case System.Net.HttpStatusCode.UnsupportedMediaType:
                    pcResult = PCResult.HttpFaild_UnsupportedMediaType;
                    break;
                case System.Net.HttpStatusCode.NotImplemented:
                    pcResult = PCResult.HttpFaild_NotImplemented;
                    break;
                case System.Net.HttpStatusCode.BadGateway:
                    pcResult = PCResult.HttpFaild_BadGateway;
                    break;
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    pcResult = PCResult.HttpFaild_ServiceUnavailable;
                    break;
                case System.Net.HttpStatusCode.HttpVersionNotSupported:
                    pcResult = PCResult.HttpFaild_HttpVersionNotSupported;
                    break;
                case System.Net.HttpStatusCode.GatewayTimeout:
                    pcResult = PCResult.HttpFaild_GatewayTimeout;
                    break;

            }
            return pcResult;
        }

        public static PCResult TransformFormWebExceptionToPcResult(string strExceptionMessage)
        {
            PCResult result = PCResult.OK;
            switch(strExceptionMessage)
            {
                case "The remote server returned an error: (400) Bad Request.": result = PCResult.HttpFaild_BadRequest;
                    break;
                case "The remote server returned an error: (401) Unauthorized.": result = PCResult.HttpFaild_Unauthorized;
                    break;
                default: result = PCResult.GetResponseFaild;
                    break;
            }

            return result;
        }

        public static PCResult TransformFromInvaillTokenToPcResult(string strJson)
        {
            PCResult result = PCResult.HttpFaild_Unknow;
            switch (strJson)
            {
                case "{\"error\":\"invalid_client\"}":
                case "{\"error\":\"unauthorized_client\"}":
                    result = PCResult.InvalidClient;
                    break;
            }

            return result;
        }

        public static RestModel.Single.RestResponse TransformFormJSONToRestResponse(string strJson)
        {
            return JsonSerializer.LoadFromJson<RestModel.Single.RestResponse>(strJson);
        }
        public static RestModel.Single.NewRestResponse TransformFormJSONToNewRestResponse(string strJson)
        {
            return JsonSerializer.LoadFromJson<RestModel.Single.NewRestResponse>(strJson);
        }
        public static RestModel.Authorized.TokenResponse TransformFormJSONToTokenResponse(string strJson)
        {
            return JsonSerializer.LoadFromJson<RestModel.Authorized.TokenResponse>(strJson);
        }

        

    }
    class UrlEncoder
    {
        public static string Encode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return Encode(str, Encoding.UTF8);
        }

        public static string Encode(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e));
        }

        internal static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return Encode(bytes, 0, bytes.Length, false);
        }

        internal static byte[] Encode(byte[] bytes, int offset, int count, bool alwaysCreateNewReturnValue)
        {
            byte[] buffer = Encode(bytes, offset, count);
            if ((alwaysCreateNewReturnValue && (buffer != null)) && (buffer == bytes))
            {
                return (byte[])buffer.Clone();
            }
            return buffer;
        }

        internal static byte[] Encode(byte[] bytes, int offset, int count)
        {
            if (!ValidateUrlEncodingParameters(bytes, offset, count))
            {
                return null;
            }
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (ch == ' ')
                {
                    num++;
                }
                else if (!IsUrlSafeChar(ch))
                {
                    num2++;
                }
            }
            if ((num == 0) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsUrlSafeChar(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        internal static bool ValidateUrlEncodingParameters(byte[] bytes, int offset, int count)
        {
            if ((bytes == null) && (count == 0))
            {
                return false;
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if ((offset < 0) || (offset > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || ((offset + count) > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            return true;
        }

        public static bool IsUrlSafeChar(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        public static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

    }
}

namespace NextLabs.JavaPC.RestAPISDK.Constant
{
    static class General
    {
        public const string JavaPCPDPSuffix = "dpc/authorization/pdp";
        public const string JavaPCTGTSuffix = "cas/v1/tickets";
        public const string OnPremAuthSuffix = "cas/token";
        public const string CloudAZAuthSuffix = "oauth/token";
        public const int AutoUpdateCookieTimeUnit = 60 * 1000;
        public const int AutoUpdateTokenTimeUnit = 60;
        public const string Equal = "=";
    }
    static class HttpRquest
    {
        //In theory MaxConnect is 1. 2 can insure more robust.
        public const int MaxConnect = 2;    //32;
        public  const string Method_Post = "POST";
        public const string Method_Get = "GET";
        public const string Method_Delete = "DELETE";
        public const string Method_Head = "HEAD";
        public const string Method_Options = "OPTIONS";
        public const string Method_Put = "PUT";
        public const string Method_Trace = "TRACE";
        public  const string Service_Eval = "EVAL";
        public const string Service = "Service";
        public const string Version = "Version";
        public const string Authorization = "Authorization";
        public  const string Version_1_0 = "1.0";
        public  const int TimeOut = 60000;

        public  const string ContentType_Unknow = "application/Unknow";
        public  const string ContentType_JSON = "application/json";
        public  const string ContentType_XML = "application/xml";
        public const string ContentType_X_WWW_From_Urlencoded = "application/x-www-form-urlencoded";

        public const string Endcoding_UTF_8 = "UTF-8";

        public const string HttpSeparator = "/";

        public const string X_WWW_From_Urlencoded_Data_Format = "username={0}&password={1}";
        public const string X_WWW_From_Urlencoded_Service = "service={0}";
        public const string UrlParameter_Getting_Authentication_Cookie = "{0}?ticket={1}";
        public const string Header_Key_Location = "Location";
    }
    static class RestResponseStatus
    {
        public  const string Status_Ok = "urn:oasis:names:tc:xacml:1.0:status:ok";
        public const string Status_MissAttributes = "urn:oasis:names:tc:xacml:1.0:status:missing-attribute";
    }
    static class RestResponse
    {
        public  const string Deny = "Deny";
        public  const string Allow = "Permit";
    }
    static class XACML
    {
        public  const string Subject_Access_Subject = "urn:oasis:names:tc:xacml:1.0:subject-category:access-subject";
        public  const string Subject_Subejct_Id = "urn:oasis:names:tc:xacml:1.0:subject:subject-id";
        public  const string Subject_Subject_Name = "urn:oasis:names:tc:xacml:1.0:subject:name";
        public  const string Subject_Prefix = "urn:oasis:names:tc:xacml:1.0:subject:";

        //public const string MycustomAttr_environment_MyCustomAttr = "attribute-category:environment-MyCustomAttr";
        //public const string 

        public const string Host_Host = "urn:nextlabs:names:evalsvc:1.0:attribute-category:host";
        public const string Host_Inet_Address = "urn:nextlabs:names:evalsvc:1.0:host:inet_address";
        public const string Host_Name = "urn:nextlabs:names:evalsvc:1.0:host:name";
        public const string Host_Prefix = "urn:nextlabs:names:evalsvc:1.0:host:";

        public  const string Recipient_Recipient_Subject= "urn:oasis:names:tc:xacml:1.0:subject-category:recipient-subject";
        public  const string Recipient_Id = "urn:nextlabs:names:evalsvc:1.0:recipient:id";
        public  const string Recipient_Name = "urn:nextlabs:names:evalsvc:1.0:recipient::name";
        public  const string Recipient_Prefix = "urn:nextlabs:names:evalsvc:1.0:recipient:";
        public  const string Recipient_Email = "urn:nextlabs:names:evalsvc:1.0:recipient:email";

        public  const string Resource = "urn:oasis:names:tc:xacml:3.0:attribute-category:resource";
        public  const string Resource_Resource_Id = "urn:oasis:names:tc:xacml:1.0:resource:resource-id";
        public  const string Resource_Resource_Type = "urn:nextlabs:names:evalsvc:1.0:resource:resource-type";
        public  const string Resource_Resource_Dimension = "urn:nextlabs:names:evalsvc:1.0:resource:resource-dimension";
        public  const string Resource_Dimension_From = "from";
        public  const string Resource_Dimension_To = "to";
        public  const string Resource_Prefix = "urn:nextlabs:names:evalsvc:1.0:resource:";

        public  const string Action = "urn:oasis:names:tc:xacml:3.0:attribute-category:action";
        public  const string Action_Action_Id = "urn:oasis:names:tc:xacml:1.0:action:action-id";

        public  const string Application = "urn:nextlabs:names:evalsvc:1.0:attribute-category:application";
        public  const string Application_Application_Id = "urn:nextlabs:names:evalsvc:1.0:application:application-id";
        public  const string Application_Application_Name = "urn:nextlabs:names:evalsvc:1.0:application:name";
        public  const string Application_Application_Url = "urn:nextlabs:names:evalsvc:1.0:application:url";
        public  const string Application_Application_Prefix = "urn:nextlabs:names:evalsvc:1.0:application:application:";

        public  const string Environment = "urn:oasis:names:tc:xacml:3.0:attribute-category:environment";
        public  const string Enviroment_Prefix = "urn:oasis:names:tc:xacml:1.0:environment:";

    }
    
}
