using System;
using System.Collections.Generic;
using System.Web;
using System.Text;
using NextLabs.PreAuthAttributes;
using Microsoft.SharePoint;
using System.Diagnostics;
using NextLabs.Common;
using DCRMPluginForSharePoint;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;
using System.Security.Cryptography;
using System.IO;
using Microsoft.Win32;
using System.Xml.Serialization;

namespace TestPlugIn
{
    public class CRMDecider : IGetPreAuthAttributes
    {
        private CRMProxy m_crmProxy = null;
        private string adeskey = "127c0798";
        public static string GetFedAuthFromRequest(HttpRequest Request)
        {
            string strToken = null;
            HttpCookieCollection colCookie = Request.Cookies;

            for (int i = 0; i < colCookie.Count; i++)
            {
                if (colCookie.Get(i).Name.Equals("FedAuth", StringComparison.OrdinalIgnoreCase))
                {
                    strToken = colCookie.Get(i).Value;
                    break;
                }
            }
            return strToken;
        }

        public void DepolyConfiguration(
            Uri orgUri,
            Uri homerealmUri, 
            AuthenticationProviderType providerType,
            string domain, 
            string username,
            string password)
        {
            DCRMPluginForSharePoint.Configuration config = new DCRMPluginForSharePoint.Configuration();
            config.OrganizationUri = orgUri.ToString();
            config.HomeRealmUri = homerealmUri == null?"":homerealmUri.ToString();
            config.Credential = new Credential();
            config.Credential.Domain = domain;
            config.Credential.UserName = username;
            config.Credential.Password = AESEncrypt(password, adeskey);
            config.AuthProviderType = providerType;
            WriteConfiguration(config);
        }
        private string AESEncrypt(string encryptString, string encryptKey)
        {
            if (string.IsNullOrEmpty(encryptString))
            {
                throw (new Exception("Encrypt string cannot be null"));
            }
            if (string.IsNullOrEmpty(encryptKey))
            {
                throw (new Exception("Encrypt key cannot be null"));
            }

            string strEncrypt = "";
            byte[] btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            Rijndael AESProvider = Rijndael.Create();
            try
            {
                byte[] m_btEncryptString = Encoding.Default.GetBytes(encryptString);
                MemoryStream stream = new MemoryStream();
                CryptoStream csstream = 
                    new CryptoStream(stream,
                    AESProvider.CreateEncryptor(Encoding.Default.GetBytes(encryptKey), btIV), 
                    CryptoStreamMode.Write);

                csstream.Write(m_btEncryptString, 0, m_btEncryptString.Length); csstream.FlushFinalBlock();
                strEncrypt = Convert.ToBase64String(stream.ToArray());
                stream.Close(); stream.Dispose();
                csstream.Close(); csstream.Dispose();
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                AESProvider.Clear();
            }
            return strEncrypt;
        }
        private static string AESDecrypt(string decryptString, string decryptKey)
        {
            if (string.IsNullOrEmpty(decryptString))
            {
                throw (new Exception("Decrypt string cannot be null"));
            }

            if (string.IsNullOrEmpty(decryptKey))
            {
                throw (new Exception("decrypt key cannot be null"));
            }

            string strDecrypt = "";
            byte[] btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
            Rijndael AESProvider = Rijndael.Create();
            try
            {
                byte[] m_btDecryptString = Convert.FromBase64String(decryptString);
                MemoryStream stream = new MemoryStream();
                CryptoStream csstream = 
                    new CryptoStream(stream, AESProvider.CreateDecryptor(Encoding.Default.GetBytes(decryptKey), btIV), CryptoStreamMode.Write);
                csstream.Write(m_btDecryptString, 0, m_btDecryptString.Length); csstream.FlushFinalBlock();
                strDecrypt = Encoding.Default.GetString(stream.ToArray());
                stream.Close(); stream.Dispose();
                csstream.Close(); csstream.Dispose();
            }
            catch (IOException ex)
            { throw ex; }
            catch (CryptographicException ex) { throw ex; }
            catch (ArgumentException ex) { throw ex; }
            catch (Exception ex) { throw ex; }
            finally { AESProvider.Clear(); }
            return strDecrypt;
        }

        private object Deserialize(Type type, string xml)
        {
            if (type == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(xml))
            {
                return null;
            }

            StringReader reader = null;
            try
            {
                reader = new StringReader(xml);

                XmlSerializer xmlDeserializer = new XmlSerializer(type);
                return xmlDeserializer.Deserialize(reader);
            }
            catch (Exception exp)
            {
                string msg = string.Format("Failed to deserialize {0} from XML {1} for {2}",
                    type.ToString(), xml, exp.Message);
                throw new Exception(msg, exp);
            }
            finally
            {
                reader.Close();
            }
        }

