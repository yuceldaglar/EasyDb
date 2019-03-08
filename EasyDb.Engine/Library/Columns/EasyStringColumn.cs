using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public class EasyStringColumn : EasyBaseColumnGeneric<string>
    {
        internal override bool GT(object source, object dest)
        {
            string x = (string)source;
            string y = (string)Convert.ChangeType(dest, typeof(string));

            int i = String.Compare(x, y);
            return i > 0;
        }

        internal override bool GTE(object source, object dest)
        {
            string x = (string)source;
            string y = (string)Convert.ChangeType(dest, typeof(string));

            int i = String.Compare(x, y);
            return i >= 0;
        }
        internal override bool LT(object source, object dest)
        {
            string x = (string)source;
            string y = (string)Convert.ChangeType(dest, typeof(string));

            int i = String.Compare(x, y);
            return i < 0;
        }
        internal override bool LTE(object source, object dest)
        {
            string x = (string)source;
            string y = (string)Convert.ChangeType(dest, typeof(string));

            int i = String.Compare(x, y);
            return i <= 0;
        }
        internal override bool EQ(object source, object dest)
        {
            string x = (string)source;
            string y = (string)dest;
            //string y = (string)Convert.ChangeType(dest, typeof(string));

            return x == y;
        }
        internal override object PLUS(object source, object dest)
        {
            string x = (string)source;
            string y = (string)Convert.ChangeType(dest, typeof(string));

            return x + y;
        }

        internal override object MINUS(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override object DIV(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override object MUL(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override bool AND(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override bool OR(object source, object dest)
        {
            throw new NotImplementedException();
        }
    }
}
