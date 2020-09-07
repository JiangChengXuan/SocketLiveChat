namespace WebSocketServer
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.combox_ip = new System.Windows.Forms.ComboBox();
            this.btn_stop = new System.Windows.Forms.Button();
            this.btn_listen = new System.Windows.Forms.Button();
            this.tbox_port = new System.Windows.Forms.TextBox();
            this.rbox_rec = new System.Windows.Forms.RichTextBox();
            this.rbox_send = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_send = new System.Windows.Forms.Button();
            this.list_clients = new WebSocketServer.myListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statelb_count = new System.Windows.Forms.ToolStripStatusLabel();
            this.rbox_sendto = new System.Windows.Forms.RichTextBox();
            this.btn_clearall = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // combox_ip
            // 
            this.combox_ip.FormattingEnabled = true;
            this.combox_ip.Location = new System.Drawing.Point(10, 12);
            this.combox_ip.Name = "combox_ip";
            this.combox_ip.Size = new System.Drawing.Size(121, 20);
            this.combox_ip.TabIndex = 2;
            // 
            // btn_stop
            // 
            this.btn_stop.Enabled = false;
            this.btn_stop.Location = new System.Drawing.Point(328, 12);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(64, 23);
            this.btn_stop.TabIndex = 28;
            this.btn_stop.Text = "停止";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // btn_listen
            // 
            this.btn_listen.Location = new System.Drawing.Point(234, 11);
            this.btn_listen.Name = "btn_listen";
            this.btn_listen.Size = new System.Drawing.Size(88, 23);
            this.btn_listen.TabIndex = 27;
            this.btn_listen.Text = "监听";
            this.btn_listen.UseVisualStyleBackColor = true;
            this.btn_listen.Click += new System.EventHandler(this.btn_listen_Click);
            // 
            // tbox_port
            // 
            this.tbox_port.Location = new System.Drawing.Point(147, 12);
            this.tbox_port.Name = "tbox_port";
            this.tbox_port.Size = new System.Drawing.Size(70, 21);
            this.tbox_port.TabIndex = 29;
            this.tbox_port.Text = "27000";
            this.tbox_port.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // rbox_rec
            // 
            this.rbox_rec.Location = new System.Drawing.Point(8, 260);
            this.rbox_rec.Name = "rbox_rec";
            this.rbox_rec.ReadOnly = true;
            this.rbox_rec.Size = new System.Drawing.Size(380, 103);
            this.rbox_rec.TabIndex = 30;
            this.rbox_rec.Text = "";
            // 
            // rbox_send
            // 
            this.rbox_send.Location = new System.Drawing.Point(8, 134);
            this.rbox_send.Name = "rbox_send";
            this.rbox_send.ReadOnly = true;
            this.rbox_send.Size = new System.Drawing.Size(380, 103);
            this.rbox_send.TabIndex = 31;
            this.rbox_send.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 245);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 32;
            this.label1.Text = "接收";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 33;
            this.label2.Text = "发送";
            // 
            // btn_send
            // 
            this.btn_send.Location = new System.Drawing.Point(324, 46);
            this.btn_send.Name = "btn_send";
            this.btn_send.Size = new System.Drawing.Size(64, 63);
            this.btn_send.TabIndex = 34;
            this.btn_send.Text = "发送";
            this.btn_send.UseVisualStyleBackColor = true;
            this.btn_send.Click += new System.EventHandler(this.btn_send_Click);
            // 
            // list_clients
            // 
            this.list_clients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader4,
            this.columnHeader3});
            this.list_clients.Font = new System.Drawing.Font("宋体", 9F);
            this.list_clients.FullRowSelect = true;
            this.list_clients.GridLines = true;
            this.list_clients.HideSelection = false;
            this.list_clients.Location = new System.Drawing.Point(398, 11);
            this.list_clients.MultiSelect = false;
            this.list_clients.Name = "list_clients";
            this.list_clients.Size = new System.Drawing.Size(465, 352);
            this.list_clients.TabIndex = 35;
            this.list_clients.UseCompatibleStateImageBehavior = false;
            this.list_clients.View = System.Windows.Forms.View.Details;
            this.list_clients.Click += new System.EventHandler(this.list_clients_Click);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "地址";
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "IP";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 120;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "通讯时间";
            this.columnHeader4.Width = 127;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "状态";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.statelb_count});
            this.statusStrip1.Location = new System.Drawing.Point(0, 411);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(872, 22);
            this.statusStrip1.TabIndex = 36;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(95, 17);
            this.toolStripStatusLabel1.Text = "当日新连接总数:";
            // 
            // statelb_count
            // 
            this.statelb_count.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.statelb_count.Name = "statelb_count";
            this.statelb_count.Size = new System.Drawing.Size(15, 17);
            this.statelb_count.Text = "0";
            // 
            // rbox_sendto
            // 
            this.rbox_sendto.Location = new System.Drawing.Point(8, 46);
            this.rbox_sendto.Name = "rbox_sendto";
            this.rbox_sendto.Size = new System.Drawing.Size(312, 63);
            this.rbox_sendto.TabIndex = 37;
            this.rbox_sendto.Text = "";
            // 
            // btn_clearall
            // 
            this.btn_clearall.Location = new System.Drawing.Point(763, 368);
            this.btn_clearall.Name = "btn_clearall";
            this.btn_clearall.Size = new System.Drawing.Size(100, 39);
            this.btn_clearall.TabIndex = 40;
            this.btn_clearall.Text = "清除所有消息";
            this.btn_clearall.UseVisualStyleBackColor = true;
            this.btn_clearall.Click += new System.EventHandler(this.btn_clearall_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(872, 433);
            this.Controls.Add(this.btn_clearall);
            this.Controls.Add(this.rbox_sendto);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.list_clients);
            this.Controls.Add(this.btn_send);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rbox_send);
            this.Controls.Add(this.rbox_rec);
            this.Controls.Add(this.tbox_port);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_listen);
            this.Controls.Add(this.combox_ip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WebSocket服务器端 v1.5.0 by[煌]";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox combox_ip;
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.Button btn_listen;
        private System.Windows.Forms.TextBox tbox_port;
        private System.Windows.Forms.RichTextBox rbox_rec;
        private System.Windows.Forms.RichTextBox rbox_send;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_send;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.RichTextBox rbox_sendto;
        private System.Windows.Forms.Button btn_clearall;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel statelb_count;
        private myListView list_clients;
    }
}

