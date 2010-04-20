namespace ODBCBrowse
{
    using System;
    using System.Data.Odbc;

    internal class OdbcConnectionManager
    {
        public OdbcConnection cnn;
        public bool connectionActive;
        public Exception currentException;

        public OdbcConnectionManager()
        {
            this.connectionActive = false;
        }

        public OdbcConnectionManager(string dsn, string username, string password)
        {
            this.connectionActive = false;
            this.connect(dsn, username, password);
        }

        public OdbcConnectResult connect(string dsn, string username, string password)
        {
            if (this.connectionActive)
            {
                this.disconnect();
            }
            string connectionString = null;
            connectionString = "DSN=" + dsn + ";UID=" + username + ";PWD=" + password + ";";
            this.cnn = new OdbcConnection(connectionString);
            try
            {
                this.cnn.Open();
                this.connectionActive = true;
                return OdbcConnectResult.OK;
            }
            catch (Exception exception)
            {
                this.currentException = exception;
                return OdbcConnectResult.FAIL;
            }
        }

        public void disconnect()
        {
            if (this.connectionActive)
            {
                this.cnn.Close();
            }
        }
    }
}

