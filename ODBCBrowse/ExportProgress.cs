namespace ODBCBrowse
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    public class ExportProgress : Form
    {
        private IContainer components;
        private Label label1;
        private ProgressBar progressBar;

        public ExportProgress()
        {
            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.progressBar = new ProgressBar();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Location = new Point(0x65, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Exporting Data...";
            this.progressBar.Location = new Point(9, 30);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(0x112, 0x15);
            this.progressBar.TabIndex = 1;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            base.ClientSize = new Size(0x125, 60);
            base.Controls.Add(this.progressBar);
            base.Controls.Add(this.label1);
            this.MaximumSize = new Size(0x135, 0x60);
            this.MinimumSize = new Size(0x135, 0x60);
            base.Name = "ExportProgress";
            this.Text = "Export Progress";
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        public void updateProgressBar(int value)
        {
            this.progressBar.Value = value;
        }

        public void updateProgressMax(int value)
        {
            this.progressBar.Maximum = value;
        }

        public void updateProgressMin(int value)
        {
            this.progressBar.Minimum = value;
        }
    }
}

