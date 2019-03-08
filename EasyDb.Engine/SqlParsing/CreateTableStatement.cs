using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.SqlParsing
{
    public class CreateTableStatement : StatementBase
    {
        public string TableName;
        public List<CreateField> Fields = new List<CreateField>();
    }

    public class CreateField
    {
        public string Name;
        public Type FieldType;
        public bool IsIdentity = false;

        public CreateField() { }

        public CreateField(string name, Type type)
        {
            Name = name;
            FieldType = type;
        }
    }
}
