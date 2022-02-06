using Rpm.Productie;
using Rpm.Various;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Forms;

namespace ProductieManager
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProductieView());
        }

        /// <summary>
        /// Tells the program that the Assembly its Seeking is located in the Embedded resources By using the
        /// <see cref="Assembly.GetManifestResourceNames"/> Function To get All the Resources
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>Note that this event won't fire if the dll is in the same folder as the application (sometimes)</remarks>
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                //gets the main Assembly
                var parentAssembly = Assembly.GetExecutingAssembly();
                //args.Name will be something like this
                //[ MahApps.Metro, Version=1.1.3.81, Culture=en-US, PublicKeyToken=null ]
                //so we take the name of the Assembly (MahApps.Metro) then add (.dll) to it
                var finalname = args.Name.Substring(0, args.Name.IndexOf(',')).Replace(".resources","") + ".dll";
                //here we search the resources for our dll and get the first match
                var ResourcesList = parentAssembly.GetManifestResourceNames();
                string OurResourceName = null;
                //(you can replace this with a LINQ extension like [Find] or [First])
                for (int i = 0; i <= ResourcesList.Length - 1; i++)
                {
                    var name = ResourcesList[i];
                    if (name.ToLower().EndsWith(finalname.ToLower()))
                    {
                        //Get the name then close the loop to get the first occuring value
                        OurResourceName = name;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(OurResourceName))
                {
                    //get a stream representing our resource then load it as bytes
                    using Stream stream = parentAssembly.GetManifestResourceStream(OurResourceName);
                    //in vb.net use [ New Byte(stream.Length - 1) ]
                    //in c#.net use [ new byte[stream.Length]; ]
                    if (stream != null)
                    {
                        byte[] block = new byte[stream.Length];
                        _= stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }

                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllName = args.Name.Contains(",") ? args.Name.Substring(0, args.Name.IndexOf(',')) : args.Name.Replace(".dll", "");

            dllName = dllName.Replace(".", "_");

            //if (dllName.EndsWith("_resources")) return null;
            if (!string.IsNullOrWhiteSpace(dllName))
            {
                var parentAssembly = Assembly.GetExecutingAssembly();
                //get a stream representing our resource then load it as bytes
                using Stream stream = parentAssembly.GetManifestResourceStream(dllName);
                //in vb.net use [ New Byte(stream.Length - 1) ]
                //in c#.net use [ new byte[stream.Length]; ]
                if (stream != null)
                {
                    byte[] block = new byte[stream.Length];
                    _= stream.Read(block, 0, block.Length);
                    return Assembly.Load(block);
                }
            }
            else
            {
                return null;
            }

            return null;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
            Manager.Database?.AddLog($"{e.ExceptionObject}", MsgType.Fout);
        }
    }
}