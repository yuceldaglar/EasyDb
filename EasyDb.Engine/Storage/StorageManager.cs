using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EasyDb.Library;

namespace EasyDb.Storage
{
    public class StorageManager
    {
        private const string RECOVERMSG = "Data is in inconsistent state. You can load with recover option to rescue available data.";
        private const uint TRANSTART = 998;
        private const uint TRANEND = 999;
        private const int PAGESIZE = 4096;
        private FileStream stream;
        private BinaryReader reader;

        public void OpenFile(string filename)
        {
            stream = File.Open(filename, FileMode.OpenOrCreate);
            reader = new BinaryReader(stream);
        }

        public void Load(EasyServer server)
        {
            Load(server, false, -1);
        }

        public void Load(EasyServer server, bool recover, long recoveryPoint)
        {            
            long filesize = stream.Length;
            long inTransaction = -1;
            long prevPos = 0;
            long oldSize = 0;
            long startPosition = 0;

            try
            {
                while (filesize > 0)
                {
                    startPosition = stream.Position;

                    uint tableId = reader.ReadUInt32();
                    EasyTable table = null;

                    #region Deleted row OR Transaction Atomicity
                    if (tableId == 0)
                    {
                        uint sz = reader.ReadUInt32();
                        filesize -= sz;
                        prevPos = 0;

                        reader.ReadBytes((int)sz - 8);
                        continue;
                    }
                    else if (tableId == TRANSTART)
                    {
                        if (recover && recoveryPoint > -1 && recoveryPoint == startPosition)
                        {
                            //actual recovery
                            //TODO: truncate file :) maybr seek -1
                            return;
                        }
                        if (inTransaction > -1)
                        {
                            if (!recover)
                                throw new Exception(RECOVERMSG);
                            else
                            {
                                throw new RecoverException();
                            }
                        }

                        inTransaction = startPosition;

                        continue;
                    }
                    else if (tableId == TRANEND)
                    {
                        if (inTransaction == -1)
                        {
                            if (!recover)
                                throw new Exception(RECOVERMSG);
                            else
                            {
                                throw new RecoverException();
                            }
                        }
                        else
                            inTransaction = -1; //everyting is ok.

                        continue;
                    }
                    #endregion

                    table = server.FindTable(tableId);

                    uint size = reader.ReadUInt32();
                    filesize -= size;

                    #region Row size control
                    if (prevPos != 0)
                    {
                        if (stream.Position - prevPos != oldSize)
                            throw new Exception("Corrupt Internal Error");
                    }
                    prevPos = stream.Position;
                    oldSize = size;
                    #endregion

                    table.RowColumn.Insert(stream.Position - 8);

                    #region Read Value
                    while ((stream.Position - startPosition) < size)
                    {
                        uint columnId = reader.ReadUInt32();
                        EasyBaseColumn column = table.FindColumn(columnId);
                        if (column is EasyUIntColumn)
                        {
                            column.Insert(reader.ReadUInt32());
                        }
                        else if (column is EasyIntColumn)
                        {
                            column.Insert(reader.ReadInt32());
                        }
                        else if (column is EasyStringColumn)
                        {
                            column.Insert(reader.ReadString());
                        }
                        else if (column is EasyDecimalColumn)
                        {
                            column.Insert(reader.ReadDecimal());
                        }
                        else if (column is EasyBoolColumn)
                        {
                            column.Insert(reader.ReadDecimal());
                        }
                    }
                    #endregion

                    #region if row does not include all columns, complete the others
                    /* Insert or update cannot gueranty to supply data for all columns.
                     * Because table can altered at any time. (Alter table add column)
                     */
                    int rowCount = table.RowColumn.GetRowCount();
                    foreach (EasyBaseColumn column in table.Columns)
                    {
                        if (column.GetRowCount() < rowCount) column.Insert(null);
                        column.SetIdentity(); 
                    }
                    #endregion

                    if (table.Id == 1) //'tables' table
                        server.CreateTableFromSchema();

                    //TODO: if this is a column and table is already created then add this new column
                }
            }
            catch (RecoverException rexp)
            {
                if (!recover) throw;

                this.CloseFile();
                Load(server, true, inTransaction);
            }
        }

        public long Append(uint tableId, uint[] columnId, object[] values)
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter w = new BinaryWriter(mem);
            
            uint size = 0;
            w.Write(tableId);
            w.Write(size);

            int count = columnId.Length;
            for (int i = 0; i < count; i++)
            {
                if (values[i] == null) continue;

                w.Write(columnId[i]);

                Type t = values[i].GetType();
                if (t == typeof(int))
                    w.Write((int)values[i]);
                else 
                    if (t == typeof(uint))
                        w.Write((uint)values[i]);
                else 
                    if (t == typeof(string))
                        w.Write((string)values[i]);
                else
                    if (t == typeof(bool))
                        w.Write((bool)values[i]);
                else
                    throw new NotSupportedException();
            }
            size = (uint)mem.Length;
            w.Seek(4, SeekOrigin.Begin);
            w.Write(size);

            long rowPos = stream.Position;
            stream.Write(mem.GetBuffer(), 0, (int)size);
            
            w.Close();
            mem.Close();

            return rowPos;
        }

        public void Delete(uint rowid)
        {
            long position = stream.Position;
            
            stream.Seek((long)rowid, SeekOrigin.Begin);
            stream.Write(new byte[]{0,0,0,0}, 0, 4);
            
            //TODO: Check performance of this operation.
            stream.Seek(position, SeekOrigin.Begin);
        }

        public void CloseFile()
        {
            try { stream.Close(); }
            catch { }
        }

        public void Flush()
        {
            stream.Flush();
        }

        public void WriteTransactionStartPoint()
        {
            stream.Write(BitConverter.GetBytes(TRANSTART), 0, 4); 
        }

        public void WriteTransactionEndPoint()
        {
            stream.Write(BitConverter.GetBytes(TRANEND), 0, 4);             
        }
    }
}
