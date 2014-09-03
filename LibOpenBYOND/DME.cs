using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace OpenBYOND
{
    /// <summary>
    /// Class for storing which files are loaded in a project.
    /// </summary>
    public class DME
    {
        public List<string> Files = new List<string>();

        private string rootDir = "";
        private static readonly ILog log = LogManager.GetLogger(typeof(DME));

        /// <summary>
        /// Load a DME file.
        /// </summary>
        /// <param name="filename">DME file to load.</param>
        public void LoadFile(string filename)
        {
            rootDir = Path.GetDirectoryName(filename);
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException(String.Format("DME {0} not found.", filename));
            }
            log.DebugFormat("Loading {0} ({1})...", filename, rootDir);
            using (StreamReader rdr = File.OpenText(filename))
            {
                string line;
                while (rdr.Peek() >= 0)
                {
                    line = rdr.ReadLine();
                    if (line.StartsWith("#include"))
                        parseInclude(line);
                }
            }
            log.DebugFormat("Loaded {0} files.", Files.Count);
        }

        public void parseInclude(string line)
        {
            bool inString = false;
            string filename = "";
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inString = !inString;
                    if (!inString)
                    {
                        string filepath = Path.Combine(rootDir, filename.Replace('\\', Path.DirectorySeparatorChar));
                        filepath = Path.GetFullPath(filepath);
                        log.DebugFormat("Added file {0} to DME.", filepath);
                        Files.Add(filepath);
                        filename = "";
                    }
                    continue;
                }
                else
                {
                    if (inString)
                        filename += c;
                }
            }
        }
    }
}
