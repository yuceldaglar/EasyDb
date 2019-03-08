using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.SqlParsing
{
    public class InsertStatement : StatementBase
    {
        public string TableName;
        public List<KeyValuePair<string, object>> Values = new List<KeyValuePair<string, object>>();
    }
}
