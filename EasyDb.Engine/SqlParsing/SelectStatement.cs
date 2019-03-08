using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.SqlParsing
{
    public class StatementBase
    {
    }

    public class SelectStatement : StatementBase
    {
        public string TableName;
        public List<string> Fields = new List<string>();
        public List<string> Where = new List<string>();
        public List<string> GroupFields = new List<string>();
    }
}
