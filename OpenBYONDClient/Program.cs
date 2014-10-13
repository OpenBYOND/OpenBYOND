using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;
using OpenBYOND.VM;
using BYONDWorld = OpenBYOND.World.World;
using log4net;
using OpenBYOND.World.Format;

namespace OpenBYOND.Client
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            OptionSet argparser = new OptionSet() { 
                {"load-dme=", v => LoadObjectTreeFrom(v)},
                {"load-dmm=", v => LoadDMM(v)},
            };
            argparser.Parse(args);
            using (var game = new OpenBYONDGame())
                game.Run();
        }

        private static void LoadDMM(string file)
        {
            log.InfoFormat("Attempting to load {0}...",file);

            BYONDWorld w = new BYONDWorld();
            w.Load(file, new DMMLoader());

            log.Info("MAP LOAD COMPLETE");

            Environment.Exit(0);
        }

        private static void LoadObjectTreeFrom(string file)
        {
            log.Info("Attempting object tree load...");

            DME dme = new DME();
            dme.LoadFile(file);

            ObjectTree otr = new ObjectTree();
            foreach (string filename in dme.Files)
            {
                if (filename.EndsWith(".dm"))
                    otr.ProcessFile(filename);
            }

            log.Info("TREE LOAD COMPLETE");

            Environment.Exit(0);
        }
    }
}
