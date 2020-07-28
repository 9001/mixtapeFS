using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace mixtapeFS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}

/*
 * https://github.com/dokan-dev/dokany
 * https://github.com/dokan-dev/dokan-dotnet
 * https://www.nuget.org/packages/DokanNet/
 * https://www.nuget.org/packages/StringInterpolationBridgeStrong/
*/