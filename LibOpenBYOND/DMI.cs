using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Hjg.Pngcs;
using Hjg.Pngcs.Chunks;

namespace LibOpenBYOND
{
    public class DMI
    {
        public Dictionary<String, String> metadata = new Dictionary<String, String>(); 
        public DMI(string icon, string icon_state)
        {
            string path = Directory.GetCurrentDirectory() + "//" + @icon;
            string value = string.Empty;
            try
            {
                PngReader pngr = FileHelper.CreatePngReader(path);
                ChunksList clist = pngr.GetChunksList();
                foreach (PngChunkZTXT desc in clist.GetById("zTXt"))
                {
                    value = desc.GetVal();
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return;
            }
            string[] split = value.Split('\n');
            bool found = false;
            int indexone = 0;
            int indextwo = 1;
            foreach (string s in split)
            {

                if (s.StartsWith("state"))
                {
                    if (found)
                    {
                        indextwo = Array.IndexOf(split, s);
                        break;
                    }
                    string sub = s.Substring(9);
                    string output = sub.Split(new char[] { '"', '"' })[0];
                    if (icon_state.Equals(output))
                    {
                        indexone = Array.IndexOf(split, s);
                        found = true;
                        continue;
                    }
                        
                }

            }
            for (int i = indexone + 1; i < indextwo; i++)
            {
                string line = split[i];
                string[] splittwo = line.Split(' ');
                metadata[splittwo[0]] = splittwo[2];
            }

        }
    }
}
