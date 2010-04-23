namespace ODBCBrowse
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Odbc;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    public class formMain : Form
    {
        private Button btConnect;
        private Button btDisconnect;
        private Button buttonExec;
        private Button buttonRefreshDSN;
        private ComboBox cbxDSNList;
        private IContainer components;
        private OdbcConnectionManager connection;
        private DataTable dataTable;
        private ToolStripMenuItem dataToolStripMenuItem;
        private DataGridView dgridMain;
        private SortedList dsnList;
        private OdbcDataSourceManager dsnManager;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem exportToCSVToolStripMenuItem;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem editToolStripMenuItem;
        private ToolStripMenuItem selectAllToolStripMenuItem;
        private ToolStripMenuItem selectNoneToolStripMenuItem;
        private FlowLayoutPanel flowLayoutPanel1;
        private FlowLayoutPanel flowLayoutPanel2;
        private Label label1;
        private Label label2;
        private Label label3;
        private ToolStripStatusLabel lblStatus;
        private MenuStrip menuStripMain;
        private SplitContainer splitContainer1;
        private StatusStrip statusStripMain;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TextBox tbPassword;
        private TextBox tbSQLQuery;
        private TextBox tbUsername;
        private TreeView tvTables;

        public formMain()
        {
            this.InitializeComponent();
            this.dsnManager = new OdbcDataSourceManager();
            this.dsnList = this.dsnManager.GetAllDataSourceNames();
            this.populateDSNDropdown();
            this.connection = new OdbcConnectionManager();
            this.dataTable = new DataTable();
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            if (this.cbxDSNList.SelectedItem == null)
            {
                MessageBox.Show("Error, you must select a DSN from the list", "NetTools: ODBCBrowse", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                Cursor.Current = Cursors.WaitCursor;
                string[] strArray = this.cbxDSNList.SelectedItem.ToString().Split(new char[] { ':' });
                string dsn = "";
                for (int i = 1; i <= (strArray.Length - 1); i++)
                {
                    dsn = dsn + strArray[i];
                }
                this.lblStatus.Text = "Connecting to DSN: " + dsn;
                if (this.connection.connect(dsn, this.tbUsername.Text, this.tbPassword.Text) != OdbcConnectResult.OK)
                {
                    if (this.connection.currentException != null)
                    {
                        MessageBox.Show("Error: Unable to connect to DSN: " + dsn + "\n\n" + this.connection.currentException.Message, "NetTools: ODBCBrowse");
                        this.lblStatus.Text = "Disconnected";
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        MessageBox.Show("Error: Unable to connect to DSN: " + dsn + " Unspecified error", "NetTools: ODBCBrowse");
                        this.lblStatus.Text = "Disconnected";
                        Cursor.Current = Cursors.Default;
                    }
                }
                else
                {
                    this.lblStatus.Text = "Connect to DSN '" + dsn + "' OK.";
                    this.tvTables.Nodes.Clear();
                    this.tvTables.Nodes.Add("Database");
                    foreach (DataRow row in this.connection.cnn.GetSchema("tables").Rows)
                    {
                        this.tvTables.Nodes[0].Nodes.Add(row.ItemArray[2].ToString());
                    }
                    this.tvTables.ExpandAll();
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void btDisconnect_Click(object sender, EventArgs e)
        {
            this.connection.disconnect();
            this.tvTables.Nodes.Clear();
            this.tbSQLQuery.Text = "";
            this.tbUsername.Text = "";
            this.tbPassword.Text = "";
            DataTable table = new DataTable();
            this.dgridMain.DataSource = table;
            this.cbxDSNList.SelectedItem = null;
            this.lblStatus.Text = "Disconnected";
        }

        private void buttonExec_Click(object sender, EventArgs e)
        {
            if (!this.connection.connectionActive)
            {
                MessageBox.Show("You must connect to a dsn first", "NetTools: ODBCBrowse", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                OdbcDataReader reader;
                Cursor.Current = Cursors.WaitCursor;
                OdbcCommand command = new OdbcCommand(this.tbSQLQuery.Text, this.connection.cnn);
                this.dataTable = new DataTable();
                try
                {
                    reader = command.ExecuteReader();
                }
                catch (Exception exception)
                {
                    Cursor.Current = Cursors.Default;
                    MessageBox.Show("Error executing command\n\n" + exception.Message);
                    return;
                }
                try
                {
                    this.dataTable.Load(reader);
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("Error: Out of memory", "NetTools: ODBCBrowse", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                this.dgridMain.DataSource = this.dataTable;
                this.lblStatus.Text = "Query successful, " + this.dataTable.Rows.Count.ToString() + " rows returned.";
                Cursor.Current = Cursors.Default;
            }
        }

        private void buttonRefreshDSN_Click(object sender, EventArgs e)
        {
            this.populateDSNDropdown();
            MessageBox.Show("DSN List Refreshed", "NetTools: ODBCBrowse");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void exportToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dataTable.Rows.Count < 1)
            {
                MessageBox.Show("Error: No data to export", "NetTools: ODBCBrowse");
            }
            else
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = "csv";
                dialog.Filter = "CSV file (*.csv)|*.csv";
                dialog.AddExtension = true;
                dialog.RestoreDirectory = true;
                dialog.Title = "Export Data to CSV";
                dialog.InitialDirectory = "C:/";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    dialog.Dispose();
                    dialog = null;
                }
                else
                {
                    StreamWriter writer = new StreamWriter(dialog.FileName);
                    string str = "";
                    foreach (DataColumn column in this.dataTable.Columns)
                    {
                        writer.Write(str);
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
                        str = ",";
                    }
                    writer.WriteLine();
                    ExportProgress progress = new ExportProgress();
                    progress.updateProgressMin(1);
                    progress.updateProgressMax(this.dataTable.Rows.Count);
                    progress.Show();
                    int num = 1;
                    foreach (DataRow row in this.dataTable.Rows)
                    {
                        str = "";
                        foreach (object obj2 in row.ItemArray)
                        {
                            writer.Write(str);
                            if (obj2.ToString().Contains(",") ||
                                obj2.ToString().Contains("\"") ||
                                obj2.ToString().Contains("\n"))
                            {
                                writer.Write("\"");
                                writer.Write(obj2.ToString().Trim().Replace("\"", "\"\"").Replace("\r", " ").Replace("\n", " "));
                                writer.Write("\"");
                            }
                            else
                            {
                                writer.Write(obj2.ToString().Trim());
                            }
                            str = ",";
                        }
                        writer.WriteLine();
                        progress.updateProgressBar(num);
                        num++;
                    }
                    writer.Close();
                    progress.Close();
                    progress.Dispose();
                    progress = null;
                    this.lblStatus.Text = "Export to '" + dialog.FileName + "' complete";
                }
            }
        }

        private void InitializeComponent()
        {
            this.menuStripMain = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();

            this.editToolStripMenuItem = new ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new ToolStripMenuItem();
            this.selectNoneToolStripMenuItem = new ToolStripMenuItem();

            this.flowLayoutPanel1 = new FlowLayoutPanel();
            this.label1 = new Label();
            this.cbxDSNList = new ComboBox();
            this.buttonRefreshDSN = new Button();
            this.label2 = new Label();
            this.tbUsername = new TextBox();
            this.label3 = new Label();
            this.tbPassword = new TextBox();
            this.btConnect = new Button();
            this.btDisconnect = new Button();
            this.splitContainer1 = new SplitContainer();
            this.tvTables = new TreeView();
            this.tableLayoutPanel1 = new TableLayoutPanel();
            this.dgridMain = new DataGridView();
            this.tableLayoutPanel2 = new TableLayoutPanel();
            this.flowLayoutPanel2 = new FlowLayoutPanel();
            this.buttonExec = new Button();
            this.tbSQLQuery = new TextBox();
            this.statusStripMain = new StatusStrip();
            this.lblStatus = new ToolStripStatusLabel();
            this.dataToolStripMenuItem = new ToolStripMenuItem();
            this.exportToCSVToolStripMenuItem = new ToolStripMenuItem();
            this.menuStripMain.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((ISupportInitialize) this.dgridMain).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            base.SuspendLayout();
            this.menuStripMain.Items.AddRange(new ToolStripItem[] { this.fileToolStripMenuItem, this.editToolStripMenuItem, this.dataToolStripMenuItem });
            this.menuStripMain.Location = new Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new Size(0x3c7, 0x18);
            this.menuStripMain.TabIndex = 0;
            this.menuStripMain.Text = "menuStrip1";
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.exitToolStripMenuItem });
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(0x25, 20);
            this.fileToolStripMenuItem.Text = "File";
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new EventHandler(this.exitToolStripMenuItem_Click);

            this.editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.selectAllToolStripMenuItem, this.selectNoneToolStripMenuItem });
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new Size(0x25, 20);
            this.editToolStripMenuItem.Text = "Edit";

            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.selectAllToolStripMenuItem.ShortcutKeys = Keys.Control & Keys.A;
            this.selectAllToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+A";
            this.selectAllToolStripMenuItem.Click += new EventHandler(selectAllToolStripMenuItem_Click);
            this.selectNoneToolStripMenuItem.Name = "selectNoneToolStripMenuItem";
            this.selectNoneToolStripMenuItem.Text = "Select None";
            this.selectNoneToolStripMenuItem.Size = new Size(0x98, 0x16);
            this.selectNoneToolStripMenuItem.ShortcutKeys = Keys.Control & Keys.Shift & Keys.A;
            this.selectNoneToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+A";
            this.selectNoneToolStripMenuItem.Click += new EventHandler(selectNoneToolStripMenuItem_Click);

            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.cbxDSNList);
            //this.flowLayoutPanel1.Controls.Add(this.buttonRefreshDSN);
            this.flowLayoutPanel1.Controls.Add(this.label2);
            this.flowLayoutPanel1.Controls.Add(this.tbUsername);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.tbPassword);
            this.flowLayoutPanel1.Controls.Add(this.btConnect);
            this.flowLayoutPanel1.Controls.Add(this.btDisconnect);
            this.flowLayoutPanel1.Dock = DockStyle.Top;
            this.flowLayoutPanel1.Location = new Point(0, 0x18);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new Size(0x3c7, 0x1c);
            this.flowLayoutPanel1.TabIndex = 1;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 6);
            this.label1.Margin = new Padding(12, 6, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x21, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "DSN:";
            this.cbxDSNList.FormattingEnabled = true;
            this.cbxDSNList.Location = new Point(0x33, 3);
            this.cbxDSNList.Name = "cbxDSNList";
            this.cbxDSNList.Size = new Size(0xbd, 0x15);
            this.cbxDSNList.TabIndex = 1;
            this.cbxDSNList.SelectedIndexChanged += new EventHandler(cbxDSNList_SelectedIndexChanged);
            this.buttonRefreshDSN.Location = new Point(0xf6, 2);
            this.buttonRefreshDSN.Margin = new Padding(3, 2, 3, 3);
            this.buttonRefreshDSN.Name = "buttonRefreshDSN";
            this.buttonRefreshDSN.Size = new Size(80, 0x16);
            this.buttonRefreshDSN.TabIndex = 2;
            this.buttonRefreshDSN.Text = "Refresh";
            this.buttonRefreshDSN.UseVisualStyleBackColor = true;
            this.buttonRefreshDSN.Click += new EventHandler(this.buttonRefreshDSN_Click);
            this.label2.AutoSize = true;
            this.label2.Location = new Point(0x14c, 6);
            this.label2.Margin = new Padding(3, 6, 3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x3a, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Username:";
            this.tbUsername.Location = new Point(0x18c, 3);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new Size(0x9c, 20);
            this.tbUsername.TabIndex = 4;
            this.label3.AutoSize = true;
            this.label3.Location = new Point(0x22e, 6);
            this.label3.Margin = new Padding(3, 6, 3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new Size(0x38, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Password:";
            this.tbPassword.Location = new Point(620, 3);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new Size(0xac, 20);
            this.tbPassword.TabIndex = 6;
            this.btConnect.Location = new Point(0x31e, 3);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new Size(80, 0x16);
            this.btConnect.TabIndex = 7;
            this.btConnect.Text = "Connect";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new EventHandler(this.btConnect_Click);
            this.btDisconnect.Location = new Point(0x374, 3);
            this.btDisconnect.Name = "btDisconnect";
            this.btDisconnect.Size = new Size(80, 0x16);
            this.btDisconnect.TabIndex = 8;
            this.btDisconnect.Text = "Disconnect";
            this.btDisconnect.UseVisualStyleBackColor = true;
            this.btDisconnect.Click += new EventHandler(this.btDisconnect_Click);
            this.splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Location = new Point(0, 0x34);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Panel1.Controls.Add(this.tvTables);
            this.splitContainer1.Panel1.Padding = new Padding(3, 3, 0, 0x18);
            this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel2.Padding = new Padding(0, 0, 0, 0x15);
            this.splitContainer1.Size = new Size(0x3c7, 0x20a);
            this.splitContainer1.SplitterDistance = 0x141;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 2;
            this.tvTables.Dock = DockStyle.Fill;
            this.tvTables.Location = new Point(3, 3);
            this.tvTables.Margin = new Padding(5);
            this.tvTables.Name = "tvTables";
            this.tvTables.Size = new Size(0x13e, 0x1ef);
            this.tvTables.TabIndex = 0;
            this.tvTables.AfterSelect += new TreeViewEventHandler(this.tvTables_AfterSelect);
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel1.Controls.Add(this.dgridMain, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Location = new Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 75f));
            this.tableLayoutPanel1.Size = new Size(0x284, 0x1f5);
            this.tableLayoutPanel1.TabIndex = 0;
            this.dgridMain.AllowUserToAddRows = false;
            this.dgridMain.AllowUserToDeleteRows = false;
            this.dgridMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgridMain.Dock = DockStyle.Fill;
            this.dgridMain.Location = new Point(3, 0x7d);
            this.dgridMain.Margin = new Padding(3, 0, 3, 3);
            this.dgridMain.Name = "dgridMain";
            this.dgridMain.ReadOnly = true;
            this.dgridMain.Size = new Size(0x27e, 0x175);
            this.dgridMain.TabIndex = 1;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel2.Controls.Add(this.flowLayoutPanel2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tbSQLQuery, 0, 0);
            this.tableLayoutPanel2.Dock = DockStyle.Fill;
            this.tableLayoutPanel2.Location = new Point(1, 1);
            this.tableLayoutPanel2.Margin = new Padding(1);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 23f));
            this.tableLayoutPanel2.Size = new Size(0x282, 0x7b);
            this.tableLayoutPanel2.TabIndex = 2;
            this.flowLayoutPanel2.Controls.Add(this.buttonExec);
            this.flowLayoutPanel2.Dock = DockStyle.Fill;
            this.flowLayoutPanel2.Location = new Point(0, 100);
            this.flowLayoutPanel2.Margin = new Padding(0);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.RightToLeft = RightToLeft.Yes;
            this.flowLayoutPanel2.Size = new Size(0x282, 0x17);
            this.flowLayoutPanel2.TabIndex = 0;
            this.buttonExec.Location = new Point(0x223, 0);
            this.buttonExec.Margin = new Padding(1, 0, 0, 0);
            this.buttonExec.Name = "buttonExec";
            this.buttonExec.Size = new Size(0x5e, 0x16);
            this.buttonExec.TabIndex = 0;
            this.buttonExec.Text = "Execute SQL";
            this.buttonExec.UseVisualStyleBackColor = true;
            this.buttonExec.Click += new EventHandler(this.buttonExec_Click);
            this.tbSQLQuery.Dock = DockStyle.Fill;
            this.tbSQLQuery.Location = new Point(1, 2);
            this.tbSQLQuery.Margin = new Padding(1, 2, 1, 1);
            this.tbSQLQuery.Multiline = true;
            this.tbSQLQuery.Name = "tbSQLQuery";
            this.tbSQLQuery.Size = new Size(640, 0x61);
            this.tbSQLQuery.TabIndex = 1;
            this.tbSQLQuery.KeyDown += new KeyEventHandler(tbSQLQuery_KeyDown);
            this.statusStripMain.Items.AddRange(new ToolStripItem[] { this.lblStatus });
            this.statusStripMain.Location = new Point(0, 0x228);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new Size(0x3c7, 0x16);
            this.statusStripMain.TabIndex = 3;
            this.statusStripMain.Text = "statusStrip1";
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(0x4f, 0x11);
            this.lblStatus.Text = "Disconnected";
            this.dataToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { this.exportToCSVToolStripMenuItem });
            this.dataToolStripMenuItem.Name = "dataToolStripMenuItem";
            this.dataToolStripMenuItem.Size = new Size(0x2b, 20);
            this.dataToolStripMenuItem.Text = "Data";
            this.exportToCSVToolStripMenuItem.Name = "exportToCSVToolStripMenuItem";
            this.exportToCSVToolStripMenuItem.Size = new Size(0x9a, 0x16);
            this.exportToCSVToolStripMenuItem.Text = "Export to CSV...";
            this.exportToCSVToolStripMenuItem.Click += new EventHandler(this.exportToCSVToolStripMenuItem_Click);
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x3c7, 0x23e);
            base.Controls.Add(this.statusStripMain);
            base.Controls.Add(this.splitContainer1);
            base.Controls.Add(this.flowLayoutPanel1);
            base.Controls.Add(this.menuStripMain);
            base.MainMenuStrip = this.menuStripMain;
            this.MinimumSize = new Size(0x3d7, 0x1bf);
            base.Name = "formMain";
            this.Text = "NetTools: ODBCBrowse";
            this.menuStripMain.ResumeLayout(false);
            this.menuStripMain.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((ISupportInitialize) this.dgridMain).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        void tbSQLQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                if (e.Shift)
                {
                    this.tbSQLQuery.DeselectAll();
                }
                else
                {
                    this.tbSQLQuery.SelectAll();
                }
            }
        }

        void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tbSQLQuery.DeselectAll();
        }

        void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.tbSQLQuery.SelectAll();
        }

        public void populateDSNDropdown()
        {
            this.cbxDSNList.Items.Clear();
            this.cbxDSNList.Text = "";
            for (int i = 0; i < this.dsnList.Count; i++)
            {
                string key = (string) this.dsnList.GetKey(i);
                DataSourceType byIndex = (DataSourceType) this.dsnList.GetByIndex(i);
                this.cbxDSNList.Items.Add(byIndex.ToString() + ":" + key);
            }
            int refreshItem = this.cbxDSNList.Items.Add("(Refresh...)");
        }

        void cbxDSNList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbxDSNList.SelectedItem.ToString() == "(Refresh...)")
            {
                this.dsnList = this.dsnManager.GetAllDataSourceNames();
                populateDSNDropdown();
            }
        }

        private void tvTables_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.tbSQLQuery.Text = "";
            this.tbSQLQuery.AppendText("select * from " + e.Node.Text);
        }
    }
}

