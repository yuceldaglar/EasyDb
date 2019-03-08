using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using EasyDb.SqlParsing;
using System.Data;
using System.Xml.Serialization;
using EasyDb.Storage;

namespace EasyDb.Library
{
    public class EasyServer
    {
        public Dictionary<string, EasyTransaction> Transaction { get; set; }

        private Dictionary<long, EasyTable> tableHash = new Dictionary<long, EasyTable>();
        private UInt32 tableId = 1000;  // 0..999 reserved for system.
        private UInt32 columnId = 1000; // 0..999 reserved for system.

        public EasyServer()
        {
            CreateSystemTables();
        }

        public StorageManager Storage;

        public void CreateSystemTables()
        {
            #region Tables
            EasyTable tables =
                CreateTableStructure(
                "tables", 
                new List<CreateField>(){ 
                    new CreateField("table_catalog", typeof(string)),
                    new CreateField("table_schema", typeof(string)),
                    new CreateField("table_name", typeof(string)),
                    new CreateField("table_type", typeof(string)),
                },
                1, new uint[]{1,2,3,4});
            #endregion

            #region Columns
            EasyTable columns =
                CreateTableStructure(
                "columns",
                new List<CreateField>(){ 
                    new CreateField("table_catalog", typeof(string)),
                    new CreateField("table_schema", typeof(string)),
                    new CreateField("table_name", typeof(string)),
                    new CreateField("column_name", typeof(string)),
                    new CreateField("ordinal_position", typeof(string)),
                    new CreateField("column_default", typeof(string)),
                    new CreateField("is_nullable", typeof(string)),
                    new CreateField("data_type", typeof(string)),
                    new CreateField("is_identity", typeof(bool)),
                },
                2, new uint[] { 5, 6, 7, 8, 9, 10, 11, 12, 13 });
            #endregion
        }

        public List<EasyTable> Tables = new List<EasyTable>();

        public EasyTable CreateTableStructure(string tablename, List<CreateField> columns, uint useTableId, uint[] useColumnId)
        {
            if (useTableId == 0)
            {
                tableId++;
                useTableId = tableId;
            }

            EasyTable table = new EasyTable() { Id = useTableId, Server = this };

            table.Name = tablename;
            int i = 0;
            foreach (CreateField column in columns)
            {
                uint id = 0;
                if (useColumnId == null)
                {
                    columnId++;
                    id = columnId;
                }
                else
                    id = useColumnId[i];
                i++;

                EasyBaseColumn col = null;
                if (column.FieldType == typeof(string))
                    col = new EasyStringColumn() { Id = id };
                
                else if (column.FieldType == typeof(int))
                    col = new EasyIntColumn() { Id = id };
                
                else if (column.FieldType == typeof(uint))
                    col = new EasyUIntColumn() { Id = id };
                
                else if (column.FieldType == typeof(bool))
                    col = new EasyBoolColumn() { Id = id };
                
                else if (column.FieldType == typeof(decimal))
                    col = new EasyDecimalColumn() { Id = id };
                else
                    throw new Exception("Unknown column type! " + column.FieldType);

                col.Name = column.Name;
                col.IsIdentity = column.IsIdentity;
                table.AddColumn(col);
            }

            Tables.Add(table);

            return table;
        }

        public EasyTable CreateTable(string tablename, List<CreateField> columns)
        {
            EasyTable table = CreateTableStructure(tablename, columns, 0, null);
            EasyTable columnsTable = FindTable("columns");
            EasyTable tablesTable = FindTable("tables");

            foreach (CreateField column in columns)
            {
                Insert(columnsTable, new KeyValuePair<string, object>[]{
                    new KeyValuePair<string,object>("table_name", tablename),
                    new KeyValuePair<string,object>("column_name", column.Name),
                    new KeyValuePair<string,object>("is_identity", column.IsIdentity),
                    new KeyValuePair<string,object>("data_type", column.FieldType.Name)});
            }

            Insert(tablesTable, new KeyValuePair<string, object>[]{
                new KeyValuePair<string,object>("table_name", tablename)});

            return table;
        }

        public void BeginTransaction()
        {
            Transaction = new Dictionary<string, EasyTransaction>();
        }

        public void Commit()
        {
            if (Transaction == null) return;

            if (Storage != null)
            {
                Storage.WriteTransactionStartPoint();
                foreach (var t in Transaction.Values)
                {
                    foreach (var r in t.DiskRows)
                    {
                        uint rowid = (uint)Storage.Append(r.tableId, r.columnId, r.values);
                        
                    }
                    foreach (var d in t.DeletedRows)
                    {
                        Storage.Delete((uint)d);
                    }
                }
                Storage.WriteTransactionEndPoint();
                Storage.Flush();
            }

            Transaction = null;
        }

