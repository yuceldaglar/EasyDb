using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.SqlParsing
{
    class DeleteStatement : StatementBase
    {
        public string TableName;
        public List<string> Where = new List<string>();
    }
}
