using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using OpenBYOND.VM;
using log4net;

namespace OpenBYOND.Client
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Utils));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            OptionSet argparser = new OptionSet() { 
                {"load-dme=", v => LoadObjectTreeFrom(v)}
            };
            argparser.Parse(args);
            using (var game = new OpenBYONDGame())
                game.Run();
        }

        private static void LoadObjectTreeFrom(string file)
        {
            log.Info("Attempting object tree load...");

            DME dme = new DME();
            dme.LoadFile(file);

            ObjectTree otr = new ObjectTree();
            foreach (string filename in dme.Files)
            {
                if(filename.EndsWith(".dm"))
                    otr.ProcessFile(filename);
            }
            Environment.Exit(0);
        }
    }
}
