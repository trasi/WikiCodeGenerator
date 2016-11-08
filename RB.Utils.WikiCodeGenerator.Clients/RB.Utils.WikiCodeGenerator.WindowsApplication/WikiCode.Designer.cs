namespace RB.Utils.WikiCodeGenerator.WindowsApplication
{
    partial class WikiCode
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
            this.b_save_wikicode = new System.Windows.Forms.Button();
            this.b_copy_wikicode = new System.Windows.Forms.Button();
            this.tb_wikicode = new System.Windows.Forms.RichTextBox();
            this.b_close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // b_save_wikicode
            // 
            this.b_save_wikicode.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.b_save_wikicode.Location = new System.Drawing.Point(364, 326);
            this.b_save_wikicode.Name = "b_save_wikicode";
            this.b_save_wikicode.Size = new System.Drawing.Size(100, 27);
            this.b_save_wikicode.TabIndex = 27;
            this.b_save_wikicode.Text = "Vista Wiki-kóða";
            this.b_save_wikicode.UseVisualStyleBackColor = true;
            this.b_save_wikicode.Click += new System.EventHandler(this.b_save_wikicode_click);
            // 
            // b_copy_wikicode
            // 
            this.b_copy_wikicode.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.b_copy_wikicode.Location = new System.Drawing.Point(258, 326);
            this.b_copy_wikicode.Name = "b_copy_wikicode";
            this.b_copy_wikicode.Size = new System.Drawing.Size(100, 27);
            this.b_copy_wikicode.TabIndex = 25;
            this.b_copy_wikicode.Text = "Afrita Wiki-kóða";
            this.b_copy_wikicode.UseVisualStyleBackColor = true;
            this.b_copy_wikicode.Click += new System.EventHandler(this.b_copy_wikicode_click);
            // 
            // tb_wikicode
            // 
            this.tb_wikicode.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.tb_wikicode.Location = new System.Drawing.Point(12, 12);
            this.tb_wikicode.Name = "tb_wikicode";
            this.tb_wikicode.Size = new System.Drawing.Size(560, 308);
            this.tb_wikicode.TabIndex = 26;
            this.tb_wikicode.Text = "";
            // 
            // b_close
            // 
            this.b_close.Location = new System.Drawing.Point(472, 326);
            this.b_close.Name = "b_close";
            this.b_close.Size = new System.Drawing.Size(100, 27);
            this.b_close.TabIndex = 28;
            this.b_close.Text = "&Loka";
            this.b_close.UseVisualStyleBackColor = true;
            this.b_close.Click += new System.EventHandler(this.b_close_Click);
            // 
            // WikiCode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 362);
            this.Controls.Add(this.b_close);
            this.Controls.Add(this.b_save_wikicode);
            this.Controls.Add(this.b_copy_wikicode);
            this.Controls.Add(this.tb_wikicode);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WikiCode";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Wiki-kóði";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.b_close_Click);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button b_save_wikicode;
        private System.Windows.Forms.Button b_copy_wikicode;
        private System.Windows.Forms.RichTextBox tb_wikicode;
        private System.Windows.Forms.Button b_close;
    }
}