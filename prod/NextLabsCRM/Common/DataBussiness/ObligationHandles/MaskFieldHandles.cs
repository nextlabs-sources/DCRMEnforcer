using NextLabs.JavaPC.RestAPISDK.CEModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Common.DataModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NextLabs.CRMEnforcer.Common.DataBussiness.Cache;
using NextLabs.CRMEnforcer.Log;
namespace NextLabs.CRMEnforcer.Common.DataBussiness.ObligationHandle
{
    public class MaskFieldHandles
    {
        private string m_strEntityName = null;
        List<MaskFieldModel> m_lisMaskFieldModel = null;
        private NextLabsCRMLogs m_log = null;
        public MaskFieldHandles(List<CEObligation> lisObligations, MemoryCache<SecureEntity> entityCache, string strEntityName, NextLabsCRMLogs log)
        {
            m_lisMaskFieldModel = MaskFieldModel.GetCurrentObligation(lisObligations, entityCache, strEntityName);
            m_strEntityName = strEntityName;
            m_log = log;
            m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Build MaskFieldHandles instance success ,Entity name {1} get obligation count {0}", m_lisMaskFieldModel.Count, strEntityName));
        }
        public MaskFieldHandles(string strEntityName, NextLabsCRMLogs log)
        {
            m_strEntityName = strEntityName;
            m_log = log;

        }
        public MaskFieldHandles(List<MaskFieldModel> lisMaskFieldModel, MemoryCache<SecureEntity> entityCache, string strEntityName, NextLabsCRMLogs log)
        {
            m_lisMaskFieldModel = lisMaskFieldModel;
            m_strEntityName = strEntityName;
            m_log = log;
        }
        //work on list view post event
        public void DoObligation(AttributeCollection sourceAttributes, string[] arrayInputCoulmns, AttributeCollection displayAttribute, SecureEntity entityStruct)
        {
            m_log.WriteLog(Enums.LogLevel.Debug, "1,Start do obligation for mask field");
            foreach (MaskFieldModel maskFiedlModel in m_lisMaskFieldModel)
            {
                m_log.WriteLog(Enums.LogLevel.Debug, "Obligation filter count " + maskFiedlModel.DictFilters.Count);
                bool bAllFieldMatch = true;
                foreach (KeyValuePair<string, ApplySecurityFilterModel> itemFilter in maskFiedlModel.DictFilters)
                {
                    bool bFindAttr = false;
                    foreach (KeyValuePair<string, object> itemAttr in sourceAttributes)
                    {
                        if (itemAttr.Key.Equals(itemFilter.Value.Field, StringComparison.OrdinalIgnoreCase))
                        {
                            bool bPartMatch = itemFilter.Value.IsMatch(itemFilter.Value.Field, EntityDataHelp.GetValueOfValue(itemAttr.Value), m_log);
                            bAllFieldMatch = bAllFieldMatch && bPartMatch;
                            m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Compare Security Filter {0} Field {1} Result {2}", itemFilter.Key, itemFilter.Value.Field, bPartMatch));
                            bFindAttr = true;
                            break;
                        }
                    }
                    if (!bFindAttr)
                    {
                        bool bPartMatch = itemFilter.Value.IsMatch(itemFilter.Value.Field, null, m_log);
                        m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Compare Security Filter {0} Field {1}  this attribute donot have value , so set value NULL Result {2}", itemFilter.Key, itemFilter.Value.Field, bPartMatch));
                        bAllFieldMatch = bAllFieldMatch && bPartMatch;
                    }
                }
                if (bAllFieldMatch)
                {
                    List<KeyValuePair<string, object>> lisNewValue = new List<KeyValuePair<string, object>>();
                    foreach (string strColumn in arrayInputCoulmns)
                    {
                        if (displayAttribute.ContainsKey(strColumn))
                        {
                            if (maskFiedlModel.listMaskFields.Exists(dir => { return dir.Equals(strColumn, StringComparison.OrdinalIgnoreCase); }))
                            {
                                lisNewValue.Add(BuildFieldValue(strColumn, displayAttribute[strColumn], maskFiedlModel.MaskCharacter));
                            }
                        }
                        else
                        {
                            if (maskFiedlModel.listMaskFields.Exists(dir => { return dir.Equals(strColumn, StringComparison.OrdinalIgnoreCase); }))
                            {
                                lisNewValue.Add(BuildFieldValue(strColumn, entityStruct.Schema.Attributes.Find(dir => { return dir.LogicName.Equals(strColumn, StringComparison.OrdinalIgnoreCase); }).DataType, maskFiedlModel.MaskCharacter));
                            }
                        }
                    }
                    foreach (KeyValuePair<string, object> item in lisNewValue)
                    {
                        displayAttribute.Remove(item.Key);
                        if (item.Value != null)
                        {
                            displayAttribute.Add(item);
                        }
                    }
                }
            }
        }
        //work on from view pre
        public void DoObligation(AttributeCollection sourceAttributes, ref ColumnSet columnSet, ref List<Common.DataModel.Share.MaskField> lisMaskField)
        {
            m_log.WriteLog(Enums.LogLevel.Debug, "2,Start do obligation for mask field");
            foreach (MaskFieldModel maskFiedlModel in m_lisMaskFieldModel)
            {
                m_log.WriteLog(Enums.LogLevel.Debug, "Obligation filter count " + maskFiedlModel.DictFilters.Count);
                bool bAllFieldMatch = true;
                foreach (KeyValuePair<string, ApplySecurityFilterModel> itemFilter in maskFiedlModel.DictFilters)
                {
                    bool bFindAttr = false;
                    foreach (KeyValuePair<string, object> itemAttr in sourceAttributes)
                    {
                        if (itemAttr.Key.Equals(itemFilter.Value.Field, StringComparison.OrdinalIgnoreCase))
                        {
                            bool bPartMatch = itemFilter.Value.IsMatch(itemAttr.Key, EntityDataHelp.GetValueOfValue(itemAttr.Value), m_log);
                            bAllFieldMatch = bAllFieldMatch && bPartMatch;
                            m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Compare Security Filter {0} Field {1} Result {2}", itemFilter.Key, itemFilter.Value.Field, bPartMatch));
                            bFindAttr = true;
                            break;
                        }
                    }
                    if (!bFindAttr)
                    {
                        bool bPartMatch = itemFilter.Value.IsMatch(itemFilter.Value.Field, null, m_log);
                        m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Compare Security Filter {0} Field {1}  this attribute donot have value , so set value NULL Result {2}", itemFilter.Key, itemFilter.Value.Field, bPartMatch));
                        bAllFieldMatch = bAllFieldMatch && bPartMatch;
                    }
                }
                if (bAllFieldMatch)
                {
                    List<string> lisNeedMaskField = new List<string>();
                    DataModel.Share.MaskField maskField = new DataModel.Share.MaskField(maskFiedlModel.MaskCharacter);
                    foreach (string strColumn in columnSet.Columns)
                    {
                        if (maskFiedlModel.listMaskFields.Exists(dir => { return dir.Equals(strColumn, StringComparison.OrdinalIgnoreCase); }))
                        {
                            if (!lisNeedMaskField.Contains(strColumn))
                            {
                                lisNeedMaskField.Add(strColumn);
                            }
                            maskField.Fields.Add(strColumn);
                        }
                    }
                    lisMaskField.Add(maskField);
                    foreach (string strColumn in lisNeedMaskField)
                    {
                        columnSet.Columns.Remove(strColumn);
                    }

                }
            }
        }
        
