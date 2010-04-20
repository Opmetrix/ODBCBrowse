namespace ODBCBrowse
{
    using Microsoft.Win32;
    using System;
    using System.Collections;

    public class OdbcDataSourceManager
    {
        public SortedList GetAllDataSourceNames()
        {
            SortedList userDataSourceNames = this.GetUserDataSourceNames();
            SortedList systemDataSourceNames = this.GetSystemDataSourceNames();
            for (int i = 0; i < systemDataSourceNames.Count; i++)
            {
                string key = systemDataSourceNames.GetKey(i) as string;
                DataSourceType byIndex = (DataSourceType) systemDataSourceNames.GetByIndex(i);
                try
                {
                    userDataSourceNames.Add(key, byIndex);
                }
                catch
                {
                }
            }
            return userDataSourceNames;
        }

        public SortedList GetSystemDataSourceNames()
        {
            SortedList list = new SortedList();
            RegistryKey key = Registry.LocalMachine.OpenSubKey("Software");
            if (key != null)
            {
                key = key.OpenSubKey("ODBC");
                if (key == null)
                {
                    return list;
                }
                key = key.OpenSubKey("ODBC.INI");
                if (key == null)
                {
                    return list;
                }
                key = key.OpenSubKey("ODBC Data Sources");
                if (key != null)
                {
                    foreach (string str in key.GetValueNames())
                    {
                        list.Add(str, DataSourceType.System);
                    }
                }
                try
                {
                    key.Close();
                }
                catch
                {
                }
            }
            return list;
        }

        public SortedList GetUserDataSourceNames()
        {
            SortedList list = new SortedList();
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
            if (key != null)
            {
                key = key.OpenSubKey("ODBC");
                if (key == null)
                {
                    return list;
                }
                key = key.OpenSubKey("ODBC.INI");
                if (key == null)
                {
                    return list;
                }
                key = key.OpenSubKey("ODBC Data Sources");
                if (key != null)
                {
                    foreach (string str in key.GetValueNames())
                    {
                        list.Add(str, DataSourceType.User);
                    }
                }
                try
                {
                    key.Close();
                }
                catch
                {
                }
            }
            return list;
        }
    }
}

