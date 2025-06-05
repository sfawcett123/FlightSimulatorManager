namespace FlightSimulator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            statusStrip1 = new StatusStrip();
            connectionStatus = new ToolStripStatusLabel();
            Connect = new Button();
            timer1 = new System.Windows.Forms.Timer(components);
            listView1 = new ListView();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.LightGray;
            statusStrip1.ForeColor = Color.Black;
            statusStrip1.Items.AddRange(new ToolStripItem[] { connectionStatus });
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(433, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "Not Connected";
            // 
            // connectionStatus
            // 
            connectionStatus.Name = "connectionStatus";
            connectionStatus.Size = new Size(88, 17);
            connectionStatus.Text = "Not Connected";
            // 
            // Connect
            // 
            Connect.Location = new Point(347, 386);
            Connect.Name = "Connect";
            Connect.Size = new Size(75, 23);
            Connect.TabIndex = 1;
            Connect.Text = "Connect";
            Connect.UseVisualStyleBackColor = true;
            Connect.Click += Connect_Click;
            // 
            // timer1
            // 
            timer1.Tick += timer1_Tick;
            // 
            // listView1
            // 
            listView1.Location = new Point(16, 13);
            listView1.Name = "listView1";
            listView1.Size = new Size(325, 396);
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.AllowColumnReorder = true;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            listView1.Sorting = SortOrder.Ascending;
            listView1.View = View.Details;
            listView1.Columns.Add("Key", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("Value", -2, HorizontalAlignment.Left);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(433, 450);
            Controls.Add(listView1);
            Controls.Add(Connect);
            Controls.Add(statusStrip1);
            Name = "Form1";
            Text = "Form1";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private Button Connect;
        private ToolStripStatusLabel connectionStatus;
        private System.Windows.Forms.Timer timer1;
        private ListView listView1;
    }
}