        //work on from edit
        public void DoObligation(AttributeCollection sourceAttributes,AttributeCollection getedAttribute)
        {
            m_log.WriteLog(Enums.LogLevel.Debug, "3.5, Start do obligation for mask field");
            foreach (MaskFieldModel maskFiedlModel in m_lisMaskFieldModel)
            {
                m_log.WriteLog(Enums.LogLevel.Debug, "Obligation filter count " + maskFiedlModel.DictFilters.Count);
                bool bAllFieldMatch = true;
                foreach (KeyValuePair<string, ApplySecurityFilterModel> itemFilter in maskFiedlModel.DictFilters)
                {
                    bool bFindAttr = false;
                    foreach (KeyValuePair<string, object> itemAttr in getedAttribute)
                    {
                        if (itemAttr.Key.Equals(itemFilter.Value.Field, StringComparison.OrdinalIgnoreCase))
                        {
                            bool bPartMatch = 
                                itemFilter.Value.IsMatch(
                                    itemAttr.Key, 
                                    (itemAttr.Value is OptionSetValue) ? ((OptionSetValue)itemAttr.Value).Value:itemAttr.Value, 
                                    m_log);
                            bAllFieldMatch = bAllFieldMatch && bPartMatch;
                            m_log.WriteLog(Enums.LogLevel.Debug, string.Format("itemAttr.Value is {0}", (itemAttr.Value is OptionSetValue) ? ((OptionSetValue)itemAttr.Value).Value.ToString() : itemAttr.Value.ToString()));
                            m_log.WriteLog(Enums.LogLevel.Debug,
                                string.Format("Compare Security Filter {0} Field {1},Value {2} to Key {3},Value {4} Result {5}",
                                itemFilter.Key,
                                itemFilter.Value.Field,
                                itemFilter.Value.Value,
                                itemAttr.Key,
                                (itemAttr.Value is OptionSetValue) ? ((OptionSetValue)itemAttr.Value).Value.ToString() : itemAttr.Value.ToString(),
                                bPartMatch));
                            bFindAttr = true;
                            break;
                        }
                    }
                    if (!bFindAttr)
                    {
                        bool bPartMatch = itemFilter.Value.IsMatch(itemFilter.Value.Field, null, m_log);
                        m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Compare Security Filter {0} Field {1}  this attribute donot have value , so set value NULL Result {2}", itemFilter.Key, itemFilter.Value.Field, bPartMatch));
                        bAllFieldMatch = bAllFieldMatch && bPartMatch;
                    }
                }
                if (bAllFieldMatch)
                {
                    List<string> lisNeedMaskField = new List<string>();
                    foreach (string key in sourceAttributes.Keys)
                    {
                        if (maskFiedlModel.listMaskFields.Exists(dir => { return dir.Equals(key, StringComparison.OrdinalIgnoreCase); }))
                        {
                            if (!lisNeedMaskField.Contains(key))
                            {
                                lisNeedMaskField.Add(key);
                            }
                        }
                    }
                    foreach (string strColumn in lisNeedMaskField)
                    {
                        sourceAttributes.Remove(strColumn);
                    }

                }
            }
        }
        //work on from view post event
        public void DoObligation(List<DataModel.Share.MaskField> lisMakeFields, string[] arrayInputCoulmns, AttributeCollection displayAttribute,SecureEntity entityStruct)
        {
            List<KeyValuePair<string, object>> lisNewValue = new List<KeyValuePair<string, object>>();
            foreach (DataModel.Share.MaskField maskField in lisMakeFields)
            {
                foreach (string strField in maskField.Fields)
                {
                    lisNewValue.Add(BuildFieldValue(strField, entityStruct.Schema.Attributes.Find(dir => { return dir.LogicName.Equals(strField, StringComparison.OrdinalIgnoreCase); }).DataType, maskField.MaskCharacter));
                }
            }
            foreach (KeyValuePair<string, object> item in lisNewValue)
            {
                displayAttribute.Remove(item.Key);
                if (item.Value != null)
                {
                    displayAttribute.Add(item);
                }
            }
        }

