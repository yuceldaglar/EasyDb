using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb
{
    class ObjectContainer
    {
        public string Label { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Label;
        }
    }
}
