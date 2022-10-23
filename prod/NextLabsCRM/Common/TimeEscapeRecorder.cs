using NextLabs.CRMEnforcer.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common
{
    class TimeEscapeRecorder
    {
        public class TimeSection
        {
            private DateTime m_pevTime = DateTime.Now;
            private DateTime m_afterTime = DateTime.Now;
            private string m_sectionName = "global";
            public TimeSection(string name)
            {
                m_sectionName = name;
            }
          
            public double GetAndUpdateEscape()
            {
                m_afterTime = DateTime.Now;
                double escape = m_afterTime.Subtract(m_pevTime).TotalMilliseconds;
                m_pevTime = m_afterTime;

                return escape;
            }
        }

        private NextLabsCRMLogs m_log;
        private bool m_enabled;
        private Dictionary<string, TimeSection> m_timeSectionDic = new Dictionary<string, TimeSection>();

        public TimeEscapeRecorder(NextLabsCRMLogs log,bool enabled)
        {
            m_log = log;
            m_enabled = enabled;
        }

        public void RecordTime(string sectionName, string description)
        {
            if(!m_enabled)
            {
                return;
            }
            if(m_log == null)
            {
                return;
            }

            if(m_timeSectionDic.ContainsKey(sectionName))
            {
                TimeSection section = m_timeSectionDic[sectionName];
                m_log.WriteLog(Enums.LogLevel.Debug, "[timecost]"+sectionName + ";" + description + ";Time cost:" + section.GetAndUpdateEscape()+"ms");
            }
            else
            {
                TimeSection section = new TimeSection(sectionName);
                m_timeSectionDic[sectionName] = section;

                m_log.WriteLog(Enums.LogLevel.Debug, sectionName+";" + description);
            }
        }
        
    }
}
