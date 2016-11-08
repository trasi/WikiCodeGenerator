using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RB.Utils.WikiCodeGenerator.WindowsApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Count() > 0)
                Application.Run(new WikiCodeGenerator(args[0]));
            else
                Application.Run(new WikiCodeGenerator());
        }
    }
}
