using System;
using System.Collections.Generic;
using System.Text;

namespace EasyDb.Library
{
    public class EasyTransaction
    {
        public EasyTable table;

        public int RowCount { get; set; }

        public List<int> DeletedRows = new List<int>();
        public List<uint> DeletedValues = new List<uint>();
        public List<EasyDiskRow> DiskRows = new List<EasyDiskRow>();
    }

    public struct EasyDiskRow
    {
        public uint tableId;
        public uint[] columnId;
        public object[] values;
    }
}
