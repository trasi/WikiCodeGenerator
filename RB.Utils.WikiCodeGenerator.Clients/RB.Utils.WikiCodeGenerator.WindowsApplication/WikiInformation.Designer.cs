namespace RB.Utils.WikiCodeGenerator.WindowsApplication
{
    partial class WikiInformation
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
            this.label1 = new System.Windows.Forms.Label();
            this.ll_url = new System.Windows.Forms.LinkLabel();
            this.b_close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Viðeigandi Wiki-skjal hefur verið uppfært:";
            // 
            // ll_url
            // 
            this.ll_url.AutoSize = true;
            this.ll_url.Location = new System.Drawing.Point(25, 36);
            this.ll_url.Name = "ll_url";
            this.ll_url.Size = new System.Drawing.Size(80, 13);
            this.ll_url.TabIndex = 1;
            this.ll_url.TabStop = true;
            this.ll_url.Text = "url wiki-skjals ...";
            this.ll_url.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ll_url_clicked);
            // 
            // b_close
            // 
            this.b_close.Location = new System.Drawing.Point(272, 83);
            this.b_close.Name = "b_close";
            this.b_close.Size = new System.Drawing.Size(100, 27);
            this.b_close.TabIndex = 2;
            this.b_close.Text = "&Loka";
            this.b_close.UseVisualStyleBackColor = true;
            this.b_close.Click += new System.EventHandler(this.b_close_Click);
            // 
            // WikiInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 122);
            this.Controls.Add(this.b_close);
            this.Controls.Add(this.ll_url);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WikiInformation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Wiki-uppfærslu lokið";
            this.ResumeLayout(false);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.b_close_Click);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel ll_url;
        private System.Windows.Forms.Button b_close;
    }
}