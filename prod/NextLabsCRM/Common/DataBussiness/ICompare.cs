using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NextLabs.CRMEnforcer.Log;

namespace NextLabs.CRMEnforcer.Common.DataBussiness
{
    interface ICompare
    {
        bool Compare(object obj1, ConditionOperator operation, object obj2);
    }
    public class CompareBase
    {
        protected bool CheckOperate(Type dateType, ConditionOperator operation)
        {
            bool bResult = false;
            if (operation.Equals(ConditionOperator.Null) || operation.Equals(ConditionOperator.NotNull))
            {
                bResult = true;
            }
            else
            {
                if (dateType == typeof(String))
                {
                    if (operation == ConditionOperator.Equal
                        || operation == ConditionOperator.NotEqual)
                    {
                        bResult = true;
                    }
                }
                else
                {
                    if (dateType == typeof(Decimal) ||
                        dateType == typeof(Double) ||
                        dateType == typeof(Int64) ||
                        dateType == typeof(int) ||
                        dateType == typeof(bool))
                    {
                        if (operation == ConditionOperator.Equal
                        || operation == ConditionOperator.NotEqual
                        || operation == ConditionOperator.GreaterEqual
                        || operation == ConditionOperator.GreaterThan
                        || operation == ConditionOperator.LessThan
                        || operation == ConditionOperator.LessEqual)
                        {
                            bResult = true;
                        }
                    }
                }
            }
            return bResult;
        }
        protected bool NULLCompare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            switch (operation)
            {
                case ConditionOperator.Equal:
                    {
                        if (obj1 == obj2)
                        {
                            bResult = true;
                        }
                    }
                    break;
                case ConditionOperator.NotEqual:
                    {
                        if (obj1 != obj2)
                        {
                            bResult = true;
                        }
                    }
                    break;
                case ConditionOperator.GreaterEqual:
                    {
                        if (obj2 == null)
                        {
                            bResult = true;
                        }
                    }
                    break;
                case ConditionOperator.GreaterThan:
                    {
                        if (obj1 != null)
                        {
                            if (obj2 == null)
                            {
                                bResult = true;
                            }
                        }
                    }
                    break;
                case ConditionOperator.LessEqual:
                    {
                        if (obj1 == null)
                        {
                            bResult = true;
                        }
                    }
                    break;
                case ConditionOperator.LessThan:
                    {
                        if (obj2 != null)
                        {
                            if (obj1 == null)
                            {
                                bResult = true;
                            }
                        }
                    }
                    break;
                case ConditionOperator.Null:
                    {
                        if(obj1==null)
                        {
                            bResult = true;
                        }
                    }
                    break;
                case ConditionOperator.NotNull:
                    {
                        if(obj1!=null)
                        {
                            bResult = true;
                        }
                    }
                    break;
            }
            return bResult;
        }
    }
    public class StringCompare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public StringCompare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {

            bool bResult = false;
            if (obj1 == null || obj2 == null|| operation.Equals(ConditionOperator.Null)||operation.Equals(ConditionOperator.NotNull))
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                string strValue1 = obj1.ToString();
                string strValue2 = obj2.ToString();
                if (operation.Equals(ConditionOperator.Equal))
                {
                    if (strValue1.Equals(strValue2, StringComparison.OrdinalIgnoreCase))
                    {
                        bResult = true;
                    }
                }
                else if (operation.Equals(ConditionOperator.NotEqual))
                {
                    if (!strValue1.Equals(strValue2, StringComparison.OrdinalIgnoreCase))
                    {
                        bResult = true;
                    }
                }
                else
                {
                    throw new Exceptions.InvalidOperationException(typeof(string), operation);
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("StringCompare obj1 {0} operator {1} obj2 {2} ,result {3} ", strValue1, operation.ToString(), strValue2, bResult));
            }
            return bResult;
        }
    }
    public class DecimalCompare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public DecimalCompare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            if (obj1 == null || obj2 == null)
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                Decimal value1, value2;
                try
                {
                    value1 = Decimal.Parse(obj1.ToString());
                    value2 = Decimal.Parse(obj2.ToString());
                }
                catch (FormatException ex)
                {
                    throw new FormatException(string.Format("Cannot parse {0} or {1} to type", obj1.ToString(), obj2.ToString(), "Decimal"), ex);
                }
                switch (operation)
                {
                    case ConditionOperator.Equal:
                        {
                            if (value1 == value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.NotEqual:
                        {
                            if (value1 != value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterEqual:
                        {
                            if (value1 >= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterThan:
                        {
                            if (value1 > value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessEqual:
                        {
                            if (value1 <= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessThan:
                        {
                            if (value1 < value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exceptions.InvalidOperationException(value1.GetType(), operation);
                        }
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("DecimalCompare obj1 {0} operator {1} obj2 {2} ,result {3} ", value1, operation.ToString(), value2, bResult));
            }
                return bResult;
        }
    }
    public class DoubleCompare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public DoubleCompare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            if (obj1 == null || obj2 == null)
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                Double value1, value2;
                try
                {
                    value1 = Double.Parse(obj1.ToString());
                    value2 = Double.Parse(obj2.ToString());
                }
                catch (FormatException ex)
                {
                    throw new FormatException(string.Format("Cannot parse {0} or {1} to type", obj1.ToString(), obj2.ToString(), "Double"), ex);
                }
                switch (operation)
                {
                    case ConditionOperator.Equal:
                        {
                            if (value1 == value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.NotEqual:
                        {
                            if (value1 != value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterEqual:
                        {
                            if (value1 >= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterThan:
                        {
                            if (value1 > value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessEqual:
                        {
                            if (value1 <= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessThan:
                        {
                            if (value1 < value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exceptions.InvalidOperationException(value1.GetType(), operation);
                        }
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("DoubleCompare obj1 {0} operator {1} obj2 {2} ,result {3} ", value1, operation.ToString(), value2, bResult));
                
            }
            return bResult;
        }
    }
    public class Int64Compare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public Int64Compare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            if (obj1 == null || obj2 == null)
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                Int64 value1, value2;
                try
                {
                    value1 = Int64.Parse(obj1.ToString());
                    value2 = Int64.Parse(obj2.ToString());
                }
                catch (FormatException ex)
                {
                    throw new FormatException(string.Format("Cannot parse {0} or {1} to type", obj1.ToString(), obj2.ToString(), "Int64"), ex);
                }
                switch (operation)
                {
                    case ConditionOperator.Equal:
                        {
                            if (value1 == value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.NotEqual:
                        {
                            if (value1 != value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterEqual:
                        {
                            if (value1 >= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterThan:
                        {
                            if (value1 > value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessEqual:
                        {
                            if (value1 <= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessThan:
                        {
                            if (value1 < value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exceptions.InvalidOperationException(value1.GetType(), operation);
                        }
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("Int64Compare obj1 {0} operator {1} obj2 {2} ,result {3} ", value1, operation.ToString(), value2, bResult));
            }
            return bResult;
        }
    }
    public class IntCompare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public IntCompare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            if (obj1 == null || obj2 == null)
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                int value1, value2;
                try
                {
                    value1 = int.Parse(obj1.ToString());
                    value2 = int.Parse(obj2.ToString());
                }
                catch (FormatException ex)
                {
                    throw new FormatException(string.Format("Cannot parse {0} or {1} to type", obj1.ToString(), obj2.ToString(), "int"), ex);
                }
                switch (operation)
                {
                    case ConditionOperator.Equal:
                        {
                            if (value1 == value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.NotEqual:
                        {
                            if (value1 != value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterEqual:
                        {
                            if (value1 >= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.GreaterThan:
                        {
                            if (value1 > value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessEqual:
                        {
                            if (value1 <= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.LessThan:
                        {
                            if (value1 < value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exceptions.InvalidOperationException(value1.GetType(), operation);
                        }
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("IntCompare obj1 {0} operator {1} obj2 {2} ,result {3} ", value1, operation.ToString(), value2, bResult));
                
            }
            return bResult;
        }
    }
    public class BooleanCompare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public BooleanCompare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            if (obj1 == null || obj2 == null)
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                Boolean value1, value2;
                try
                {
                    value1 = Boolean.Parse(obj1.ToString());
                    value2 = Boolean.Parse(obj2.ToString());
                }
                catch (FormatException ex)
                {
                    throw new FormatException(string.Format("Cannot parse {0} or {1} to type", obj1.ToString(), obj2.ToString(), "Boolean"), ex);
                }
                switch (operation)
                {
                    case ConditionOperator.Equal:
                        {
                            if (value1 == value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.NotEqual:
                        {
                            if (value1 != value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exceptions.InvalidOperationException(value1.GetType(), operation);
                        }
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("BooleanCompare obj1 {0} operator {1} obj2 {2} ,result {3} ", value1, operation.ToString(), value2, bResult));
            }
            return bResult;
        }
    }
    public class DateTimeCompare : CompareBase, ICompare
    {
        private NextLabsCRMLogs m_log = null;
        public DateTimeCompare(NextLabsCRMLogs log)
        {
            m_log = log;
        }
        public bool Compare(object obj1, ConditionOperator operation, object obj2)
        {
            bool bResult = false;
            if (obj1 == null || obj2 == null)
            {
                bResult = NULLCompare(obj1, operation, obj2);
            }
            else
            {
                DateTime value1, value2;
                try
                {
                    value1 = DateTime.Parse(obj1.ToString()).Date;
                    value2 = DateTime.Parse(obj2.ToString()).Date;
                }
                catch (FormatException ex)
                {
                    throw new FormatException(string.Format("Cannot parse {0} or {1} to type", obj1.ToString(), obj2.ToString(), "DateTimeCompare"), ex);
                }
                switch (operation)
                {
                    case ConditionOperator.On:
                        {
                            if (value1 == value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.OnOrAfter:
                        {
                            if (value1>= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    case ConditionOperator.OnOrBefore:
                        {
                            if (value1 <= value2)
                            {
                                bResult = true;
                            }
                        }
                        break;
                    default:
                        {
                            throw new Exceptions.InvalidOperationException(value1.GetType(), operation);
                        }
                }
                m_log.WriteLog(Enums.LogLevel.Debug, string.Format("DateTimeCompare obj1 {0} operator {1} obj2 {2} ,result {3} ", value1, operation.ToString(), value2, bResult));
            }
            return bResult;
        }
    }
}
