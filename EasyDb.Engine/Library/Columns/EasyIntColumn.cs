using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public class EasyIntColumn : EasyBaseColumnGeneric<int>
    {
        public int IdCounter = 0;

        public EasyIntColumn()
        {
            Index = new SortedSet<EasyValue>();
        }

        public override void SetIdentity()
        {
            if (!IsIdentity) return;

            int i = Values[Values.Count - 1];
            if (i > IdCounter)
                IdCounter = i;
        }

        public override object GetDefaultValue()
        {
            if (IsIdentity)
            {
                IdCounter++;
                return IdCounter;
            }
            else
                return 0;
        }

        internal override bool GT(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x > y;
        }

        internal override bool GTE(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x >= y;
        }
        internal override bool LT(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x < y;
        }
        internal override bool LTE(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x <= y;
        }
        internal override bool EQ(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x == y;
        }

        internal override object PLUS(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x + y;
        }

        internal override object MINUS(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x - y;
        }
        internal override object DIV(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

            return x / y;
        }
        internal override object MUL(object source, object dest)
        {
            int x = (int)source;
            int y = (int)Convert.ChangeType(dest, typeof(int));

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
