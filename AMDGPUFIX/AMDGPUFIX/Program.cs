using System;
using System.Reflection;
using System.Windows.Forms;

namespace AMDGPUFIX
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Included dll
            EmbeddedAssembly.Load("AMDGPUFIX.Resources.MaterialSkin.zip", "MaterialSkin.dll");
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Program.CurrentDomain_AssemblyResolve);
            // Continue...
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Preload());
        }

        //
        // DLL Resolver
        //
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }
        //
        // End
        //
    }
}
