namespace ODBCBrowse
{
    using System;
    using System.Data;
    using System.Data.Odbc;
    using System.IO;
    using System.Windows.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                if ((args.Length != 5) && (args.Length != 0))
                {
                    showUsage();
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new formMain());
            }
            else
            {
                OdbcDataReader reader2;
                string dsn = args[0];
                string username = args[1];
                string password = args[2];
                string path = args[3];
                string str5 = args[4];
                OdbcConnectionManager manager = new OdbcConnectionManager(dsn, username, password);
                StreamReader reader = new StreamReader(path);
                OdbcCommand command = new OdbcCommand(reader.ReadLine(), manager.cnn);
                DataTable table = new DataTable();
                try
                {
                    reader2 = command.ExecuteReader();
                }
                catch (Exception exception)
                {
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Error executing command\n\n" + exception.Message);
                    return;
                }
                try
                {
                    table.Load(reader2);
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("Error: Out of memory", "NetTools: ODBCBrowse", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                StreamWriter writer = new StreamWriter(str5);
                string str7 = "";
                foreach (DataColumn column in table.Columns)
                {
                    writer.Write(str7);
                    if (column.ColumnName.Contains(",") ||
                        column.ColumnName.Contains("\"") || 
                        column.ColumnName.Contains("\n"))
                    {
                        writer.Write("\"");
                        writer.Write(column.ColumnName.Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " "));
                        writer.Write("\"");
                    }
                    else
                    {
                        writer.Write(column.ColumnName);
                    }
                    str7 = ",";
                }
                writer.WriteLine();
                ExportProgress progress = new ExportProgress();
                progress.updateProgressMin(1);
                progress.updateProgressMax(table.Rows.Count);
                progress.Show();
                int num = 1;
                foreach (DataRow row in table.Rows)
                {
                    str7 = "";
                    foreach (object obj2 in row.ItemArray)
                    {
                        writer.Write(str7);
                        if (obj2.ToString().Contains(",") ||
                            obj2.ToString().Contains("\"") ||
                            obj2.ToString().Contains("\n"))
                        {
                            writer.Write("\"");
                            writer.Write(obj2.ToString().Trim().Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " "));
                            writer.Write("\"");
                        } else {
                            writer.Write(obj2.ToString().Trim());
                        }
                        str7 = ",";
                    }
                    writer.WriteLine();
                    progress.updateProgressBar(num);
                    num++;
                }
                writer.Close();
            }
        }

        private static void showUsage()
        {
            MessageBox.Show(((((("Usage:" + Environment.NewLine) + "ODBCBrowse.exe <DSN> <username> <password> <sqlfile> <outputfile>" + Environment.NewLine) + Environment.NewLine + "Example: ODBCBrowse.exe \"SalesLink DSN\" \"\" \"\" \"sql command.txt\" \"transactions.csv\" ") + Environment.NewLine + Environment.NewLine) + "The above example connects to a DSN called \"SalesLink DSN\" with no username or password, ") + "then reads the SQL command to be executed from \"sql command.txt\" and outputs the resulting " + "table to \"transactions.csv\" in comma seperated format. ");
        }
    }
}

