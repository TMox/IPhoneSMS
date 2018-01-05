using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Diagnostics.Trace;

namespace iPhoneSMS
{
    public class Sqlitedb
    {
        string _file = null;
        SQLiteConnection _connection = null;

        public Sqlitedb(string filename)
        {
            _file = filename;
        }

        public class Table
        {
            public string Name { get; set; }
            public List<Dictionary<string, object>> Items { get; set; }
        }


        public Table GetTableData(string name)
        {
            if (!File.Exists(_file))
                throw new Exception("Fuck");
            _connection = new SQLiteConnection("Data Source=" + _file + ";Version=3;Read Only=True;FailIfMissing=True;");
            _connection.Open();
            DataTable table = GetTable(name);
            TraceInformation("{0} item{1}", table.Rows.Count, table.Rows.Count == 1 ? "" : "s");
            var foo = (from DataRow r in table.Rows select (from i in Enumerable.Range(0, table.Columns.Count) select (table.Columns[i].ColumnName, r[i])).ToDictionary(x => x.Item1, x => x.Item2)).ToList();
            _connection.Dispose();
            return new Table { Name = name, Items = foo };
        }

        public List<Table> GetTables()
        {
            if (!File.Exists(_file))
                throw new Exception("Fuck");
            _connection = new SQLiteConnection("Data Source=" + _file + ";Version=3;Read Only=True;FailIfMissing=True;");
            _connection.Open();
            var tables = GetTableList();
            if (tables == null || tables.Count == 0)
                return null;
            DataTable table;
            List<Table> retVal = new List<Table>();
            foreach (var item in tables)
            {
                TraceInformation("table: {0}", item);
                try
                {
                    table = GetTable(item);
                    TraceInformation("{0} item{1}", table.Rows.Count, table.Rows.Count == 1 ? "" : "s");
                    var foo = (from DataRow r in table.Rows select (from i in Enumerable.Range(0, table.Columns.Count) select (table.Columns[i].ColumnName, r[i])).ToDictionary(x => x.Item1, x => x.Item2)).ToList();
                    retVal.Add(new Table { Name = item, Items = foo });
                }
                catch { }
            }
            _connection.Dispose();
            return retVal;
        }

        List<string> GetTableList()
        {
            string s = "SELECT * from sqlite_master where type='table';";
            SQLiteDataAdapter adpt = new SQLiteDataAdapter(s, _connection);
            DataSet set = new DataSet();
            adpt.Fill(set);
            adpt.Dispose();
            List<string> ret = (from DataRow r in set.Tables[0].Rows select r["name"] as string).ToList();
            set.Dispose();
            return ret;
        }

        public DataTable Query(string sql)
        {
            _connection = new SQLiteConnection("Data Source=" + _file + ";Version=3;Read Only=True;FailIfMissing=True;");
            _connection.Open();
            SQLiteDataAdapter adtp = new SQLiteDataAdapter(sql, _connection);
            DataSet set = new DataSet();
            adtp.Fill(set);
            adtp.Dispose();
            set.Dispose();
            _connection.Dispose();
            return set.Tables[0];
        }

        DataTable GetTable(string name)
        {
            string s = string.Format("SELECT * from {0};", name);
            SQLiteDataAdapter adpt = new SQLiteDataAdapter(s, _connection);
            DataSet set = new DataSet();
            adpt.Fill(set);
            adpt.Dispose();
            set.Dispose();
            return set.Tables[0];
        }
    }
}
