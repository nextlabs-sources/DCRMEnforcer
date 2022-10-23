using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextLabs.CRMEnforcer.Common.DataBussiness.Obligation
{
    class FilterUtil
    {
        static public bool IsNullOrEmptyFilter(filter fetchFilter)
        {
            return (fetchFilter == null || fetchFilter.Items == null || fetchFilter.Items.Count == 0);
        }

        static public bool IsNullOrEmptyFilter(FilterExpression queryFilter)
        {
            if(queryFilter == null)
            {
                return true;
            }

            bool bExistCondtion = false;
            if(queryFilter.Conditions != null && queryFilter.Conditions.Count != 0)
            {
                bExistCondtion = true;
            }

            bool bExistFilter = false;
            if (queryFilter.Filters != null && queryFilter.Filters.Count != 0)
            {
                bExistFilter = true;
            }
           
            if(bExistFilter || bExistCondtion)
            {
                return false;
            }

            return true;
        }
        static public int FindTopEntityIndex(FetchType fetch)
        {
            if(fetch == null || fetch.Items == null || fetch.Items.Count == 0)
            {
                return -1;
            }

            int index = -1;
            for(int i = 0; i < fetch.Items.Count; i ++)
            {
                if(fetch.Items[i] is FetchEntityType)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        static public int FindTopFilterIndex(FetchEntityType fetchEntity)
        {
            if (fetchEntity == null || fetchEntity.Items == null || fetchEntity.Items.Count == 0)
            {
                return -1;
            }

            int index = -1;
            for (int i = 0; i < fetchEntity.Items.Count; i++)
            {
                if (fetchEntity.Items[i] is filter)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }
        static public FilterExpression Merge(List<FilterExpression> filters, LogicalOperator op,ref FilterExpression head)
        {
            if((filters == null || filters.Count == 0) && head == null)
            {
                return null;
            }

            if(head == null)
            {
                head = new FilterExpression(op);
            }
            else
            {
                if(head.FilterOperator != op)
                {
                    FilterExpression temp = head;
                    head = new FilterExpression(op);
                    head.AddFilter(temp);
                }
            }

            foreach(FilterExpression filter in filters)
            {
                head.AddFilter(filter);
            }

            return head;
        }

        static public filter Merge(List<filter> filters, filterType op, ref filter head)
        {
            if ((filters == null || filters.Count == 0) && head == null)
            {
                return null;
            }

            if (head == null)
            {
                head = new filter();
                head.type = op;
                head.Items = new List<object>();
            }
            else
            {
                if (head.type != op)
                {                    
                    filter temp = head;
                    head = new filter();
                    head.type = op;
                    head.Items = new List<object>();
                    head.Items.Add(temp);
                }
            }

            foreach (filter item in filters)
            {                
                head.Items.Add(item);
            }

            return head;
        }

        static public ConditionExpression CloneCondition(ConditionExpression source)
        {
            if(source == null)
            {
                return null;
            }
            ConditionExpression condition = new ConditionExpression();
            condition.AttributeName = source.AttributeName;
            condition.EntityName = source.EntityName;
            condition.Operator = source.Operator;
            condition.Values.AddRange(source.Values);
            return condition;
        }

        static public conditionValue CloneConditionValue(conditionValue source)
        {
            conditionValue value = new conditionValue();
            value.uiname = source.uiname;
            value.uitype = source.uitype;
            value.Value = source.Value;
            return value;
        }

        static public List<conditionValue> CloneConditionValues(List<conditionValue> source)
        {
            if(source == null)
            {
                return null;
            }
            List<conditionValue> values = new List<conditionValue>();
            for (int index = 0; index < source.Count; index++)
            {
                values.Add(source[index]);
            }

            return values;
        }
        static public condition CloneCondition(condition source)
        {
            if (source == null)
            {
                return null;
            }
            condition cond = new condition();
            cond.column = source.column;
            cond.entityname = source.entityname;
            cond.attribute = source.attribute;
            cond.aggregate = source.aggregate;
            cond.aggregateSpecified = source.aggregateSpecified;
            cond.alias = source.alias;
            cond.@operator = source.@operator;
            cond.rowaggregate = source.rowaggregate;
            cond.rowaggregateSpecified = source.rowaggregateSpecified;
            cond.uihidden = source.uihidden;
            cond.uihiddenSpecified = source.uihiddenSpecified;
            cond.uiname = source.uiname;
            cond.uitype = source.uitype;
            cond.value = source.value;

            if (source.Items != null)
            {
                cond.Items = CloneConditionValues(source.Items);
            }
            return cond;
        }
        static public FilterExpression CloneFilter(FilterExpression source)
        {
            if(source == null)
            {
                return null;
            }

            FilterExpression fe = new FilterExpression(source.FilterOperator);
            fe.IsQuickFindFilter = source.IsQuickFindFilter;

            int subFilterCount = source.Filters.Count;
            for (int index = 0; index < subFilterCount; index++)
            {
                fe.AddFilter(CloneFilter(source.Filters[index]));
            }

            int subConditionCount = source.Conditions.Count();
            for (int index = 0; index < subConditionCount;index++)
            {
                fe.AddCondition(CloneCondition(source.Conditions[index]));
            }
            return fe;
        }

        static public filter CloneFilter(filter source)
        {
            if (source == null)
            {
                return null;
            }

            filter fe = new filter();
            fe.isquickfindfields = source.isquickfindfields;
            fe.isquickfindfieldsSpecified = source.isquickfindfieldsSpecified;
            fe.type = source.type;

            if(source.Items == null)
            {
                return fe;
            }

            fe.Items = new List<object>();
            int subFilterCount = source.Items.Count;
            for (int index = 0; index < subFilterCount; index++)
            {
                object sourceItem = source.Items[index];
                if (sourceItem is filter)
                {
                    filter sourceSubFilter = CloneFilter((filter)sourceItem);
                    if (sourceSubFilter != null)
                    {
                        fe.Items.Add(sourceSubFilter);
                    }
                }
                else if (sourceItem is condition)
                {
                    condition sourceSubCondition = CloneCondition((condition)sourceItem);
                    if (sourceSubCondition != null)
                    {
                        fe.Items.Add(sourceSubCondition);
                    }
                }
                else
                {
                    //should never reach here
                }
            }
            return fe;
        }
        static public void CompressQueryFilter(FilterExpression parent, FilterExpression currentFilter)
        {
            int subFilterCount = currentFilter.Filters.Count;

            for (int index = subFilterCount - 1; index >= 0; index--)
            {
                if (currentFilter.Filters[index].Filters.Count == 0)
                {
                    if (currentFilter.Filters[index].Conditions.Count == 0)
                    {
                        currentFilter.Filters.RemoveAt(index);
                    }
                }
                else
                {
                    CompressQueryFilter(currentFilter, currentFilter.Filters[index]);
                }
            }

            if (currentFilter.Filters.Count == 1 && currentFilter.Conditions.Count == 0)
            {
                parent.Filters.Add(currentFilter.Filters[0]);
                parent.Filters.Remove(currentFilter);
            }
            else if (currentFilter.Filters.Count == 0 && currentFilter.Conditions.Count == 0)
            {
                parent.Filters.Remove(currentFilter);
            }
        }

        static private void GetFetchFiltersAndConditions(List<object> items, List<filter> filters, List<condition> conditions)
        {
            foreach (object obj in items)
            {
                if(obj is filter)
                {
                    filters.Add((filter)obj);
                }
                else if(obj is condition)
                {
                    conditions.Add((condition)obj);
                }
            }
        }
        static public void CompressFetchFilter(filter parent, filter currentFilter)
        {
            List<filter> filters = new List<filter>();
            List<condition> conditions = new List<condition>();
            
            GetFetchFiltersAndConditions(currentFilter.Items, filters, conditions);
            int filterCount = filters.Count;
            int conditionCount = conditions.Count;

            for (int index = filterCount - 1; index >= 0; index--)
            {
                object item = currentFilter.Items[index];
                if (item is filter)
                {
                    
                    filter subFilter = (filter)item;
                    List<filter> subFilters = new List<filter>();
                    List<condition> subConditions = new List<condition>();
                    GetFetchFiltersAndConditions(subFilter.Items, subFilters, subConditions);
                    int subSubFilterCount = subFilters.Count;
                    int subConditionCount = subConditions.Count;

                    if (subSubFilterCount == 0)
                    {
                        if (subConditionCount == 0)
                        {
                            currentFilter.Items.RemoveAt(index);
                        }
                    }
                    else
                    {
                        CompressFetchFilter(currentFilter, subFilter);
                    }
                }
            }

            filters.Clear();
            conditions.Clear();
            GetFetchFiltersAndConditions(currentFilter.Items, filters,conditions);

            if (filters.Count == 1 && conditions.Count == 0)
            {
                parent.Items.Add(currentFilter.Items[0]);
                parent.Items.Remove(currentFilter);
            }
            else if (filters.Count == 0 && conditions.Count == 0)
            {
                parent.Items.Remove(currentFilter);
            }
        }
    }
}
