using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    static class JPCWrapper
    {
        private static NextLabs.JavaPC.RestAPISDK.JavaPC m_javaPC;
        private static System.Threading.ReaderWriterLock m_rwLock = new System.Threading.ReaderWriterLock();
        public static NextLabs.JavaPC.RestAPISDK.JavaPC JAVAPC
        {
            get
            {
                try
                {
                    m_rwLock.AcquireReaderLock(Int32.MaxValue);
                    return m_javaPC;
                }
                finally
                {
                    if (m_rwLock.IsReaderLockHeld)
                    {
                        m_rwLock.ReleaseReaderLock();
                    }
                }
            }
        }

        public static void Update(Common.DataModel.GeneralSetting setting)
        {
            try
            {
                m_rwLock.AcquireWriterLock(Int32.MaxValue);
                string jpcHostAddress = Util.GetJPCHost(setting.PolicyControllerHost) + ":" + Util.GetJPCPort();
                //workaround with local jpc
                if (jpcHostAddress.IndexOf("qalab01") != -1)
                {
                    jpcHostAddress = @"http:\\" + Util.GetJPCHost(setting.PolicyControllerHost) + ":58080";
                }
                else if (setting.IsHttps)
                //if (setting.IsHttps)
                {
                    jpcHostAddress = @"https://" + jpcHostAddress;
                }
                else
                {
                    jpcHostAddress = @"http://" + jpcHostAddress;
                }

                if (!string.IsNullOrWhiteSpace(setting.OAuthServerHost))
                {
                    string oauthHostAddress = @"https://"+ Util.GetCCHost(setting.OAuthServerHost) + ":" + Util.GetCCPort();
                    m_javaPC =
                        new NextLabs.JavaPC.RestAPISDK.JavaPC(NextLabs.JavaPC.RestAPISDK.RequestDataType.Json,
                                                                                        jpcHostAddress,
                                                                                        oauthHostAddress,
                                                                                        setting.ClientID,
                                                                                        setting.ClientPassword,
                                                                                        null, true, 3600);
                }
                else
                {
                    m_javaPC =
                        new NextLabs.JavaPC.RestAPISDK.JavaPC(NextLabs.JavaPC.RestAPISDK.RequestDataType.Json,
                                                                                        jpcHostAddress,
                                                                                        null);
                }
            }
            finally
            {
                if (m_rwLock.IsWriterLockHeld)
                {
                    m_rwLock.ReleaseWriterLock();
                }
            }
        }
    }
}