        public void Rollback()
        {
            foreach (string tablename in Transaction.Keys)
            {
                EasyTable table = FindTable(tablename);
                EasyTransaction tran = Transaction[tablename];

                table.Truncate(tran.RowCount);
                for (int i = 0; i < tran.DeletedRows.Count; i++)
                {
                    table.Undelete(tran.DeletedRows[i], tran.DeletedValues[i]);
                }
            }
            Transaction = null;
        }

        public int Insert(EasyTable table, KeyValuePair<string, object>[] values)
        {
            table.Insert(values, Transaction, -1); 

            return 1;
        }

        public void Update(int index, EasyTable table, KeyValuePair<string, object>[] values)
        {
            table.Update(index, values, Transaction);
        }

        public void Update(EasyTable table, EasyBaseColumn condition,  KeyValuePair<string, object>[] values)
        {
            if (condition is EasyBoolColumn)
            {
                EasyBoolColumn b = (EasyBoolColumn)condition;
                int l = b.GetRowCount();
                for (int i = 0; i < l; i++)
                {
                    if (b.Values[i])
                        table.Update(i, values, Transaction);
                }
            }
            else if (condition is EasyIntColumn)
            {
                EasyIntColumn iCol = (EasyIntColumn)condition;
                int l = iCol.GetRowCount();
                for (int i = 0; i < l; i++)
                {
                    table.Update(iCol.Values[i], values, Transaction);
                }
            }
            else
                throw new NotSupportedException();
        }
        public void Delete(EasyTable table, EasyBaseColumn condition)
        {           
            if (condition is EasyBoolColumn)
            {
                EasyBoolColumn b = (EasyBoolColumn)condition;
                int l = b.GetRowCount();
                for (int i = 0; i < l; i++)
                {
                    if (b.Values[i])
                        table.Delete(i, Transaction);
                }
            }
            else if (condition is EasyIntColumn)
            {
                EasyIntColumn iCol = (EasyIntColumn)condition;
                int l = iCol.GetRowCount();
                for (int i = 0; i < l; i++)
                {
                    table.Delete(iCol.Values[i], Transaction);
                }
            }
            else
                throw new NotSupportedException();
        }

        public DataTable Select(string tablename, List<string> fields, List<string> groupFields, WhereDelegate where)
        {
            if (groupFields != null && groupFields.Count == 0) groupFields = null;

            EasyTable table = FindTable(tablename);

            EasyBoolColumn rows = null;
            if (where != null)
            {
                rows = where();
                //rows = (EasyBoolColumn)rows.Custom("&", table.RowColumn, EasyBaseColumn.COLLECT);
            }

            DataTable dt = new DataTable();
            //List<EasyBaseColumn> selectedFields = new List<EasyBaseColumn>();
            Dictionary<string, EasyBaseColumn> selectedFields = new Dictionary<string,EasyBaseColumn>();
            foreach (string field in fields)
            {
                if (field != "*")
                {
                    EasyBaseColumn column = null;
                    if (SqlParser.IsId(field))
                    {
                        column = table.FindColumn(field);
                        //selectedFields.Add(column);
                        selectedFields[field] = column;
                        dt.Columns.Add(field, column.GetColumnType());
                    }
                    else
                    {
                        List<string> t = SqlParser.Tokenize(field);
                        column = (EasyBaseColumn)ExecuteExpression(table, t);
                        
                        //selectedFields.Add(column);
                        selectedFields[field] = column;
                        dt.Columns.Add(field, column.GetColumnType());
                    }
                }
                else
                {
                    foreach (var column in table.Columns)
                    {
                        //selectedFields.Add(column);
                        selectedFields[column.Name] = column;
                        dt.Columns.Add(column.Name, column.GetColumnType());
                    }
                }
            }

            //if (rows != null)
           // {
                int k = 0;
                int rowCount = 0;
                foreach (EasyBaseColumn column in selectedFields.Values)
                {
                    rowCount = column.GetRowCount();
                    break;
                }
                Hashtable previousValues = new Hashtable();
                for (int i = 0; i < rowCount; i++)
                {
                    if (rows != null && !rows.Values[i]) continue;
                    if (table.RowColumn.Values[i] == 0) continue;

                    int j = 0;

                    bool group = false;
                    if (groupFields != null)
                    {
                        group = true;
                        foreach (string groupField in groupFields)
                        {
                            if (k > 0)
                            {
                                object val = selectedFields[groupField].GetValue(i);
                                if (!selectedFields[groupField].EQ(val, previousValues[groupField]))
                                {
                                    group = false;
                                    previousValues[groupField] = val;
                                }
                            }
                            else
                            {
                                group = false;
                                previousValues[groupField] = selectedFields[groupField].GetValue(i);
                            }
                        }
                    }
                    if (group) continue;

                    dt.Rows.Add();
                    foreach (EasyBaseColumn column in selectedFields.Values)
                    {
                        dt.Rows[k][j] = column.GetValue(i);
                        j++;
                    }
                    k++;
                }
            //}
            //else
            //{
            //    int k = 0;
            //    int rowCount = 0;
            //    foreach (EasyBaseColumn column in selectedFields.Values)
            //    {
            //        rowCount = column.GetRowCount();
            //        break;
            //    }
            //    for (int i = 0; i < rowCount; i++)
            //    {
            //        if (table.RowColumn.Values[i] == 0) continue;

            //        dt.Rows.Add();
            //        int j = 0;
            //        foreach (EasyBaseColumn column in selectedFields.Values)
            //        {
            //            dt.Rows[k][j] = column.GetValue(i);
            //            j++;
            //        }
            //        k++;
            //    }
            //}

            return dt;
        }

