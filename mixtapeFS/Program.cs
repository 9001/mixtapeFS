using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            AppDomain.CurrentDomain.AssemblyResolve += (sender, dargs) =>
            {
                // workaround for win7 custom themes
                if (dargs.Name.Contains("PresentationFramework")) return null;

                // vs2019
                if (dargs.Name.StartsWith("mixtapeFS.XmlSerializers")) return null;

                String resourceName = "mixtapeFS.lib." +
                    dargs.Name.Substring(0, dargs.Name.IndexOf(", ")) + ".dll";

                using (var stream = Assembly.GetExecutingAssembly().
                            GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

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