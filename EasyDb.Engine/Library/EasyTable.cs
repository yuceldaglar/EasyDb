using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Xml.Serialization;
using EasyDb.Storage;

namespace EasyDb.Library
{
    public class EasyTable
    {
        public EasyServer Server;

        private Dictionary<long, EasyBaseColumn> columnHash = new Dictionary<long, EasyBaseColumn>();
        public UInt32 Id;
        public string Name;
        public EasyUIntColumn RowColumn = new EasyUIntColumn();
        public List<EasyBaseColumn> Columns = new List<EasyBaseColumn>();

        public void AddColumn(EasyBaseColumn col)
        {
            Columns.Add(col);
        }

        public EasyBaseColumn FindColumn(string columnname)
        {
            return FindColumn(columnname, true);
        }

        public EasyBaseColumn FindColumn(string columnname, bool throwException)
        {
            foreach (EasyBaseColumn column in Columns)
            {
                if (column.Name == columnname) return column;
            }
            if (throwException)
                throw new Exception("Column Not Found. " + columnname);
            else
                return null;
        }

        public EasyBaseColumn FindColumn(uint columnId)
        {
            return FindColumn(columnId, true);
        }

        public EasyBaseColumn FindColumn(uint columnId, bool throwException)
        {
            if(columnHash.ContainsKey(columnId))
                return columnHash[columnId];

            foreach (EasyBaseColumn column in Columns)
            {
                if (column.Id == columnId) return column;
            }            
            if (throwException)
                throw new Exception("Column Not Found. " + columnId);
            else
                return null;
        }

        public int GetRowCount()
        {
            return RowColumn.GetRowCount();
        }

        public void Truncate(int rowcount)
        {
            foreach (EasyBaseColumn column in this.Columns)
            {
                column.Truncate(rowcount);
            }
        }

        public void Insert(KeyValuePair<string, object>[] values, Dictionary<string, EasyTransaction> Transaction, int valueFrom)
        {
            bool noTransaction = !joinTransaction(Transaction);

            Hashtable valueTable = new Hashtable(values.Length);
            foreach (KeyValuePair<string, object> item in values)
            {
                valueTable[item.Key] = item.Value;                
            }

            object[] pValues = new object[Columns.Count];
            uint[] pIds = new uint[Columns.Count];
            int pIndex = 0;

            foreach (EasyBaseColumn column in this.Columns)
            {
                object value = null;
                if (valueTable.ContainsKey(column.Name))
                    value = valueTable[column.Name];
                else if(valueFrom>-1)
                    value = column.GetValue(valueFrom);

                pValues[pIndex] = column.Insert(value);
                pIds[pIndex] = column.Id;
                pIndex++;
            }

            Server.Storage_Append(this, Name, Id, pIds, pValues);
        }

        public void Update(int index, KeyValuePair<string, object>[] values, Dictionary<string, EasyTransaction> Transaction)
        {
            Delete(index, Transaction);

            Insert(values, Transaction, index);
        }

        public void Delete(int index, Dictionary<string, EasyTransaction> Transaction)
        {
            bool noTransaction = !joinTransaction(Transaction, index, RowColumn.Values[index]);

            uint rowid = RowColumn.Values[index];
            RowColumn.Values[index] = 0;

            Server.Storage_Delete(rowid);
        }

        private bool joinTransaction(Dictionary<string, EasyTransaction> Transaction)
        {
            return joinTransaction(Transaction, -1, 0);
        }

        private bool joinTransaction(Dictionary<string, EasyTransaction> Transaction, 
            int deletedIndex, uint deletedValue)
        {
            if (Transaction != null)
            {
                if (!Transaction.ContainsKey(this.Name))
                    Transaction.Add(this.Name, new EasyTransaction() { RowCount = this.GetRowCount() });

                if (deletedIndex > -1)
                {
                    Transaction[Name].DeletedRows.Add(deletedIndex);
                    Transaction[Name].DeletedValues.Add(deletedValue);
                }

                return true;
            }

            return false;
        }

        public void Undelete(int index, uint value)
        {
            RowColumn.Values[index] = value;
        }
    }
}