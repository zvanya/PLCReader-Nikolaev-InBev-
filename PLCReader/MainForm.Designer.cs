namespace PLCReader
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.DisconnectBtn = new System.Windows.Forms.Button();
            this.ConnectBtn = new System.Windows.Forms.Button();
            this.timerPlcDataPolling = new System.Windows.Forms.Timer(this.components);
            this.timerSendDataToServer = new System.Windows.Forms.Timer(this.components);
            this.timerPlcConnectionCheck = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPLCStatus = new System.Windows.Forms.Label();
            this.timerStatus = new System.Windows.Forms.Timer(this.components);
            this.btnClearListBox = new System.Windows.Forms.Button();
            this.timerProductivityCalc = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 75);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(599, 277);
            this.listBox1.TabIndex = 56;
            // 
            // DisconnectBtn
            // 
            this.DisconnectBtn.Enabled = false;
            this.DisconnectBtn.Location = new System.Drawing.Point(90, 12);
            this.DisconnectBtn.Name = "DisconnectBtn";
            this.DisconnectBtn.Size = new System.Drawing.Size(75, 60);
            this.DisconnectBtn.TabIndex = 55;
            this.DisconnectBtn.Text = "Disconnect";
            this.DisconnectBtn.UseVisualStyleBackColor = true;
            this.DisconnectBtn.Click += new System.EventHandler(this.DisconnectBtn_Click);
            // 
            // ConnectBtn
            // 
            this.ConnectBtn.Location = new System.Drawing.Point(12, 12);
            this.ConnectBtn.Name = "ConnectBtn";
            this.ConnectBtn.Size = new System.Drawing.Size(72, 60);
            this.ConnectBtn.TabIndex = 54;
            this.ConnectBtn.Text = "Connect";
            this.ConnectBtn.UseVisualStyleBackColor = true;
            this.ConnectBtn.Click += new System.EventHandler(this.ConnectBtn_Click);
            // 
            // timerPlcDataPolling
            // 
            this.timerPlcDataPolling.Interval = 2000;
            this.timerPlcDataPolling.Tick += new System.EventHandler(this.timerPlcDataPolling_Tick);
            // 
            // timerSendDataToServer
            // 
            this.timerSendDataToServer.Interval = 20000;
            this.timerSendDataToServer.Tick += new System.EventHandler(this.timerSendDataToServer_Tick);
            // 
            // timerPlcConnectionCheck
            // 
            this.timerPlcConnectionCheck.Interval = 60000;
            this.timerPlcConnectionCheck.Tick += new System.EventHandler(this.timerPlcConnectionCheck_Tick);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblStatus.Location = new System.Drawing.Point(596, 30);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(15, 15);
            this.lblStatus.TabIndex = 57;
            // 
            // lblPLCStatus
            // 
            this.lblPLCStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPLCStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblPLCStatus.Location = new System.Drawing.Point(596, 12);
            this.lblPLCStatus.Name = "lblPLCStatus";
            this.lblPLCStatus.Size = new System.Drawing.Size(15, 15);
            this.lblPLCStatus.TabIndex = 57;
            // 
            // timerStatus
            // 
            this.timerStatus.Enabled = true;
            this.timerStatus.Interval = 1000;
            this.timerStatus.Tick += new System.EventHandler(this.timerStatus_Tick);
            // 
            // btnClearListBox
            // 
            this.btnClearListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearListBox.Location = new System.Drawing.Point(536, 49);
            this.btnClearListBox.Name = "btnClearListBox";
            this.btnClearListBox.Size = new System.Drawing.Size(75, 23);
            this.btnClearListBox.TabIndex = 58;
            this.btnClearListBox.Text = "clear";
            this.btnClearListBox.UseVisualStyleBackColor = true;
            this.btnClearListBox.Click += new System.EventHandler(this.btnClearListBox_Click);
            // 
            // timerProductivityCalc
            // 
            this.timerProductivityCalc.Interval = 60000;
            this.timerProductivityCalc.Tick += new System.EventHandler(this.timerProductivityCalc_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 370);
            this.Controls.Add(this.btnClearListBox);
            this.Controls.Add(this.lblPLCStatus);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.DisconnectBtn);
            this.Controls.Add(this.ConnectBtn);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "S7 PLC Reader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        internal System.Windows.Forms.Button DisconnectBtn;
        internal System.Windows.Forms.Button ConnectBtn;
        private System.Windows.Forms.Timer timerPlcDataPolling;
        private System.Windows.Forms.Timer timerSendDataToServer;
        private System.Windows.Forms.Timer timerPlcConnectionCheck;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPLCStatus;
        private System.Windows.Forms.Timer timerStatus;
        private System.Windows.Forms.Button btnClearListBox;
        private System.Windows.Forms.Timer timerProductivityCalc;
    }
}