        private string Serialize(Type type, object obj)
        {
            if (type == null)
            {
                return null;
            }

            if (obj == null)
            {
                return null;
            }

            MemoryStream stream = new MemoryStream();
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            string xml = null;
            StreamReader streamReader = null;
            try
            {
                xmlSerializer.Serialize(stream, obj);

                stream.Position = 0;
                streamReader = new StreamReader(stream);
                xml = streamReader.ReadToEnd();
            }
            catch (Exception exp)
            {
                string msg = string.Format("Failed to Serialize {0} from XML {1} for {2}",
                    type.ToString(), xml, exp.Message);
                throw new Exception(msg, exp);
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }

                if (stream != null)
                {
                    stream.Close();
                }
            }
            return xml;
        }
        public DCRMPluginForSharePoint.Configuration ReadConfiguration()
        {
            RegistryKey localkey = null;
            RegistryKey preAuthdll = null;
            try
            {
                localkey = Registry.LocalMachine;

                string configFolder =
                    @"SOFTWARE\NextLabs\Compliant Enterprise\SharePoint Enforcer\PreAuthDll";
                preAuthdll = localkey.OpenSubKey(configFolder);
                if(preAuthdll == null)
                {
                    Trace.WriteLine("Cannot find the key of settings for pre auth dll");
                }
                string configXml = (string)preAuthdll.GetValue("config");

                DCRMPluginForSharePoint.Configuration config = 
                    (DCRMPluginForSharePoint.Configuration)Deserialize(typeof(DCRMPluginForSharePoint.Configuration), configXml);
                return config;
            }
            finally
            {
                if(preAuthdll != null)
                    preAuthdll.Close();

                if(localkey != null)
                    localkey.Close();
            }
        }

