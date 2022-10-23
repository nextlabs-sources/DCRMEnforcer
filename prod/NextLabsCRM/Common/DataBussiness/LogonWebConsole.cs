using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Xml;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.LogonCC
{
    public class LogonWebConsole
    {
        /*
         *@function: Get web console auto login url. e.g. https://cchost:443/console/j_spring_cas_security_check?ticket=ST-134-dntcrzUNiRC63QOHteWr-cchost
         * @param strLoginUrl: e.g. https://cchost/cas/login?service=https%3A%2F%2Fcchost%3A443%2Fconsole%2Fj_spring_cas_security_check
         * @param strUserName: the username to login
         * @param strPwd:  the password of the user: username
         * @param strLt:  login parameter:lt
         * @param strExecution: login parameter:execution
         * @return loginurl.
         */
        static public string GetAutologinUrl(string strLoginUrl, string strUserName, string strPwd, string strLt, string strExecution, out HttpStatusCode statusCode)
        {
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(strLoginUrl);
            hwr.Method = "POST";
            hwr.KeepAlive = false;
            hwr.AllowAutoRedirect = false;

            hwr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            hwr.Headers["Accept-Encoding"] = "gzip,deflate,br";
            hwr.Headers["Accept-Language"] = "zh-CN,zh;q=0.9,en;q=0.8,en-US;q=0.7";
            hwr.Headers["Upgrade-Insecure-Requests"] = "1";
            hwr.ContentType = "application/x-www-form-urlencoded";
            hwr.Headers["Cookie"] = "CASPRIVACY=;TGC=";
            hwr.Headers["Cache-Control"] = "max-age=0";
            hwr.Referer = strLoginUrl;
            //  hwr.Host = "azure-dev-cc";
            hwr.UserAgent = "WebConsoleHelper";

            string strPostData = string.Format("username={0}&password={1}&lt={2}&execution={3}&_eventId=submit&submit=Login",
                strUserName, strPwd, strLt, strExecution);

            using (Stream stream = hwr.GetRequestStream())
            {
                byte[] byteData = System.Text.Encoding.Default.GetBytes(strPostData);
                stream.Write(byteData, 0, byteData.Length);
                stream.Close();
            }


            HttpWebResponse webResp = hwr.GetResponse() as HttpWebResponse;
            Stream stre = webResp.GetResponseStream();
            StreamReader sr = new StreamReader(stre);
            string html = sr.ReadToEnd();

            statusCode = webResp.StatusCode;

            string strRedirUrl = webResp.Headers["Location"];

            return strRedirUrl;
        }

        /*
        *@function: get cookie after login to web console
        * @param strLoginUrl: e.g. https://cchost/cas/login?service=https%3A%2F%2Fcchost%3A443%2Fconsole%2Fj_spring_cas_security_check
        * @param strUserName: the username to login
        * @param strPwd:  the password of the user: username
        * @param strLt:  login parameter:lt
        * @param strExecution: login parameter:execution
        * @out strSetCookie:  e.g. JSESSIONID=656B8A6CA24F981DAB59C1EB920EE388; Path=/console; Secure; HttpOnly
        */
        static public bool Login(string strLoginUrl, string strUserName, string strPwd, string strLt, string strExecution, out string strSetCookie)
        {
            strSetCookie = "";
            HttpStatusCode statusCode = 0;
            //request to get redirect url
            string strRedirUrl = GetAutologinUrl(strLoginUrl, strUserName, strPwd, strLt, strExecution, out statusCode);

            //open resut rul
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(strRedirUrl);
            hwr.AllowAutoRedirect = false;
            hwr.Method = "GET";
            hwr.KeepAlive = false;
            hwr.AllowAutoRedirect = false;
            HttpWebResponse loginResp = hwr.GetResponse() as HttpWebResponse;
            loginResp.GetResponseStream();

            strSetCookie = loginResp.Headers["Set-cookie"];

            return true;
        }

        /*
        *@function: get login parameters: authurl, lt and execution
        * @param strConsole: e.g. https://cchost/console
        * @out strAuthUrl:  e.g. https://cchost/cas/login?service=https%3A%2F%2Fcchost%3A443%2Fconsole%2Fj_spring_cas_security_check
        * @out strLt:  login parameter:lt
        * @out strExecution: login parameter:execution
        */
        static public bool GetAuthUrlAndParameter(string strConsole, out string strAuthUrl, out string strlt, out string strExecution)
        {
            strAuthUrl = "";
            strlt = "";
            strExecution = "";

            //request
            HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(strConsole);
            hwr.Method = "GET";
            hwr.KeepAlive = false;
            Stream stre = hwr.GetResponse().GetResponseStream();
            StreamReader sr = new StreamReader(stre);
            string html = sr.ReadToEnd();

            //parse result
            int nPosForm = html.IndexOf("<form");

            //get auth url
            {
                string strKey = "action=\"";
                int nPosAction = html.IndexOf(strKey, nPosForm);
                int nPosEnd = html.IndexOf("\"", nPosAction + strKey.Length);
                strAuthUrl = html.Substring(nPosAction + strKey.Length, nPosEnd - nPosAction - strKey.Length);
            }

            //get lt
            {
                string strKey = "name=\"lt\"";
                int nPosLtName = html.IndexOf(strKey, nPosForm);
                int nltBegin = html.IndexOf("\"", nPosLtName + strKey.Length);
                int nLtEnd = html.IndexOf("\"", nltBegin + 1);
                strlt = html.Substring(nltBegin + 1, nLtEnd - nltBegin - 1);
            }

            //get execute
            {
                string strKey = "name=\"execution\"";
                int nPosExecName = html.IndexOf(strKey, nPosForm);
                int nExecBegin = html.IndexOf("\"", nPosExecName + strKey.Length);
                int nExecEnd = html.IndexOf("\"", nExecBegin + 1);
                strExecution = html.Substring(nExecBegin + 1, nExecEnd - nExecBegin - 1);
            }

            return true;
        }

        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }
    }
}
