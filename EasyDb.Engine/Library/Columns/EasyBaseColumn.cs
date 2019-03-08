/*
 * EasyDb,
 *   Kodlarý kullanmak ile ilgili tüm sorumluluk size aittir,
 *   Kodlarý satamaz ve üstünde hak iddia edemezsiniz,
 *   Ticari olsun olmasýn herhangi bir projede kullanabilirsiniz,
 *   EasyDb kullandýðýnýzý belirtmelisiniz(uygun yerlerde).
 *
 * Yücel Daðlar
 *
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public abstract class EasyBaseColumn
    {
        public UInt32 Id;
        public string Name;
        public bool IsIdentity = false;

        public SortedSet<EasyValue> Index;

        public abstract Type GetColumnType();

        public abstract void SetIdentity();
        public abstract object GetDefaultValue();

        public abstract object Insert(object value);

        public abstract object Update(int index, object value);

        public abstract void Delete(int index);

        public abstract object GetValue(int i);

        public abstract int GetRowCount();

        public abstract void Truncate(int rowcount);
        
        public abstract object ConvertTo(object value);

        internal abstract bool GTE(object source, object dest);
        internal abstract bool GT(object source, object dest);
        internal abstract bool LTE(object source, object dest);
        internal abstract bool LT(object source, object dest);
        internal abstract bool EQ(object source, object dest);
        internal abstract object PLUS(object source, object dest);
        internal abstract object MINUS(object source, object dest);
        internal abstract object DIV(object source, object dest);
        internal abstract object MUL(object source, object dest);
        internal abstract bool AND(object source, object dest);
        internal abstract bool OR(object source, object dest);

        public abstract object Custom(string msg, object value, CustomOperation operation);

        /// <summary>
        /// Boolean Index Array
        /// </summary>
        public static EasyBoolColumn BI(EasyBaseColumn column, object result, int index, object value, bool condition)
        {
            if (result == null) result = new EasyBoolColumn();
            EasyBoolColumn b = (EasyBoolColumn)result;

            if (condition)
                b.Values.Add(true);
            else
                b.Values.Add(false);

            return b;
        }

        /// <summary>
        /// Index Array
        /// </summary>
        public static EasyIntColumn IA(EasyBaseColumn column, object result, int index, object value, bool condition)
        {
            if (result == null) result = new EasyIntColumn();
            EasyIntColumn b = (EasyIntColumn)result;

            if (condition) b.Insert(index);

            return b;
        }

        public static EasyBaseColumn COLLECT(EasyBaseColumn column, object result, int index, object value, bool condition)
        {
            if (result == null && value != null)
            {
                if (value.GetType() == typeof(int))
                    result = new EasyIntColumn();
                else if (value.GetType() == typeof(string))
                    result = new EasyStringColumn();
                else if (value.GetType() == typeof(bool))
                    result = new EasyBoolColumn();
                else
                    throw new NotSupportedException();
            }
            if (result == null) return null;

            EasyBaseColumn b = (EasyBaseColumn)result;
            //if (condition) deðiþtirme!!!
                b.Insert(value);

            return b;
        }

        public static object SUM(EasyBaseColumn column, object result, int index, object value, bool condition)
        {
            if (result != null)
                return column.PLUS(result, value);
            else
                return value;
        }

        public static object COUNT(EasyBaseColumn column, object result, int index, object value, bool condition)
        {
            if (result != null && condition)
                return ((int)result) + 1;
            else if (condition)
                return 1;

            return null;
        }
    }

    [Serializable()]
    public abstract class EasyBaseColumnGeneric<T> : EasyBaseColumn
    {
        public List<T> Values = new List<T>();

        public override int GetRowCount()
        {
            return Values.Count;
        }

        public override Type GetColumnType()
        {
            return typeof(T);
        }

        public override object GetValue(int i)
        {
            return Values[i];
        }

        public override void Truncate(int rowcount)
        {
            Values.RemoveRange(rowcount, Values.Count - rowcount);
        }

        public override void SetIdentity()
        {
            //do nothing
        }

        public override object GetDefaultValue()
        {
            return default(T);
        }

        public override object Insert(object value)
        {
            if (value == null)
                value = GetDefaultValue();
            else if (!(value is T))
                value = Convert.ChangeType(value, typeof(T));
            
            Values.Add((T)value);
            if (Index != null)
                Index.Add(new EasyValue() { Column = this, Index = Values.Count - 1 });

            return value;
        }

        public override object Update(int index, object value)
        { 
            Values[index] = (T)value;
            return value;
        }

        public override void Delete(int index)
        {
            Values.RemoveAt(index);
        }

        public override object ConvertTo(object value)
        {
            return Convert.ChangeType(value, typeof(T));
        }

        public override object Custom(string msg, object value, CustomOperation operation)
        {
            object result = null;
            object v = value;
            if (!(value is EasyBaseColumn)) v = this.ConvertTo(value);
            int index = 0;
            bool b = true;
            
            //foreach (var item in Values)
            //{
            int length = Values.Count;
            for (int i = 0; i < length; i++)
			{
                object item = Values[i];

                if (value is EasyBaseColumn)     
                    v = ((EasyBaseColumn)value).GetValue(index);

                switch (msg)
                {
                    case ">":
                        b = GT(item, v);
                        result = operation(this, result, index, b, b);
                        break;

                    case ">=":
                        b = GTE(item, v);
                        result = operation(this, result, index, b, b);
                        break;

                    case "<":
                        b = LT(item, v);
                        result = operation(this, result, index, b, b);
                        break;

                    case "<=":
                        b = LTE(item, v);
                        result = operation(this, result, index, b, b);
                        break;

                    case "=":
                        b = EQ(item, v);
                        result = operation(this, result, index, b, b);
                        break;

                    case "+":
                        result = operation(this, result, index, PLUS(item, v), true);
                        break;

                    case "-":
                        result = operation(this, result, index, MINUS(item, v), true);
                        break;

                    case "/":
                        result = operation(this, result, index, DIV(item, v), true);
                        break;

                    case "*":
                        result = operation(this, result, index, MUL(item, v), true);
                        break;

                    case "\\":
                        result = operation(this, result, index, item, true);
                        break;

                    case "&":
                        b = AND(item, v);
                        result = operation(this, result, index, b, b);
                        break;

                    default:
                        throw new NotSupportedException();
                }
                index++;
            }
            return result;
        }
    }

    public delegate object CustomOperation(EasyBaseColumn column, object result, int index, object value, bool condition);

    public class EasyValue : IComparable<EasyValue>
    {
        public EasyBaseColumn Column;
        public int Index;

        public virtual object GetValue()
        {
            return Column.GetValue(Index);
        }

        #region IComparable<object> Members

        public int CompareTo(EasyValue other)
        {
            object source = GetValue();
            object value = ((EasyValue)other).GetValue();

            if (Column.GT(source, value))
                return 1;
            else if (Column.LT(source, value))
                return -1;
            else
                return 0;
        }

        #endregion
    }

    public class EasyConst : EasyValue
    {
        public object Value;

        public override object GetValue()
        {
            return Value;
        }
    }
}
