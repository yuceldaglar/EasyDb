using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public class EasyUIntColumn : EasyBaseColumnGeneric<UInt32>
    {
        internal override bool GT(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x > y;
        }

        internal override bool GTE(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x >= y;
        }
        internal override bool LT(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x < y;
        }
        internal override bool LTE(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x <= y;
        }
        internal override bool EQ(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x == y;
        }

        internal override object PLUS(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x + y;
        }

        internal override object MINUS(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x - y;
        }

        internal override object DIV(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x / y;
        }

        internal override object MUL(object source, object dest)
        {
            uint x = (uint)source;
            uint y = (uint)Convert.ChangeType(dest, typeof(uint));

            return x * y;
        }

        internal override bool AND(object source, object dest)
        {
            throw new NotSupportedException();
        }

        internal override bool OR(object source, object dest)
        {
            throw new NotSupportedException();
        }
    }
}
