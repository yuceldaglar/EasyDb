using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EasyDb.Library;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using EasyDb.Storage;
using EasyDb.SqlParsing;

namespace EasyDb
{
    public partial class MainForm : Form
    {
        EasyServer Server;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            List<object> results = Server.ExecuteSql(txtSql.Text);
            w.Stop();
            labelStatus.Text = "Elapsed : " + w.ElapsedMilliseconds;

            comResults.Items.Clear();
            foreach (var item in results)
                comResults.Items.Add(new ObjectContainer() { Label = "Result(" + item.ToString() + ")", Value = item }); 
            if (comResults.Items.Count > 0) comResults.SelectedIndex = 0;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Server != null)
                Server.Close();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string sql = "select * from tables";
            DataTable t = ExecuteSql(sql);

            treeDb.Nodes[0].Nodes.Clear();
            foreach (DataRow row in t.Rows)
            {
                TreeNode node = treeDb.Nodes[0].Nodes.Add((string)row["table_name"]);
                node.ContextMenuStrip = cmenuTable;
                node.ImageIndex = 1;
                node.SelectedImageIndex = 1;
            }
        }

        private DataTable ExecuteSql(string sql)
        {
            List<object> result = Server.ExecuteSql(sql);
            return (DataTable)result[0];
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string fileLabel = Path.GetFileNameWithoutExtension(dialog.FileName);
            treeDb.Nodes[0].Text = "Database (" + fileLabel + ")";

            if (Server != null) Server.Close();
            Server = new EasyServer();
            Server.Attach(dialog.FileName);

            refreshToolStripMenuItem_Click(null, EventArgs.Empty);

            w.Stop();
            labelStatus.Text = "Elapsed : " + w.ElapsedMilliseconds;
        }

        private void selectFromToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeDb.SelectedNode;
            if (node == null) return;

            txtSql.Text = "select * from " + node.Text;
        }

        private void scriptCreateTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtSql.Text = "create table newtable (column1 Int32, column2 String)";
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Server != null)
            {
                Server.Close();
                Server = null;
                treeDb.Nodes[0].Text = "Database";
                treeDb.Nodes[0].Nodes.Clear();
            }            
        }

        private void newDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() != DialogResult.OK) return;

            string fileLabel = Path.GetFileNameWithoutExtension(dialog.FileName);
            treeDb.Nodes[0].Text = "Database (" + fileLabel + ")";

            if (Server != null) Server.Close();
            Server = new EasyServer();
            if (fileLabel != "memory")
                Server.Attach(dialog.FileName);
        }

        private void comResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView2.DataSource = ((ObjectContainer)comResults.SelectedItem).Value;
        }

        private void btntest_Click(object sender, EventArgs e)
        {
            SelectStatement select = new SelectStatement();
            //select.TableName = "customer";
            //select.Fields.Add("id");
            //select.Fields.Add("name");
            //select.Fields.Add("city");
            //select.Fields.Add("age");
            //select.GroupFields.Add("city");
            select.TableName = "t";
            select.Fields.Add("a");
            select.Fields.Add("b");
            select.Fields.Add("c");
            select.GroupFields.Add("c");
            

            Stopwatch w = new Stopwatch();
            w.Start();
            List<object> results = new List<object>();
            results.Add(Server.ExecuteStatement(select));
            w.Stop();
            labelStatus.Text = "Elapsed : " + w.ElapsedMilliseconds;

            comResults.Items.Clear();
            foreach (var item in results)
                comResults.Items.Add(new ObjectContainer() { Label = "Result(" + item.ToString() + ")", Value = item });
            if (comResults.Items.Count > 0) comResults.SelectedIndex = 0;
        }
    }
}

#region some old code
/*
         private void btnTest1_Click(object sender, EventArgs e)
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            int j = Server.FindTable("telefon").FindColumn("yaþ").GetRowCount();
            int i = 0;
            while (w.ElapsedMilliseconds<1000)
            {
                
                Server.Insert(
                    Server.FindTable("telefon"),
                    new KeyValuePair<string, object>[]{ 
                                        new KeyValuePair<string,object>("adsoyad", "A " + i),
                                        new KeyValuePair<string,object>("telno", "210 10 " + i),
                                        new KeyValuePair<string,object>("yaþ", j + i)
                                    }
                );
                i++;
            }
            w.Stop();
            MessageBox.Show("1 Saniyede insert sayýsý: " + i);
        }

        private void btnSum_Click(object sender, EventArgs e)
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            int i = (int)Server.FindTableColumn("telefon", "yaþ").
                Custom("\\", null, EasyBaseColumn.SUM);
            w.Stop();
            MessageBox.Show("Sum: " + i + ", Elapsed ms: " + w.ElapsedMilliseconds);

            MessageBox.Show("Count: " +
                Server.FindTableColumn("telefon", "yaþ").
                Custom("\\", null, EasyBaseColumn.COUNT));

            MessageBox.Show("Count >5: " +
                Server.FindTableColumn("telefon", "yaþ").
                Custom(">", 5, EasyBaseColumn.COUNT));
        }
 */
#endregion