using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace NextLabs.JavaPC.RestAPISDK
{
    [Obsolete]
    class HttpHelp
    {
        HttpWebRequest m_request;

        public HttpHelp(string strContentType, string strMethod, string strUrl, bool bKeepAlive, List<KeyValuePair<string, string>> lisHeaders, int timeout)
        {
            m_request = (HttpWebRequest)WebRequest.Create(strUrl);
            m_request.KeepAlive = bKeepAlive;
            m_request.Method = strMethod;
            m_request.ContentType = strContentType;
            if (lisHeaders != null && lisHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> header in lisHeaders)
                {
                    m_request.Headers.Add(header.Key, header.Value);
                }
            }
            m_request.Timeout = timeout;
        }

        public PCResult SendDataAndGetResponse(string strRequest, Encoding encode, ref CookieContainer cookie, out string strResponse, out List<KeyValuePair<string, string>> lisHeaders)
        {
            PCResult result = PCResult.OK;
            strResponse = string.Empty;
            lisHeaders = new List<KeyValuePair<string, string>>();
            m_request.CookieContainer = cookie;

            if (!string.IsNullOrEmpty(strRequest))
            {
                try
                {
                    using (Stream stream = m_request.GetRequestStream())
                    {
                        using (StreamWriter sw = new StreamWriter(stream, encode))
                        {
                            sw.Write(strRequest);
                            sw.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = PCResult.SendFaild;
                }
                finally
                {

                }
            }
            if (result.Equals(PCResult.OK))
            {
                HttpWebResponse response = null;
                try
                {
                    if ((cookie != null && cookie.Count == 0) || cookie == null)
                    {
                        m_request.CookieContainer = new CookieContainer();
                        cookie = m_request.CookieContainer;
                    }
                    else
                    {
                        m_request.CookieContainer = cookie;
                    }
                    response = (HttpWebResponse)m_request.GetResponse();
                    string encoding = response.ContentEncoding;
                    if (encoding == null || encoding.Length < 1)
                    {
                        encoding = Constant.HttpRquest.Endcoding_UTF_8;
                    }
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream, Encoding.GetEncoding(encoding)))
                        {
                            strResponse = sr.ReadToEnd();
                        }
                    }
                    for (int i = 0; i < response.Headers.Count; i++)
                    {
                        lisHeaders.Add(new KeyValuePair<string, string>(response.Headers.Keys[i], response.Headers[response.Headers.Keys[i]]));
                    }
                }
                catch (Exception ex)
                {

                    result = ModelTransform.Function.TransformFormWebExceptionToPcResult(ex.Message);
                }
                finally
                {

                }
            }
            return result;
        }
    }

    class HttpHelpHttpClient
    {
        private object m_lockObject = new object();
        private System.Net.Http.HttpClient[] m_arryClients = new System.Net.Http.HttpClient[NextLabs.JavaPC.RestAPISDK.Constant.HttpRquest.MaxConnect];
        private int m_iCurrentIndex = 0;
        private int CurrentIndex
        {
            get
            {
                lock (m_lockObject)
                {
                    int tempCurrectIndex = m_iCurrentIndex;
                    if (m_iCurrentIndex >= NextLabs.JavaPC.RestAPISDK.Constant.HttpRquest.MaxConnect-1)
                    {
                        m_iCurrentIndex = 0;
                    }
                    else
                    {
                        m_iCurrentIndex++;
                    }
                    return tempCurrectIndex;
                }
            }
        }

        public HttpHelpHttpClient()
        {
            for (int i = 0; i < m_arryClients.Length; i++)
            {
                m_arryClients[i] = new HttpClient();
            }

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
        /// <param name="bUseSingleHttpClient">Because cannot add cookie for httpClient, so at 8.0.2 version, we need only use single httpclient object, but at 8.0.3 version, auto use token ,we can create A large number of httpclient object</param>
        /// <returns></returns>
        public PCResult SendDataAndGetResponse(string strMethod, string strContentType, List<KeyValuePair<string, string>> lisHeaders, string strUrl, string strRequest, Encoding encode, out string strResponse, out List<KeyValuePair<string, string>> lisOutHeaders, bool bUseSingleHttpClient=false)
        {
            PCResult pcResult = PCResult.Failed;
            lisOutHeaders = new List<KeyValuePair<string, string>>();
            strResponse = string.Empty;
            HttpMethod method = new HttpMethod(strMethod);
            Uri uriSite = new Uri(strUrl);
            HttpRequestMessage requestMessage = new HttpRequestMessage(method, uriSite);
            if (!string.IsNullOrEmpty(strRequest))
            {
                requestMessage.Content = new StringContent(strRequest, encode, strContentType);
                requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(strContentType);
            }

            if (lisHeaders != null && lisHeaders.Count > 0)
            {
                foreach (KeyValuePair<string, string> header in lisHeaders)
                {
                    if (header.Value != null)
                    {
                        requestMessage.Headers.Add(header.Key, header.Value);
                    }
                }
            }


            HttpResponseMessage httpResponse;
            try
            {
                if (!bUseSingleHttpClient)
                {
                    httpResponse = m_arryClients[CurrentIndex].SendAsync(requestMessage).Result;
                }
                else
                {
                    httpResponse = m_arryClients[0].SendAsync(requestMessage).Result;
                }
                if (httpResponse.IsSuccessStatusCode)
                {
                    foreach (var p in httpResponse.Headers)
                    {
                        lisOutHeaders.Add(new KeyValuePair<string, string>(p.Key, string.Join(" ", p.Value)));
                    }
                    strResponse = httpResponse.Content.ReadAsStringAsync().Result;
                    pcResult = PCResult.OK;
                }
                else
                {
                    pcResult = ModelTransform.Function.TransformFromStatusCodeToPcResult(httpResponse.StatusCode);
                }
            }
            catch (Exception ex)
            {
                pcResult = PCResult.SendFaild;
            }
            return pcResult;
        }
    }
}
