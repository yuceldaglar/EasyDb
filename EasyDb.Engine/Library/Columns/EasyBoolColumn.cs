using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public class EasyBoolColumn : EasyBaseColumnGeneric<bool>
    {
        internal override bool AND(object source, object dest)
        {
            bool b1 = (bool)source;
            bool b2 = (bool)Convert.ChangeType(dest, typeof(bool));

            return b1 && b2;
        }

        internal override bool OR(object source, object dest)
        {
            bool b1 = (bool)source;
            bool b2 = (bool)Convert.ChangeType(dest, typeof(bool));

            return b1 || b2;
        }

        internal override bool GTE(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override bool GT(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override bool LTE(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override bool LT(object source, object dest)
        {
            throw new NotImplementedException();
        }

        internal override bool EQ(object source, object dest)
        {
            bool b1 = (bool)source;
            bool b2 = (bool)Convert.ChangeType(dest, typeof(bool));

            return b1 == b2;
        }

        internal override object PLUS(object source, object dest)
        {
            throw new NotImplementedException();
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
    }
}
