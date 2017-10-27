using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Data.Common;

namespace PLCReader
{
    static class DBControl
    {
        public static SQLiteConnection conn;
        static string dbFileName = "pw.db3";

        public static void OpenConnection()
        {
            conn = new SQLiteConnection("Data Source=" + dbFileName + "; Version=3;");
            conn.Open();
        }

        public static void CloseConnection()
        {
            conn.Close();
            conn.Dispose();
        }

        public static DataSet Select(string strSQL)
        {
            DataSet ds = new DataSet();

            //conn = new SQLiteConnection("Data Source=" + dbFileName + "; Version=3;");
            
            //conn.Open();

            if (conn.State == ConnectionState.Open)
            {
                SQLiteCommand cmd = new SQLiteCommand(strSQL, conn);

                SQLiteDataAdapter ad = new SQLiteDataAdapter(cmd);
                ad.Fill(ds);

                cmd.Dispose();
                ad.Dispose();

                //conn.Close();
                //conn.Dispose();

                return ds;
            }

            //conn.Close();
            //conn.Dispose();

            return null;
        }

        public static void ExecuteQuery(string strSQL, Dictionary<int, object> p = null)
        {
            using (conn = new SQLiteConnection("Data Source=" + dbFileName + "; Version=3;"))
            {
                conn.Open();

                if (conn.State == ConnectionState.Open)
                {
                    SQLiteCommand cmd = new SQLiteCommand(conn);
                    cmd.CommandText = strSQL;

                    if (p != null)
                    {
                        foreach (KeyValuePair<int, object> item in p)
                        {
                            cmd.Parameters.AddWithValue("@"+item.Key, item.Value);
                        }
                    }

                    cmd.ExecuteNonQuery();

                }
            }
        }
    }
}
