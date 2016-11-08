using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace RB.Utils.WikiCodeGenerator.WindowsApplication
{
    public partial class WikiCode : Form
    {
        private Form caller;

        public WikiCode()
        {
            InitializeComponent();
        }

        public WikiCode(Form caller, string wikicode)
        {
            InitializeComponent();

            this.tb_wikicode.Text = wikicode;
            this.caller = caller;

            this.Location = new Point(caller.Location.X + 50, caller.Location.Y + 50);
        }

        private void b_close_Click(object sender, EventArgs e)
        {
            this.Close();

            caller.Enabled = true;
            caller.Focus();

            this.Dispose();
        }

        private void b_save_wikicode_click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();

            fd.OverwritePrompt = true;

            DialogResult dr = fd.ShowDialog();

            if (dr == DialogResult.OK)
                File.WriteAllText(fd.FileName, tb_wikicode.Text, System.Text.Encoding.UTF8);
        }
        private void tb_wikicode_changed(object sender, EventArgs e)
        {
            b_copy_wikicode.Enabled = (tb_wikicode.Text.Length == 0) ? false : true;
            b_save_wikicode.Enabled = (tb_wikicode.Text.Length == 0) ? false : true;
        }
        
        private void b_copy_wikicode_click(object sender, EventArgs e)
        {
            Clipboard.SetText(tb_wikicode.Text);
        }

        private void b_close_Click(object sender, FormClosingEventArgs e)
        {
            caller.Enabled = true;
            caller.Focus();
        }
    }
}