        private void WriteConfiguration(DCRMPluginForSharePoint.Configuration config)
        {
            RegistryKey localkey = null;
            RegistryKey preAuthKey = null;
            try
            {
                localkey = Registry.LocalMachine;

                string configFolder =
                    @"SOFTWARE\NextLabs\Compliant Enterprise\SharePoint Enforcer\PreAuthDLL";

                preAuthKey = localkey.CreateSubKey(configFolder);
                string xml = Serialize(typeof(DCRMPluginForSharePoint.Configuration), config);
                preAuthKey.SetValue("config", xml);
                preAuthKey.Flush();
                localkey.Flush();
            }
            finally
            {
                
                if (preAuthKey != null)
                    preAuthKey.Close();

                if (localkey != null)
                    localkey.Close();
            }
        }
        private ClientCredentials ParseInCredentials(
            Credential cred, 
            AuthenticationProviderType endpointType)
        {
            
            ClientCredentials result = new ClientCredentials();

            switch (endpointType)
            {
                case AuthenticationProviderType.ActiveDirectory:
                    {
                        result.Windows.ClientCredential = new System.Net.NetworkCredential()
                        {
                            UserName = cred.UserName,
                            Domain = cred.Domain,
                            Password = AESDecrypt(cred.Password, adeskey)
                        };
                    }
                    break;
                case AuthenticationProviderType.LiveId:
                case AuthenticationProviderType.Federation:
                case AuthenticationProviderType.OnlineFederation:
                    {
                        result.UserName.UserName = cred.UserName;
                        result.UserName.Password = AESDecrypt(cred.Password, adeskey);
                    }
                    break;
                default:
                    break;
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDomainName(SPUser spUser)
        {

            System.Security.Principal.IPrincipal ipl = null;

            if (System.Web.HttpContext.Current == null)
            {
                string clientIpAddr = WebRemoteAddressMap.GetRemoteAddress
                            (spUser.LoginName, spUser.ParentWeb.Url, ref ipl);
            }
            else
            {
                ipl = System.Web.HttpContext.Current.User;
            }

            if (ipl.Identity == null)
            {
                Trace.WriteLine("The identity of current user is null");
                return null;
            }

            if( string.IsNullOrEmpty(ipl.Identity.AuthenticationType) || 
                !ipl.Identity.AuthenticationType.Equals("Federation",StringComparison.OrdinalIgnoreCase))
            {
                Trace.WriteLine("The authentiation type in the identity of current user is null or mismath 'Federation'");
                return null;
            }

            if(string.IsNullOrEmpty(ipl.Identity.Name))
            {
                Trace.WriteLine("The identity name of current user is null or empty");
                return null;
            }

            
            //format reference
            //https://social.technet.microsoft.com/wiki/contents/articles/13921.sharepoint-20102013-claims-encoding.aspx
            //windows claim -> 0#.w|pmpf1\administrator 
            //SAML Provider -> 05.t | saml provider | administrator@pmpf1.qalab01.nextlabs.com
            //when using SAML provider, the email can get from SPUser, so will not process the SAML provider here
            string[] claimInfoItems = ipl.Identity.Name.Split('|');
            if(claimInfoItems.Length < 2)
            {
                Trace.WriteLine(string.Format("The name in identity of current user is incorrect: {0}", ipl.Identity.Name));
                return null;
            }
            if(!claimInfoItems[0].Equals("0#.w")) //windows claim
            {
                Trace.WriteLine(string.Format("The name in identity of current user is not for windows claim: {0}", ipl.Identity.Name));
                return null;
            }

            return claimInfoItems[1];
        }

        private CRMProxy InitCrmProxy()
        {
            DCRMPluginForSharePoint.Configuration config = ReadConfiguration();
            ClientCredentials credentials = ParseInCredentials(config.Credential, config.AuthProviderType);

            CRMProxy crmProxy = new CRMProxy();
            crmProxy.Init(
                new Uri(config.OrganizationUri),
                null,
                credentials,
                null);
            return crmProxy;
        }
        private CRMProxy GetCrmProxy()
        {
            lock(this)
            {
                if(m_crmProxy == null)
                {
                    m_crmProxy = InitCrmProxy();
                }
                return m_crmProxy;
            }
        }

        private int CheckDecision(CRMProxy proxy, Guid userId, string site, string folder, string action)
        {
            int decision = 2;//don't care
            switch(action.ToLower())
            {
                case "open":
                    decision = proxy.OnViewAction(userId, folder, site);
                    break;
                case "delete":
                    decision = proxy.OnDeleteAction(userId, folder, site);
                    break;
                case "upload":
                    decision = proxy.OnUpdateAction(userId, folder, site);
                    break;
                case "edit":
                    decision = proxy.OnUpdateAction(userId, folder, site);
                    break;
                default:
                    break;
            }

            return decision;
        }
        public int GetCustomAttributes(
            object spUser, 
            object spSource, 
            object SPFile, 
            string strAction, 
            Dictionary<string, string> userPair, 
            Dictionary<string, string> srcPair, 
            Dictionary<string, string> dstPair, 
            int nobjecttype, 
            int nTimeout = 3000)
        {
            try
            {
                int iResult = 0;
                Trace.WriteLine("------------------enter GetCustomAttributes---------------------");
                if (spSource is SPList)
                {
                    Trace.WriteLine(string.Format("source is SPList"));
                    return 0;
                }
                else if (spSource is SPWeb)
                {
                    Trace.WriteLine(string.Format("source is SPWeb"));
                    return 0;
                }
                
                SPUser user = (SPUser)spUser;
                CRMProxy crmProxy = GetCrmProxy();


                Guid crmUserId = Guid.Empty;
                if (!string.IsNullOrEmpty(user.Email))
                {
                    //impersonate by user email
                    crmUserId = crmProxy.QueryUserIDByEmail(user.Email);
                }
                else
                {
                    //try to get the domain name from identity
                    string domainName = GetDomainName(user);
                    if (string.IsNullOrEmpty(domainName))
                    {
                        Trace.WriteLine(string.Format("Cannot get domain name from identity"));
                        srcPair.Add("nxlcrmdecision", "2"); //2 means don't care
                        return 0;
                    }

                    crmUserId = crmProxy.QueryUserIDByDomainName(domainName);
                }

                if (crmUserId.Equals(Guid.Empty))
                {
                    Trace.WriteLine(string.Format("Cannot get user id name from crm"));
                    srcPair.Add("nxlcrmdecision", "2"); //2 means don't care
                    return 0;
                }

                if (spSource is SPListItem)
                {
                    SPListItem item = spSource as SPListItem;
                    Trace.WriteLine(string.Format("item url is {0}", item.Url));
                    Trace.WriteLine(string.Format("item web is {0}", item.Web.Url));


                    int index = item.Url.LastIndexOf('/');
                    string folderpath = item.Url.Substring(0, index);
                    string site = item.Web.Url;

                    int decision = CheckDecision(crmProxy, crmUserId, site, folderpath, strAction);
                    srcPair.Add("nxlcrmdecision", decision.ToString());
                    Trace.WriteLine(string.Format("Decision from CRM is {0} for this {1}", decision.ToString(), strAction));
                }
                else if (spSource is string && spSource != null)
                {
                    string strSource = (string)spSource;
                    if (string.IsNullOrEmpty(strSource))
                    {
                        return 0;
                    }
                    if (strAction.Equals("edit", StringComparison.OrdinalIgnoreCase))
                    {
                        Trace.WriteLine("should be upload action");
                        int index = strSource.LastIndexOf('/');
                        string fullFolderpath = strSource.Substring(0, index);
                        string webUrl = user.ParentWeb.Url.EndsWith("/") ? user.ParentWeb.Url : user.ParentWeb.Url + "/";
                        string folderpath = fullFolderpath.Remove(0, webUrl.Length);

                        int decision = CheckDecision(crmProxy, crmUserId, webUrl, folderpath, strAction);
                        srcPair.Add("nxlcrmdecision", decision.ToString());
                        Trace.WriteLine(string.Format("Decision from CRM is {0} for this upload action", decision.ToString()));
                    }
                }

                Trace.WriteLine("------------------leave GetCustomAttributes normally---------------------");
                return iResult;
            }
            catch(Exception exp)
            {
                Trace.Write(exp.Message);
                return 0;
            }
        }
    }
}
