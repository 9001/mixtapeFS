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
            this.SuspendLayout();
            // 
            // gLog
            // 
            this.gLog.AutoSize = true;
            this.gLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gLog.Location = new System.Drawing.Point(0, 0);
            this.gLog.Name = "gLog";
            this.gLog.Size = new System.Drawing.Size(35, 13);
            this.gLog.TabIndex = 0;
            this.gLog.Text = "label1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(961, 898);
            this.Controls.Add(this.gLog);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label gLog;

    }
}

