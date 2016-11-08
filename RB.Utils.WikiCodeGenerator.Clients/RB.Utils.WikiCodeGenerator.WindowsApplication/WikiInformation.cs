using System;
using System.Drawing;
using System.Windows.Forms;

namespace RB.Utils.WikiCodeGenerator.WindowsApplication
{
    public partial class WikiInformation : Form
    {
        private Form caller;

        public WikiInformation()
        {
            InitializeComponent();
        }

        public WikiInformation(Form caller, string url)
        {
            InitializeComponent();

            this.ll_url.Text = url;
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

        private void ll_url_clicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(ll_url.Text);
        }

        private void b_close_Click(object sender, FormClosingEventArgs e)
        {
            caller.Enabled = true;
            caller.Focus();
        }
    }
}
