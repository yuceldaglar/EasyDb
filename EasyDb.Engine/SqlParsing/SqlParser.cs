using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.SqlParsing
{
    class SqlParser
    {
        public static List<string> Tokenize(string sql)
        {
            List<string> tokens = new List<string>();
            tokens.Add(null);
            bool instr = false;
            foreach (var c in sql.ToCharArray())
            {
                if (c == '\'' && !instr)
                {
                    instr = true;
                    if (!string.IsNullOrEmpty(tokens[tokens.Count - 1])) tokens.Add(null);
                }
                else if (c == '\'')
                {
                    instr = false;
                    tokens.Add(null); //Mutlaka yeni ekle, böylece empty string olan bir token a izin veriyoruz str deðerleri için.
                }
                else if (!instr &&
                    (c == ' ' || c == ',' || c == '=' || c == '+' || c == '-' || c == '*' || c == '/'
                    || c == '(' || c == ')' || c == '<') || c == '>' || c == '&' || c == '|')
                {
                    if (!string.IsNullOrEmpty(tokens[tokens.Count - 1])) tokens.Add(null);
                    if (c == ' ') continue;
                    tokens[tokens.Count - 1] += c;
                    tokens.Add(null);
                }
                else if (!instr &&
                    (c == '\r' || c == '\n' || c == '\t'))
                {
                    if (!string.IsNullOrEmpty(tokens[tokens.Count - 1])) tokens.Add(null);
                }
                else
                    tokens[tokens.Count - 1] += c;
            }
            for (int index = 0; index < tokens.Count - 1; index++)
            {
                if ("<>=&|+-*/".Contains(tokens[index]) &&
                    "<>=&|+-*/".Contains(tokens[index + 1]))
                {
                    tokens[index] += tokens[index + 1];
                    tokens.RemoveAt(index + 1);
                }
            }
            if (string.IsNullOrEmpty(tokens[tokens.Count - 1])) tokens.RemoveAt(tokens.Count - 1);

            return tokens;
        }

        public static List<StatementBase> Parse(string sql)
        {
            List<string> tokens = Tokenize(sql);

            List<StatementBase> result = new List<StatementBase>();
            DeleteStatement delete = null;
            SelectStatement select = null;
            CreateTableStatement create = null;
            InsertStatement insert = null;
            UpdateStatement update = null;
            SimpleStatement simple = null;

            int i = 0;
            int state = 0;
            string prevToken = null;

            while (i < (tokens.Count))
            {
                if (i > 0) prevToken = tokens[i - 1];
                string token = tokens[i];

                switch (state)
                {
                    #region Start
                    case 0:
                        if (token == "select")
                        {
                            state = 1000;
                            select = new SelectStatement();
                            result.Add(select);
                        }
                        else if (token == "create")
                        {
                            state = 2000;
                            create = new CreateTableStatement();
                            result.Add(create);
                        }

                        else if (token == "insert")
                        {
                            state = 3000;
                            insert = new InsertStatement();
                            result.Add(insert);
                        }
                        else if (token == "delete")
                        {
                            state = 4000;
                            delete = new DeleteStatement();
                            result.Add(delete);
                        }
                        else if (token == "begin")
                        {
                            state = 5000;
                            simple = new SimpleStatement();
                            result.Add(simple);

                            simple.Statement = token;
                        }
                        else if (token == "commit")
                        {
                            //TODO: implement ';' state = ?;
                            simple = new SimpleStatement();
                            result.Add(simple);

                            simple.Statement = token;
                        }
                        else if (token == "rollback")
                        {
                            //TODO: implement ';' state = ?;
                            simple = new SimpleStatement();
                            result.Add(simple);

                            simple.Statement = token;
                        }
                        else if (token == "update")
                        {
                            state = 6000;
                            update = new UpdateStatement();
                            result.Add(update);
                        }
                        else
                            throw new Exception("Invalid Token:" + token);
                        break; 
                    #endregion

                    #region Select
                    case 1000:
                        //TODO: check token is identifier
                        if (token == ",")
                            select.Fields.Add(null);
                        else if (token != "from")
                        {
                            if (select.Fields.Count == 0) select.Fields.Add(null);
                            if (!string.IsNullOrEmpty(select.Fields[select.Fields.Count - 1]))
                                select.Fields[select.Fields.Count - 1] += " ";
                            select.Fields[select.Fields.Count - 1] += token;
                            state = 1000;
                        }
                        else
                            state = 1002;
                        break;

                    case 1002:
                        //TODO: check token is identifier
                        select.TableName = token;
                        state = 1003;
                        break;

                    case 1003:
                        if (token == "where")
                            state = 1004;
                        else if (token == ";")
                            state = 0;
                        else //else if 'order by', 'group by'...
                            throw new Exception("Invalid Token:" + token);
                        break;

                    case 1004:
                        if (token != ";")
                        {
                            select.Where.Add(token);
                            state = 1004;
                        }
                        else
                            state = 0;
                        break; 
                    #endregion

                    #region Create Table
                    case 2000:
                        if (token != "table") throw new Exception("Invalid Token:" + token);
                        state = 2001;
                        break;

                    case 2001:
                        create.TableName = token;
                        state = 2002;
                        break;

                    case 2002:
                        if (token != "(") throw new Exception("Invalid Token:" + token);
                        state = 2003;
                        break;

                    case 2003:
                        //TODO: check token is identifier
                        state = 2004;
                        break;

                    case 2004:
                        //TODO: check token is a c# primitive type name
                        Type t1 = Type.GetType("System." + token);
                        create.Fields.Add(new CreateField(prevToken, t1));
                        state = 2005;
                        break;

                    case 2005:
                        if (token == ")") //end of create statement
                            state = 2006;
                        else if (token == "IDENTITY")
                            create.Fields[create.Fields.Count - 1].IsIdentity = true; //state 2005
                        else if (token == ",")
                            state = 2003; //we hope there will be another column definition :)
                        else
                            throw new Exception("Invalid Token:" + token);
                            break;

                    case 2006:
                        if (token == ";")
                            state = 0;
                        else
                            throw new Exception("Invalid Token:" + token);
                        break;
                    #endregion                        

                    #region Insert Into
                    case 3000:
                        if (token != "into") throw new Exception("Invalid Token:" + token);
                        state = 3001;
                        break;

                    case 3001:
                        //TODO: check id
                        insert.TableName = token;
                        state = 3002;
                        break;

                    case 3002:
                        if (token != "(") throw new Exception("Invalid Token:" + token);
                        state = 3003;
                        break;

                    case 3003:
                        //TODO: check id
                        insert.Values.Add(new KeyValuePair<string, object>(token, null));
                        state = 3004;
                        break;

                    case 3004:
                        if (token != "=") throw new Exception("Invalid Token:" + token);
                        state = 3005;
                        break;

                    case 3005:
                        //TODO: check id or const
                        insert.Values[insert.Values.Count - 1] =
                            new KeyValuePair<string, object>(insert.Values[insert.Values.Count - 1].Key, token);
                        state = 3006;
                        break;

                    case 3006:
                        if (token == ",")
                            state = 3003;
                        else if (token == ")")
                            state = 3007;
                        else
                            throw new Exception("Invalid Token:" + token);
                        break;
                        
                    case 3007:
                        if (token == ";")
                            state = 0;
                        else
                            throw new Exception("Invalid Token:" + token);
                        break;
                    #endregion                       

                    #region Update
                    case 6000:
                        //TODO: check token is id
                        update.TableName = token;
                        state = 6001;
                        break; 

                    case 6001:
                        if (token != "set")
                            throw new Exception("Invalid Token: " + token);
                        state = 6002;
                        break;

                    case 6002:
                        //TODO: check id
                        update.Values.Add(new KeyValuePair<string, object>(token, null));
                        state = 6003;
                        break;

                    case 6003:
                        if (token != "=") throw new Exception("Invalid Token:" + token);
                        state = 6004;
                        break;

                    case 6004:
                        //TODO: check id or const
                        update.Values[update.Values.Count - 1] =
                            new KeyValuePair<string, object>(update.Values[update.Values.Count - 1].Key, token);
                        state = 6005;
                        break;
                    
                    case 6005:
                        if (token == ",")
                            state = 6002;
                        else if (token == "where")
                            state = 6006;
                        else if (token == ";")
                            state = 0;
                        else
                            throw new Exception("Invalid Token:" + token);
                        break;

                    case 6006:
                        if (token != ";")
                        {
                            update.Where.Add(token);
                            state = 6006;
                        }
                        else
                            state = 0;
                        break; 
                    #endregion

                    #region Delete
                    case 4000:
                        if (token == "from")
                            state = 4001;
                        else
                            throw new Exception("Invalid Token:" + token);
                        break;

                    case 4001:
                        //TODO: check token is identifier
                        delete.TableName = token;
                        state = 4002;
                        break;

                    case 4002:
                        if (token == "where")
                            state = 4003;
                        else if (token == ";")
                            state = 0;
                        else
                            throw new Exception("Invalid Token:" + token);
                        break;

                    case 4003:
                        if (token != ";")
                        {
                            delete.Where.Add(token);
                            state = 4003;
                        }
                        else
                            state = 0;
                        break;
                    #endregion

                    #region Begin Tran
                    case 5000:
                        if (token == "tran" || token == "transaction")
                        {
                            simple.Statement += "tran";
                            state = 5001;
                        }
                        else
                            throw new Exception("Unknown token: " + token);
                        break;

                    case 5001:
                        if (token == ";")
                            state = 0;
                        else
                            throw new Exception("Expected ';'. Invalid Token:" + token);
                        break;
                    #endregion

                    default:
                        break;
                }

                i++;
            }

            return result;
        }

        public static bool IsId(string token)
        {
            foreach (var c in token)
            {
                if (Char.IsLetterOrDigit(c)) continue;
                return false;
            }
            return true;
        }
    }
}