        public List<object> ExecuteSql(string sql)
        {
            List<object> result = new List<object>();

            try
            {
                List<StatementBase> statements = SqlParser.Parse(sql);

                foreach (StatementBase statement in statements)
                {
                    result.Add(ExecuteStatement(statement));
                }

                return result;
            }
            catch
            {
                if (Transaction != null) Rollback();
                throw;
            }
        }

        public object ExecuteStatement(StatementBase statement)
        {
            if (statement is SelectStatement)
            {
                SelectStatement select = (SelectStatement)statement;

                if (select.Where.Count == 0)
                    return Select(select.TableName, select.Fields, select.GroupFields, null);
                else
                    return Select(select.TableName, select.Fields, select.GroupFields,
                        delegate()
                        {
                            return (EasyBoolColumn)ExecuteExpression(FindTable(select.TableName), select.Where);
                        });
            }
            else if (statement is InsertStatement)
            {
                InsertStatement insert = (InsertStatement)statement;
                return Insert(
                    FindTable(insert.TableName),
                    insert.Values.ToArray());
            }
            else if (statement is UpdateStatement)
            {
                UpdateStatement update = (UpdateStatement)statement;
                EasyTable table = FindTable(update.TableName);
                EasyBaseColumn condition = (EasyBaseColumn)ExecuteExpression(table, update.Where);
                Update(
                    table,
                    condition,
                    update.Values.ToArray());
                return "Success";
            }
            else if (statement is CreateTableStatement)
            {
                CreateTableStatement c = (CreateTableStatement)statement;
                CreateTable(c.TableName, c.Fields);
                return "Success";
            }
            else if (statement is DeleteStatement)
            {
                DeleteStatement d = (DeleteStatement)statement;
                EasyTable table = FindTable(d.TableName);
                EasyBaseColumn condition = (EasyBaseColumn)ExecuteExpression(table, d.Where);
                Delete(table, condition);
                return "Success";
            }
            else if (statement is SimpleStatement)
            {
                SimpleStatement simple = (SimpleStatement)statement;
                switch (simple.Statement)
                {
                    case "begintran":
                        BeginTransaction();
                        break;
                    case "commit":
                        Commit();
                        break;
                    case "rollback":
                        Rollback();
                        break;
                    default:
                        break;
                }
                return "Success";
            }
            throw new NotSupportedException();
        }

        private object ExecuteExpression(EasyTable table, List<string> list)
        {
            Stack<object> stack = new Stack<object>();

            #region Postfix Notation
            //TODO: apply paranthesis hack
            for (int i = list.Count - 2; i > -1; i--)
            {
                if (list[i] == "|" || list[i] == "&" || list[i] == "=" 
                    || list[i] == "<" || list[i] == ">"
                    || list[i] == "<=" || list[i] == ">="
                    || list[i] == "*" || list[i] == "/"
                    || list[i] == "+" || list[i] == "-")
                {
                    int k = 0;
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (k == 0 && list[j] != "(")
                        {
                            string s = list[j];
                            list[j] = list[i];
                            list[i] = s;
                            break;
                        }
                        else //find matching parantheses
                        {
                            if (list[j] == "(") k++; else if (list[j] == ")") k--;
                            if (k == 0)
                            {
                                string s = list[i];
                                for (int x = i; x < j; x++)
                                {
                                    list[x] = list[x + 1];
                                }
                                list[j] = s;
                                break;
                            }
                        }
                    }
                }
            } 
            #endregion

