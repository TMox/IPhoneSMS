namespace iPhoneMessageExport
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.comboBackups = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblGroupCount = new System.Windows.Forms.Label();
            this.lbPreview = new System.Windows.Forms.ListBox();
            this.lbMessageGroup = new System.Windows.Forms.ListBox();
            this.labelMessageGroup = new System.Windows.Forms.Label();
            this.labelBackupFiles = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBackups
            // 
            this.comboBackups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBackups.Enabled = false;
            this.comboBackups.FormattingEnabled = true;
            this.comboBackups.ItemHeight = 16;
            this.comboBackups.Location = new System.Drawing.Point(16, 36);
            this.comboBackups.Margin = new System.Windows.Forms.Padding(4);
            this.comboBackups.Name = "comboBackups";
            this.comboBackups.Size = new System.Drawing.Size(780, 24);
            this.comboBackups.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblGroupCount);
            this.panel1.Controls.Add(this.lbPreview);
            this.panel1.Controls.Add(this.lbMessageGroup);
            this.panel1.Controls.Add(this.labelMessageGroup);
            this.panel1.Controls.Add(this.labelBackupFiles);
            this.panel1.Controls.Add(this.btnLoad);
            this.panel1.Controls.Add(this.btnExport);
            this.panel1.Controls.Add(this.comboBackups);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(860, 442);
            this.panel1.TabIndex = 4;
            // 
            // lblGroupCount
            // 
            this.lblGroupCount.AutoSize = true;
            this.lblGroupCount.Font = new System.Drawing.Font("Verdana", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGroupCount.Location = new System.Drawing.Point(415, 84);
            this.lblGroupCount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblGroupCount.Name = "lblGroupCount";
            this.lblGroupCount.Size = new System.Drawing.Size(12, 12);
            this.lblGroupCount.TabIndex = 9;
            this.lblGroupCount.Text = "0";
            this.lblGroupCount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lbPreview
            // 
            this.lbPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbPreview.FormattingEnabled = true;
            this.lbPreview.IntegralHeight = false;
            this.lbPreview.ItemHeight = 16;
            this.lbPreview.Location = new System.Drawing.Point(435, 96);
            this.lbPreview.Margin = new System.Windows.Forms.Padding(4);
            this.lbPreview.Name = "lbPreview";
            this.lbPreview.Size = new System.Drawing.Size(411, 292);
            this.lbPreview.TabIndex = 8;
            // 
            // lbMessageGroup
            // 
            this.lbMessageGroup.FormattingEnabled = true;
            this.lbMessageGroup.IntegralHeight = false;
            this.lbMessageGroup.ItemHeight = 16;
            this.lbMessageGroup.Location = new System.Drawing.Point(16, 97);
            this.lbMessageGroup.Margin = new System.Windows.Forms.Padding(4);
            this.lbMessageGroup.Name = "lbMessageGroup";
            this.lbMessageGroup.Size = new System.Drawing.Size(411, 292);
            this.lbMessageGroup.TabIndex = 7;
            this.lbMessageGroup.SelectedIndexChanged += new System.EventHandler(this.lbMessageGroup_SelectedIndexChanged);
            // 
            // labelMessageGroup
            // 
            this.labelMessageGroup.AutoSize = true;
            this.labelMessageGroup.Location = new System.Drawing.Point(16, 76);
            this.labelMessageGroup.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMessageGroup.Name = "labelMessageGroup";
            this.labelMessageGroup.Size = new System.Drawing.Size(139, 17);
            this.labelMessageGroup.TabIndex = 6;
            this.labelMessageGroup.Text = "Message Group List:";
            // 
            // labelBackupFiles
            // 
            this.labelBackupFiles.AutoSize = true;
            this.labelBackupFiles.Location = new System.Drawing.Point(16, 15);
            this.labelBackupFiles.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelBackupFiles.Name = "labelBackupFiles";
            this.labelBackupFiles.Size = new System.Drawing.Size(92, 17);
            this.labelBackupFiles.TabIndex = 5;
            this.labelBackupFiles.Text = "Backup Files:";
            // 
            // btnLoad
            // 
            this.btnLoad.Enabled = false;
            this.btnLoad.Image = ((System.Drawing.Image)(resources.GetObject("btnLoad.Image")));
            this.btnLoad.Location = new System.Drawing.Point(805, 30);
            this.btnLoad.Margin = new System.Windows.Forms.Padding(4);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(39, 36);
            this.btnLoad.TabIndex = 4;
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExport.Enabled = false;
            this.btnExport.Location = new System.Drawing.Point(16, 399);
            this.btnExport.Margin = new System.Windows.Forms.Padding(4);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(828, 28);
            this.btnExport.TabIndex = 0;
            this.btnExport.Text = "Export Messages for Selected Message Group to HTML";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 442);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "iPhone Message Exporter";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBackups;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label labelBackupFiles;
        private System.Windows.Forms.ListBox lbMessageGroup;
        private System.Windows.Forms.Label labelMessageGroup;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ListBox lbPreview;
        private System.Windows.Forms.Label lblGroupCount;
    }
}