        private KeyValuePair<string, object> BuildFieldValue(string strKey, Object objDisplayValue, string strMask)
        {
            m_log.WriteLog(Enums.LogLevel.Debug, "Build new value for " + strKey);
            object objNewValue = null;
            if (objDisplayValue.GetType() == typeof(String))
            {
                objNewValue = strMask;
            }
            //else if (objDisplayValue.GetType() == typeof(EntityReference))
            //{
            //    EntityReference tempObj = objDisplayValue as EntityReference;
            //    tempObj.Name = strMask;
            //    //if we don't give new guid for this, it can open record from mask field
            //    tempObj.Id = Guid.NewGuid();
            //    objNewValue = tempObj;
            //}
            else
            {
                m_log.WriteLog(Enums.LogLevel.Warning, string.Format("cannot mask field , due it type {0} unsupport", objDisplayValue.GetType()));
            }
            return new KeyValuePair<string, object>(strKey, objNewValue);
        }
        private KeyValuePair<string, object> BuildFieldValue(string strKey, string strDateType, string strMask)
        {
            m_log.WriteLog(Enums.LogLevel.Debug, "Build new value for " + strKey);
            object objNewValue = null;
            switch (strDateType.ToLower())
            {
                case EntityDataHelp.DataType_String_Lowcase:
                case EntityDataHelp.DataType_Memo_Lowcase:
                    {
                        objNewValue = strMask;
                    }
                    break;
                //case EntityDataHelp.DataType_Lookup_Lowcase:
                //case EntityDataHelp.DataType_Owner_Lowcase:
                //    {
                //        EntityReference tempObj = new EntityReference();
                //        tempObj.Name = strMask;
                //        //if we give a new id for this , it will bolck  we open reocrd
                //        //tempObj.Id = Guid.NewGuid();
                //        objNewValue = tempObj;
                //    }
                //    break;
                default:
                    {
                        m_log.WriteLog(Enums.LogLevel.Warning, string.Format("cannot mask field {1} , due it type {0} unsupport", strDateType, strKey));
                    }
                    break;
            }

            return new KeyValuePair<string, object>(strKey, objNewValue);
        }


        public bool NeedMaskField()
        {
            bool bResult = false;
            if(m_lisMaskFieldModel!=null&& m_lisMaskFieldModel.Count>0)
            {
                bResult = true;
            }
            return bResult;
        }
    }
}
