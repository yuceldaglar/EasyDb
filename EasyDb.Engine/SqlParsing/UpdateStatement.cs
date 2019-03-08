using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.SqlParsing
{
    public class UpdateStatement : StatementBase
    {
        public string TableName;
        public List<KeyValuePair<string, object>> Values = new List<KeyValuePair<string, object>>();
        public List<string> Where = new List<string>();
    }
}
