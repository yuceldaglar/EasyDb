using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public class EasyDecimalColumn : EasyBaseColumnGeneric<decimal>
    {
        internal override bool GTE(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x >= y;
        }

        internal override bool GT(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x > y;
        }

        internal override bool LTE(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x <= y;
        }

        internal override bool LT(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x < y;
        }

        internal override bool EQ(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x == y;
        }

        internal override object PLUS(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x + y;
        }

        internal override object MINUS(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x - y;
        }

        internal override object DIV(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x / y;
        }

        internal override object MUL(object source, object dest)
        {
            decimal x = (decimal)source;
            decimal y = (decimal)Convert.ChangeType(dest, typeof(decimal));

            return x * y;
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
