namespace Bitbird.Core.Tests.Forms.TestForms.AsyncTimer
{
    partial class AsyncTimerTestForm
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
            this.btnStartAwait = new System.Windows.Forms.Button();
            this.btnStopAwait = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnDispose = new System.Windows.Forms.Button();
            this.tbLog = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnCreate = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.timerInfo = new System.Windows.Forms.Timer(this.components);
            this.timerGc = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStartAwait
            // 
            this.btnStartAwait.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStartAwait.Location = new System.Drawing.Point(168, 18);
            this.btnStartAwait.Margin = new System.Windows.Forms.Padding(8);
            this.btnStartAwait.Name = "btnStartAwait";
            this.btnStartAwait.Size = new System.Drawing.Size(134, 54);
            this.btnStartAwait.TabIndex = 0;
            this.btnStartAwait.Text = "await Start";
            this.btnStartAwait.UseVisualStyleBackColor = true;
            this.btnStartAwait.Click += new System.EventHandler(this.btnStartAwait_Click);
            // 
            // btnStopAwait
            // 
            this.btnStopAwait.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStopAwait.Location = new System.Drawing.Point(318, 18);
            this.btnStopAwait.Margin = new System.Windows.Forms.Padding(8);
            this.btnStopAwait.Name = "btnStopAwait";
            this.btnStopAwait.Size = new System.Drawing.Size(134, 54);
            this.btnStopAwait.TabIndex = 1;
            this.btnStopAwait.Text = "await Stop";
            this.btnStopAwait.UseVisualStyleBackColor = true;
            this.btnStopAwait.Click += new System.EventHandler(this.btnStopAwait_Click);
            // 
            // btnStop
            // 
            this.btnStop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStop.Location = new System.Drawing.Point(468, 18);
            this.btnStop.Margin = new System.Windows.Forms.Padding(8);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(134, 54);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnDispose
            // 
            this.btnDispose.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDispose.Location = new System.Drawing.Point(618, 18);
            this.btnDispose.Margin = new System.Windows.Forms.Padding(8);
            this.btnDispose.Name = "btnDispose";
            this.btnDispose.Size = new System.Drawing.Size(134, 54);
            this.btnDispose.TabIndex = 3;
            this.btnDispose.Text = "Dispose";
            this.btnDispose.UseVisualStyleBackColor = true;
            this.btnDispose.Click += new System.EventHandler(this.btnDispose_Click);
            // 
            // tbLog
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.tbLog, 6);
            this.tbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLog.Font = new System.Drawing.Font("Consolas", 8.142858F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbLog.Location = new System.Drawing.Point(13, 83);
            this.tbLog.Multiline = true;
            this.tbLog.Name = "tbLog";
            this.tbLog.ReadOnly = true;
            this.tbLog.Size = new System.Drawing.Size(1009, 524);
            this.tbLog.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tbLog, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnStartAwait, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnStopAwait, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnStop, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnDispose, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnCreate, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblInfo, 5, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1035, 620);
            this.tableLayoutPanel1.TabIndex = 5;
            // 
            // btnCreate
            // 
            this.btnCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCreate.Location = new System.Drawing.Point(20, 20);
            this.btnCreate.Margin = new System.Windows.Forms.Padding(10);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(130, 50);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInfo.Font = new System.Drawing.Font("Consolas", 8.142858F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInfo.Location = new System.Drawing.Point(763, 10);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(259, 70);
            this.lblInfo.TabIndex = 6;
            this.lblInfo.Text = "Info";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // timerInfo
            // 
            this.timerInfo.Enabled = true;
            this.timerInfo.Interval = 500;
            this.timerInfo.Tick += new System.EventHandler(this.timerInfo_Tick);
            // 
            // timerGc
            // 
            this.timerGc.Enabled = true;
            this.timerGc.Interval = 1000;
            this.timerGc.Tick += new System.EventHandler(this.timerGc_Tick);
            // 
            // AsyncTimerTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1035, 620);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AsyncTimerTestForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Async Timer Test";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStartAwait;
        private System.Windows.Forms.Button btnStopAwait;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnDispose;
        private System.Windows.Forms.TextBox tbLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Timer timerInfo;
        private System.Windows.Forms.Timer timerGc;
    }
}