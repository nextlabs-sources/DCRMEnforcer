using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    public class ChartDatadefinitionXMLHelp
    {
        private const string XPATHCurrentEntityName = "datadefinition/fetchcollection/fetch/entity";
        private const string XPATHFetch = "datadefinition/fetchcollection/fetch";
        private const string AttributeKeyName = "name";
        private const string AttributeKeyType = "type";
        private const string AttributeKeyAttribute = "attribute";
        private const string AttributeKeyOperator = "operator";
        private const string AttributeKeyValue = "value";

        private const string NodeNameKeyFilter = "filter";
        private const string NodeNameKeyCondition = "condition";
        private const string FilterlogicAnd = "and";
        
        public string CurrentEntityName
        {
            get
            {
                string strCurrentEntityName = null;
                XmlNode nodeEntityName = GetSingleXmlNodeByXPATH(XMLObject, XPATHCurrentEntityName);
                if (nodeEntityName != null)
                {
                    if (nodeEntityName.Attributes[AttributeKeyName] != null)
                    {
                        strCurrentEntityName = nodeEntityName.Attributes[AttributeKeyName].Value;
                    }
                }
                return strCurrentEntityName;
            }
        }

        public string GetFetchXMLInitial()
        {
            XmlNode nodeEntity = XMLObject.SelectSingleNode(XPATHFetch);
            if (nodeEntity != null)
            {
                return nodeEntity.OuterXml;
            }
            else
            {
                throw new Exception("Cannot get fetch xml from chart datadefinition ,detail "+this.ToString());
            }
        }

        public void ReplaceFetch(string strNewFetchXML)
        {
            XmlNode nodeEntity = XMLObject.SelectSingleNode(XPATHFetch);
            if (nodeEntity != null)
            {
                nodeEntity.ParentNode.InnerXml = strNewFetchXML;
            }
            else
            {
                throw new Exception("Cannot get fetch xml from chart datadefinition ,detail " + this.ToString());
            }

        }

        XmlDocument m_xmlObject = null;
        private XmlDocument XMLObject
        {
            get
            {
                return m_xmlObject;
            }
        }
        private ChartDatadefinitionXMLHelp(XmlDocument xmlObject)
        {
            m_xmlObject = xmlObject;
        }
        public static ChartDatadefinitionXMLHelp CreateInstance(string strxml)
        {
            ChartDatadefinitionXMLHelp chartDatadefinitionXMLHelp = null;
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(strxml);
                if (xml != null)
                {
                    chartDatadefinitionXMLHelp = new ChartDatadefinitionXMLHelp(xml);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("NextLabs.CRMEnforcer.Common.DataBussiness.ChartDatadefinitionXMLHelp CreateInstance Faild:" + ex.Message, ex);
            }
            return chartDatadefinitionXMLHelp;
        }

        private XmlNode GetSingleXmlNodeByXPATH(XmlDocument xml, string strXPath)
        {
            return xml.SelectSingleNode(strXPath);
        }

        private void CreateFilterNode(XmlNode parentNode, Microsoft.Xrm.Sdk.Query.FilterExpression filter)
        {
            XmlNode FilterNode = XMLObject.CreateNode(XmlNodeType.Element, NodeNameKeyFilter, null);
            XmlAttribute FilterAttr = XMLObject.CreateAttribute(AttributeKeyType);
            FilterAttr.Value = Enum.GetName(typeof(Microsoft.Xrm.Sdk.Query.LogicalOperator), filter.FilterOperator).ToLower();
            FilterNode.Attributes.Append(FilterAttr);
            parentNode.AppendChild(FilterNode);
            foreach (var condition in filter.Conditions)
            {
                XmlElement elementCondition = XMLObject.CreateElement(NodeNameKeyCondition);
                elementCondition.SetAttribute(AttributeKeyAttribute, condition.AttributeName);
                elementCondition.SetAttribute(AttributeKeyOperator, TranfromConditionOperatorToString(condition.Operator));
                elementCondition.SetAttribute(AttributeKeyValue, string.Join(" ", condition.Values.ToArray()));

                FilterNode.AppendChild(elementCondition);
            }
            if (filter.Filters != null)
            {
                foreach (var singleFilter in filter.Filters)
                {
                    CreateFilterNode(FilterNode, singleFilter);
                }
            }
        }

        public void AddFilter(Microsoft.Xrm.Sdk.Query.FilterExpression filters)
        {
            XmlNode nodeEntity = XMLObject.SelectSingleNode(XPATHCurrentEntityName);

            if (nodeEntity != null)
            {
                CreateFilterNode(nodeEntity, filters);
            }
        }

        public override string ToString()
        {
            return XMLObject.InnerXml;
        }

        /// <summary>
        /// TranfromConditionOperatorToString
        /// refer https://msdn.microsoft.com/en-us/library/gg309405.aspx
        /// </summary>
        /// <param name="conditionOperator"></param>
        /// <returns></returns>
        private string TranfromConditionOperatorToString(Microsoft.Xrm.Sdk.Query.ConditionOperator conditionOperator)
        {
            switch (conditionOperator)
            {
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Equal:
                    {
                        return "eq";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotEqual:
                    {
                        return "neq";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.DoesNotEndWith:
                    {
                        return "ne";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.GreaterThan:
                    {
                        return "gt";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.GreaterEqual:
                    {
                        return "ge";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LessEqual:
                    {
                        return "le";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LessThan:
                    {
                        return "lt";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Like:
                    {
                        return "like";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.In:
                    {
                        return "in";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotIn:
                    {
                        return "not-in";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Between:
                    {
                        return "between";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotBetween:
                    {
                        return "not-between";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Null:
                    {
                        return "null";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotNull:
                    {
                        return "not-null";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Yesterday:
                    {
                        return "yesterday";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Today:
                    {
                        return "today";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Tomorrow:
                    {
                        return "tomorrow";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Last7Days:
                    {
                        return "last-seven-days";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Next7Days:
                    {
                        return "next-seven-days";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastWeek:
                    {
                        return "last-week";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.ThisWeek:
                    {
                        return "this-week";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextWeek:
                    {
                        return "next-week";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastMonth:
                    {
                        return "last-month";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.ThisMonth:
                    {
                        return "this-month";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextMonth:
                    {
                        return "next-month";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.On:
                    {
                        return "on";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OnOrBefore:
                    {
                        return "on-or-before";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OnOrAfter:
                    {
                        return "on-or-after";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastYear:
                    {
                        return "last-year";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.ThisYear:
                    {
                        return "this-year";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextYear:
                    {
                        return "next-year";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXHours:
                    {
                        return "last-x-hours";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXHours:
                    {
                        return "next-x-hours";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXDays:
                    {
                        return "last-x-days";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXDays:
                    {
                        return "next-x-days";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXWeeks:
                    {
                        return "last-x-weeks";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXWeeks:
                    {
                        return "next-x-weeks";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXMonths:
                    {
                        return "last-x-months";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXMonths:
                    {
                        return "next-x-months";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OlderThanXMonths:
                    {
                        return "olderthan-x-months";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OlderThanXYears:
                    {
                        return "olderthan-x-years";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OlderThanXWeeks:
                    {
                        return "olderthan-x-weeks";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OlderThanXDays:
                    {
                        return "olderthan-x-days";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OlderThanXHours:
                    {
                        return "olderthan-x-hours";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.OlderThanXMinutes:
                    {
                        return "olderthan-x-minutes";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXYears:
                    {
                        return "last-x-years";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXYears:
                    {
                        return "next-x-years";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualUserId:
                    {
                        return "eq-userid";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotEqualUserId:
                    {
                        return "ne-userid";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualUserTeams:
                    {
                        return "eq-userteams";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualUserOrUserTeams:
                    {
                        return "eq-useroruserteams";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualUserOrUserHierarchy:
                    {
                        return "eq-useroruserhierarchy";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualUserOrUserHierarchyAndTeams:
                    {
                        return "eq-useroruserhierarchyandteams";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualBusinessId:
                    {
                        return "eq-businessid";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotEqualBusinessId:
                    {
                        return "ne-businessid";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EqualUserLanguage:
                    {
                        return "eq-userlanguage";
                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.ThisFiscalYear:
                    {
                        return "this-fiscal-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.ThisFiscalPeriod:
                    {
                        return "this-fiscal-period";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextFiscalYear:
                    {
                        return "next-fiscal-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextFiscalPeriod:
                    {
                        return "next-fiscal-period";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastFiscalYear:
                    {
                        return "last-fiscal-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastFiscalPeriod:
                    {
                        return "last-fiscal-period";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXFiscalYears:
                    {
                        return "last-x-fiscal-years";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.LastXFiscalPeriods:
                    {
                        return "last-x-fiscal-periods";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXFiscalYears:
                    {
                        return "next-x-fiscal-years";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NextXFiscalPeriods:
                    {
                        return "next-x-fiscal-periods";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.InFiscalYear:
                    {
                        return "in-fiscal-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.InFiscalPeriod:
                    {
                        return "in-fiscal-period";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.InFiscalPeriodAndYear:
                    {
                        return "in-fiscal-period-and-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.InOrBeforeFiscalPeriodAndYear:
                    {
                        return "in-or-before-fiscal-period-and-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.InOrAfterFiscalPeriodAndYear:
                    {
                        return "in-or-after-fiscal-period-and-year";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.BeginsWith:
                    {
                        return "begins-with";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.DoesNotBeginWith:
                    {
                        return "not-begin-with";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.EndsWith:
                    {
                        return "ends-with";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Under:
                    {
                        return "under";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.UnderOrEqual:
                    {
                        return "eq-or-under";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.NotUnder:
                    {
                        return "not-under";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.Above:
                    {
                        return "above";

                    }
                case Microsoft.Xrm.Sdk.Query.ConditionOperator.AboveOrEqual:
                    {
                        return "eq-or-above";
                    }
                default:
                    {
                        return "eq";
                    }

            }
        }
    }
}
