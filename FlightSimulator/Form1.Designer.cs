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
            redisStatus = new ToolStripStatusLabel();
            listView1 = new ListView();
            Key = new ColumnHeader();
            Value = new ColumnHeader();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.BackColor = Color.LightGray;
            statusStrip1.ForeColor = Color.Black;
            statusStrip1.Items.AddRange(new ToolStripItem[] { connectionStatus, redisStatus });
            statusStrip1.Location = new Point(0, 428);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(658, 22);
            statusStrip1.TabIndex = 0;
            // 
            // connectionStatus
            // 
            connectionStatus.Image = Properties.Resources.plane_red;
            connectionStatus.Name = "connectionStatus";
            connectionStatus.Size = new Size(16, 17);
            // 
            // redisStatus
            // 
            redisStatus.Image = Properties.Resources.redis_red;
            redisStatus.Name = "redisStatus";
            redisStatus.Size = new Size(16, 17);

            // 
            // listView1
            // 
            listView1.AllowColumnReorder = true;
            listView1.Columns.AddRange(new ColumnHeader[] { Key, Value });
            listView1.FullRowSelect = true;
            listView1.GridLines = true;
            listView1.Location = new Point(16, 13);
            listView1.Name = "listView1";
            listView1.Size = new Size(550, 400);
            listView1.Sorting = SortOrder.Ascending;
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // Key
            // 
            Key.Text = "Key";
            Key.Width = 275;
            // 
            // Value
            // 
            Value.Text = "Value";
            Value.Width = 275;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(658, 450);
            Controls.Add(listView1);
            Controls.Add(statusStrip1);
            Name = "Form1";
            Text = "Flight Simulator Listener";
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private ToolStripStatusLabel connectionStatus , redisStatus;
        private ListView listView1;
        private ColumnHeader Key;
        private ColumnHeader Value;
    }
}
