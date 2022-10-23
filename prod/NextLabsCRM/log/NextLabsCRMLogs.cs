using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness;
using NextLabs.CRMEnforcer.Common.Enums;
using NextLabs.CRMEnforcer.Plugin;
using System;

namespace NextLabs.CRMEnforcer.Log
{
    public class NextLabsCRMLogs
    {
        

#if DCRM2015
#warning This Is Build For Dynamic crm 2015.  
#elif DCRM2016
#warning This Is Build For Dynamic crm 2016.  
#else 
#warning Don't Defind condition compilation symbles
#endif

        private string m_user;
        //private string m_loglevel;
        private LogLevel m_logLevel;
#if DCRM2015

        private string m_event;
        private string m_entity;
        private IOrganizationService m_service;
#else
        private bool m_writeprefix;
        private ITracingService m_trace;
#endif

#if DCRM2015
        public NextLabsCRMLogs(string evt, string entity, string strLoginedUserFullname,LogLevel logLevev, IOrganizationService service)
#else
        public NextLabsCRMLogs(ITracingService trace,string strLoginedUserFullname, LogLevel logLevev)
#endif
        {
            m_user = strLoginedUserFullname;
            m_logLevel = logLevev;
#if DCRM2015
            m_event = evt;
            m_entity = entity;
            m_service = service;
#else
            m_trace = trace;
            m_writeprefix = false;
#endif
        }

#pragma warning disable 0168
        public void WriteLog(NextLabs.CRMEnforcer.Common.Enums.LogLevel level, string message)
        {
            try
            {
                if(m_logLevel.Equals(LogLevel.None))
                {
                    return;
                }
                else if (m_logLevel.Equals(LogLevel.Error))
                {
                    if (level.Equals(LogLevel.Error) == false)
                    {
                        return;
                    }
                }
                else if (m_logLevel.Equals(LogLevel.Warning))
                {
                    if (level.Equals(LogLevel.Debug) || level.Equals(LogLevel.Information))
                    {
                        return;
                    }
                }
                else if (m_logLevel.Equals(LogLevel.Information))
                {
                    if (level.Equals(LogLevel.Debug))
                    {
                        return;
                    }
                }
#if DCRM2015
                Entity log = new Entity("nxl_logs");

                log["nxl_user"] = m_user;
                log["nxl_event"] = m_event;
                log["nxl_entity"] = m_entity;

                log["nxl_time"] = DateTime.Now;

                log["nxl_messagelv"] = Enum.GetName(typeof(LogLevel), level);
                log["nxl_message"] = message;

                m_service.Create(log);
#else
            if (m_writeprefix == false)
            {
                m_writeprefix = true;
                m_trace.Trace("NXL:" + m_user);
            }

            m_trace.Trace(level + ":" + message);
#endif
            }
            catch(Exception ex)
            {
#warning we need find way handle exception when write log crash
                //can not find a way handle this exception on wirte log
            }
        }
    }
}
