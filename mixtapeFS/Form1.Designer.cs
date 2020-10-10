namespace mixtapeFS
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
            this.gLog = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gCacheGB = new System.Windows.Forms.TextBox();
            this.gCacheMin = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.gBrowseMount = new System.Windows.Forms.Button();
            this.gDst = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.gStart = new System.Windows.Forms.Button();
            this.gList = new System.Windows.Forms.ListBox();
            this.gDel = new System.Windows.Forms.Button();
            this.gAdd = new System.Windows.Forms.Button();
            this.gName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gBrowseSource = new System.Windows.Forms.Button();
            this.gSrc = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gCacheDir = new System.Windows.Forms.TextBox();
            this.gBrowseCache = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gLog
            // 
            this.gLog.AutoSize = true;
            this.gLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gLog.Location = new System.Drawing.Point(0, 345);
            this.gLog.Name = "gLog";
            this.gLog.Size = new System.Drawing.Size(244, 13);
            this.gLog.TabIndex = 0;
            this.gLog.Text = "filesystem log feed goes here (nothing to show yet)";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gBrowseCache);
            this.panel1.Controls.Add(this.gCacheDir);
            this.panel1.Controls.Add(this.gCacheGB);
            this.panel1.Controls.Add(this.gCacheMin);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.gBrowseMount);
            this.panel1.Controls.Add(this.gDst);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.gStart);
            this.panel1.Controls.Add(this.gList);
            this.panel1.Controls.Add(this.gDel);
            this.panel1.Controls.Add(this.gAdd);
            this.panel1.Controls.Add(this.gName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.gBrowseSource);
            this.panel1.Controls.Add(this.gSrc);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1244, 345);
            this.panel1.TabIndex = 1;
            // 
            // gCacheGB
            // 
            this.gCacheGB.Location = new System.Drawing.Point(181, 43);
            this.gCacheGB.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.gCacheGB.Name = "gCacheGB";
            this.gCacheGB.Size = new System.Drawing.Size(49, 20);
            this.gCacheGB.TabIndex = 14;
            this.gCacheGB.Text = "64";
            // 
            // gCacheMin
            // 
            this.gCacheMin.Location = new System.Drawing.Point(62, 43);
            this.gCacheMin.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.gCacheMin.Name = "gCacheMin";
            this.gCacheMin.Size = new System.Drawing.Size(49, 20);
            this.gCacheMin.TabIndex = 11;
            this.gCacheMin.Text = "30";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(236, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "gigabyte, in";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(117, 46);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "minutes, or";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 46);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Cache:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(144, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(216, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "for  example  M  or  R   (any free drive name)";
            // 
            // gBrowseMount
            // 
            this.gBrowseMount.Location = new System.Drawing.Point(611, 155);
            this.gBrowseMount.Name = "gBrowseMount";
            this.gBrowseMount.Size = new System.Drawing.Size(64, 23);
            this.gBrowseMount.TabIndex = 2;
            this.gBrowseMount.Text = "Browse";
            this.gBrowseMount.UseVisualStyleBackColor = true;
            this.gBrowseMount.Visible = false;
            this.gBrowseMount.Click += new System.EventHandler(this.gBrowseMount_Click);
            // 
            // gDst
            // 
            this.gDst.Location = new System.Drawing.Point(62, 14);
            this.gDst.Name = "gDst";
            this.gDst.Size = new System.Drawing.Size(76, 20);
            this.gDst.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Mount:";
            // 
            // gStart
            // 
            this.gStart.Location = new System.Drawing.Point(423, 12);
            this.gStart.Name = "gStart";
            this.gStart.Size = new System.Drawing.Size(110, 25);
            this.gStart.TabIndex = 0;
            this.gStart.Text = "S T A R T !";
            this.gStart.UseVisualStyleBackColor = true;
            this.gStart.Click += new System.EventHandler(this.gStart_Click);
            // 
            // gList
            // 
            this.gList.FormattingEnabled = true;
            this.gList.Location = new System.Drawing.Point(12, 131);
            this.gList.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.gList.Name = "gList";
            this.gList.Size = new System.Drawing.Size(521, 199);
            this.gList.TabIndex = 7;
            // 
            // gDel
            // 
            this.gDel.Location = new System.Drawing.Point(377, 99);
            this.gDel.Name = "gDel";
            this.gDel.Size = new System.Drawing.Size(156, 23);
            this.gDel.TabIndex = 7;
            this.gDel.Text = "Delete selected row";
            this.gDel.UseVisualStyleBackColor = true;
            this.gDel.Click += new System.EventHandler(this.gDel_Click);
            // 
            // gAdd
            // 
            this.gAdd.Location = new System.Drawing.Point(208, 99);
            this.gAdd.Name = "gAdd";
            this.gAdd.Size = new System.Drawing.Size(163, 23);
            this.gAdd.TabIndex = 6;
            this.gAdd.Text = "Add new";
            this.gAdd.UseVisualStyleBackColor = true;
            this.gAdd.Click += new System.EventHandler(this.gAdd_Click);
            // 
            // gName
            // 
            this.gName.Location = new System.Drawing.Point(62, 101);
            this.gName.Margin = new System.Windows.Forms.Padding(3, 6, 3, 3);
            this.gName.Name = "gName";
            this.gName.Size = new System.Drawing.Size(140, 20);
            this.gName.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Name:";
            // 
            // gBrowseSource
            // 
            this.gBrowseSource.Location = new System.Drawing.Point(469, 70);
            this.gBrowseSource.Name = "gBrowseSource";
            this.gBrowseSource.Size = new System.Drawing.Size(64, 23);
            this.gBrowseSource.TabIndex = 4;
            this.gBrowseSource.Text = "Browse";
            this.gBrowseSource.UseVisualStyleBackColor = true;
            this.gBrowseSource.Click += new System.EventHandler(this.gBrowseSource_Click);
            // 
            // gSrc
            // 
            this.gSrc.Location = new System.Drawing.Point(62, 72);
            this.gSrc.Name = "gSrc";
            this.gSrc.Size = new System.Drawing.Size(401, 20);
            this.gSrc.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source:";
            // 
            // gCacheDir
            // 
            this.gCacheDir.Location = new System.Drawing.Point(303, 43);
            this.gCacheDir.Name = "gCacheDir";
            this.gCacheDir.Size = new System.Drawing.Size(160, 20);
            this.gCacheDir.TabIndex = 16;
            this.gCacheDir.Text = "c:\\cmfsc\\";
            // 
            // gBrowseCache
            // 
            this.gBrowseCache.Location = new System.Drawing.Point(469, 41);
            this.gBrowseCache.Name = "gBrowseCache";
            this.gBrowseCache.Size = new System.Drawing.Size(64, 23);
            this.gBrowseCache.TabIndex = 17;
            this.gBrowseCache.Text = "Browse";
            this.gBrowseCache.UseVisualStyleBackColor = true;
            this.gBrowseCache.Click += new System.EventHandler(this.gBrowseCache_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1244, 601);
            this.Controls.Add(this.gLog);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "mixtapeFS  --  github.com/9001/mixtapeFS";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label gLog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button gStart;
        private System.Windows.Forms.ListBox gList;
        private System.Windows.Forms.Button gDel;
        private System.Windows.Forms.Button gAdd;
        private System.Windows.Forms.TextBox gName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button gBrowseSource;
        private System.Windows.Forms.TextBox gSrc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button gBrowseMount;
        private System.Windows.Forms.TextBox gDst;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox gCacheMin;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox gCacheGB;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox gCacheDir;
        private System.Windows.Forms.Button gBrowseCache;

    }
}

