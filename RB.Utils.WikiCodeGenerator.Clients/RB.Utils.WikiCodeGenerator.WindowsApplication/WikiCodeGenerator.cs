using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace RB.Utils.WikiCodeGenerator.WindowsApplication
{
    public partial class WikiCodeGenerator : Form
    {
        private Dictionary<string, string> languages;

        public WikiCodeGenerator()
        {
            InitializeComponent();

            this.Text = string.Format("RB Wiki-kóðasmiður ({0})", Assembly.GetExecutingAssembly().GetName().Version.ToString());

            languages = new Dictionary<string, string>();
            languages.Add("Íslenska", WikiCodeLanguage.Icelandic);
            languages.Add("Enska", WikiCodeLanguage.English);

            foreach (string language in languages.Keys)
                cb_language.Items.Add(language);

            cb_language.SelectedIndex = cb_language.Items.Count == 0 ? -1 : 0;
        }

        public WikiCodeGenerator(string wsdl_file) : this()
        {
            tb_wsdl.Text = wsdl_file;
            tb_wsdl_changed(null, null);
        }

        private void b_generate_wikicode_click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                string wikicode = Generator.GenerateWikiCode(
                                            tb_description.Text,
                                            cb_operation.SelectedItem.ToString(),
                                            tb_wsdl.Text,
                                            String.IsNullOrWhiteSpace(tb_message_schema.Text) ? tb_wsdl.Text : tb_message_schema.Text,
                                            tb_type_schema.Text,
                                            tb_common_type_schema.Text,
                                            tb_version_number.Text,
                                            dp_version_date.Text,
                                            tb_version_description.Text,
                                            cb_example.Checked,
                                            cb_version.Checked,
                                            cb_dictionary.Checked,
                                            languages[cb_language.SelectedItem.ToString()]);

                this.Enabled = false;
                
                if (cb_wiki.CheckState == CheckState.Checked)
                {
                    string url = Generator.SubmitToWiki(
                                            WebServiceParser.GetServiceName(tb_wsdl.Text),
                                            cb_operation.SelectedItem.ToString(),
                                            wikicode,
                                            tb_wiki_user.Text,
                                            tb_wiki_password.Text,
                                            !cb_merge.Checked);

                    WikiInformation wiki_information = new WikiInformation(this, url);
                    wiki_information.Show();
                }
                else
                {
                    WikiCode wiki_information = new WikiCode(this, wikicode);
                    wiki_information.Show();
                }

            }
            catch (Exception x)
            {
                MessageBox.Show(
                    String.Format(
                        @"Eftirfarandi villa kom upp: {0} {3} {3}Köllunarstakkur villu: {3}{1} {2}",
                        x.Message,
                        x.StackTrace,
                        x.InnerException == null ? 
                            String.Empty : 
                            String.Format("{1} {1}Innri villa: {1}{0} ", 
                                x.InnerException.Message, 
                                System.Environment.NewLine),
                        System.Environment.NewLine),
                    "Villa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                this.Enabled = true;
            }
            finally
            {
                this.Cursor = Cursors.Default;                
            }
        }

        private void b_message_schema_file_click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            fd.FileName = Path.GetFileName(tb_message_schema.Text);
            fd.Filter = "W3C XML Schema|*.xsd;";
            fd.Multiselect = false;

            DialogResult dr = fd.ShowDialog();

            if (dr == DialogResult.OK)
                tb_message_schema.Text = fd.FileName;
        }

        private void b_type_schema_file_click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            fd.FileName = Path.GetFileName(tb_type_schema.Text);
            fd.Filter = "W3C XML Schema|*.xsd";
            fd.Multiselect = false;

            DialogResult dr = fd.ShowDialog();

            if (dr == DialogResult.OK)
                tb_type_schema.Text = fd.FileName;

        }

        private void b_common_type_schema_file_click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            fd.FileName = Path.GetFileName(tb_common_type_schema.Text);
            fd.Filter = "W3C XML Schema|*.xsd";
            fd.Multiselect = false;

            DialogResult dr = fd.ShowDialog();

            if (dr == DialogResult.OK)
                tb_common_type_schema.Text = fd.FileName;
        }

        private void cb_version_toggled(object sender, EventArgs e)
        {
            tb_version_number.Enabled = (cb_version.CheckState == CheckState.Checked) ? true : false;
            dp_version_date.Enabled = (cb_version.CheckState == CheckState.Checked) ? true : false;
            tb_version_description.Enabled = (cb_version.CheckState == CheckState.Checked) ? true : false;
        }

        private void tb_wsdl_changed(object sender, EventArgs e)
        {
            Tuple<string, string, string> schema_files = null;

            try
            {
                schema_files = WebServiceParser.GetSchemaFiles(tb_wsdl.Text);
            }
            catch
            { }

            if (schema_files != null)
            {
                if (!String.IsNullOrWhiteSpace(schema_files.Item1))
                    tb_message_schema.Text = schema_files.Item1;

                if (!String.IsNullOrWhiteSpace(schema_files.Item2))
                    tb_type_schema.Text = schema_files.Item2;

                if (!String.IsNullOrWhiteSpace(schema_files.Item3))
                    tb_common_type_schema.Text = schema_files.Item3;
            }

            List<Tuple<string, string, string>> operations = new List<Tuple<string, string, string>>();

            try
            {
                operations = WebServiceParser.GetOperationsWithMessages(tb_wsdl.Text);
            }
            catch
            { }

            cb_operation.Items.Clear();

            if (operations.Count == 0)
            {
                cb_operation.Items.Add("Engar þjónustuaðgerðir finnast ...");

                cb_operation.SelectedIndex = 0;

                b_generate_wikicode.Enabled = false;

                return;
            }

            operations.Sort();

            foreach (Tuple<string, string, string> operation in operations)
            {
                cb_operation.Items.Add(operation.Item1);
            }

            cb_operation.SelectedIndex = 0;

            b_generate_wikicode.Enabled = true;
        }
        
        private void b_wsdl_file_click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            fd.FileName = Path.GetFileName(tb_message_schema.Text);
            fd.Filter = "Web Service Definition Language|*.wsdl"; ;
            fd.Multiselect = false;

            DialogResult dr = fd.ShowDialog();

            if (dr == DialogResult.OK)
                tb_wsdl.Text = fd.FileName;
        }

        private void cb_wiki_toggled(object sender, EventArgs e)
        {
            tb_wiki_user.Enabled = (cb_wiki.CheckState == CheckState.Checked) ? true : false;
            tb_wiki_password.Enabled = (cb_wiki.CheckState == CheckState.Checked) ? true : false;
            cb_merge.Enabled = (cb_wiki.CheckState == CheckState.Checked) ? true : false;
        }

        
    }
}