            foreach (var item in list)
            {
                if (item == "(" || item == ")") continue;

                if (item == "&" || item == "|" ||
                    item == ">" || item == ">=" || item == "<" || item == "<=" ||
                    item == "+" || item == "-" || item == "*" || item == "/" || 
                    item == "=" || item == "!=")
                {
                    string msg = item;
                    object p2 = stack.Pop();
                    object p1 = stack.Pop();
                    //TODO: if p1 isConst swap p1,p2; msg = reverse(msg)

                    EasyBaseColumn column = null;
                    if (p1 is EasyBaseColumn)
                        column = (EasyBaseColumn)p1;
                    else
                    {
                        column = table.FindColumn((string)p1, true);
                        //TODO: if p1 and p2, are both id then use a special column to execute
                        //TODO: if(p2 is id) p2 = findCol(p2)
                        //object result = column.Message(msg, p2);
                    }
                    object result = null;

                    //if (msg != "=" || column.Index == null)
                        result = column.Custom(msg, p2, EasyBaseColumn.COLLECT);
                    //else
                    //{
                    //    result = column.Index.Contains(new EasyConst() { Value = p2 });
                    //    EasyBoolColumn b = new EasyBoolColumn();
                    //    for (int i = 0; i < column.GetRowCount(); i++)
                    //    {
                    //        b.Insert(false);
                    //    }
                    //    //if (result != null) b.Values[1] = true;
                    //    if (result != null) b.Values[((EasyValue)result).Index] = true;
                    //    result = b;
                    //}

                    stack.Push(result);

                    continue;
                }
                stack.Push(item);
            }

            //TODO: stack can not contain more than one value
            return stack.Pop();
        }

        public EasyTable FindTable(string tablename)
        {
            foreach (EasyTable table in Tables)
            {
                if (table.Name == tablename) return table;
            }
            throw new Exception("Table Not Found. " + tablename);
        }

        public EasyTable FindTable(uint tableId)
        {
            if (tableHash.ContainsKey(tableId))
                return tableHash[tableId];

            foreach (EasyTable table in Tables)
            {
                if (table.Id == tableId)
                {
                    tableHash[tableId] = table;
                    return table;
                }
            }
            throw new Exception("Table Not Found. " + tableId);
        }

        public EasyBaseColumn FindTableColumn(string tablename, string columnname)
        {
            foreach (EasyTable table in Tables)
            {
                if (table.Name == tablename) return table.FindColumn(columnname);
            }
            throw new Exception("Table Not Found. " + tablename);
        }

        public void CreateTableFromSchema()
        {
            EasyTable tables = FindTable(1);
            EasyTable columnsTable = FindTable(2);
            EasyBaseColumn column = tables.FindColumn("table_name");
            int index = column.GetRowCount() - 1;

            string tablename = (string)tables.FindColumn("table_name").GetValue(index);

            EasyIntColumn inx = (EasyIntColumn)
                columnsTable.FindColumn("table_name").Custom("=", tablename, EasyBaseColumn.IA);

            List<CreateField> columns = new List<CreateField>(); 
            for (int i = 0; i < inx.GetRowCount(); i++)
            {
                int j = (int)inx.GetValue(i);
                CreateField field = new CreateField();
                field.Name = (string)columnsTable.FindColumn("column_name").GetValue(j);
                field.IsIdentity = (bool)columnsTable.FindColumn("is_identity").GetValue(j);
                string data = (string)columnsTable.FindColumn("data_type").GetValue(j);
                field.FieldType = Type.GetType("System." + data);

                columns.Add(field);
            }

            CreateTableStructure(tablename, columns, 0 ,null);
        }

        public void Close()
        {
            if (Storage != null) Storage.CloseFile();
        }

        public void Attach(string filename)
        {
            Storage = new StorageManager();
            Storage.OpenFile(filename);
            Storage.Load(this);               
        }

        public void Storage_Append(EasyTable table, string tablename, uint tableId, uint[] columnId, object[] values)
        {
            if (Transaction == null)
            {
                if (Storage != null)
                {
                    uint rowid = (uint)Storage.Append(tableId, columnId, values);
                    table.RowColumn.Insert(rowid);
                }
                else
                    table.RowColumn.Insert((uint)1);
            }
            else
            {
                EasyTransaction t = Transaction[tablename];
                if (t == null)
                {
                    t = new EasyTransaction();
                    t.table = table;
                    Transaction.Add(tablename, t);
                }

                EasyDiskRow r = new EasyDiskRow();
                r.tableId = tableId;
                r.columnId = columnId;
                r.values = values;
                t.DiskRows.Add(r);
            }
        }

        public void Storage_Delete(uint rowid)
        {
            if (Transaction == null)
            {
                if (Storage != null) Storage.Delete(rowid);
            }
            else
            {
                //do nothing.
            }
        }
    }

    public delegate EasyBoolColumn WhereDelegate();
}
